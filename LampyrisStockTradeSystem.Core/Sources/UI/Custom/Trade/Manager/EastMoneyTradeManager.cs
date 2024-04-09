/*
** Author: wushuhong
** Contact: gameta@qq.com
** Description: 东方财富通交易管理器
*/
using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using OpenTK.Compute.OpenCL;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace LampyrisStockTradeSystem;

public enum EastMoneyTradeMode
{
    Normal = 0, // 普通沪深A股模式
    HKLink = 1,// 港股通模式
    MarginTrade = 2, // 融资融券模式(暂不支持)
    Count = 3 // 数量
}

public class EastMoneyTradeModeName:Singleton<EastMoneyTradeModeName>
{
    public string this[EastMoneyTradeMode mode]
    {
        get
        {
            switch(mode)
            {
                case EastMoneyTradeMode.Normal:
                    return "沪深A股";
                case EastMoneyTradeMode.HKLink:
                    return "港股通";
                case EastMoneyTradeMode.MarginTrade:
                    return "融资融券";
                default:
                    return "";
            }
        }
    }
}
public enum EastMoneyTradeFunctionType
{
    Position = 0, // 持仓
    Buy = 1,// 买入
    Sell = 2, // 卖出
    Revoke = 3, // 撤单
    TodayOrder = 4, // 当日委托
    TodayDeal = 5, // 当日成交
    HistoryOrder = 6, // 历史委托
    HistoryDeal = 7, // 历史成交
    Count = 8, 
}

public enum EastMonsterTradeWaitTaskStatus
{
    Done = 0,
    NotReady = 1,
    Failed = 2,
}

public interface IEastMonsterTradeWaitTask
{
    public EastMonsterTradeWaitTaskStatus GetStatus();

    public void Execute();
}

public class EastMonsterTradeHKLinkBuyWaitTask : IEastMonsterTradeWaitTask
{
    private string m_code;
    private int m_ratio;

    // 查询买卖五档，最大可买，最小股数
    private Task<HttpResponseMessage> m_queryAskBidTask;
    private Task<HttpResponseMessage> m_queryMaxCanBuy;
    private Task<HttpResponseMessage> m_queryMinUnit;

    public EastMonsterTradeHKLinkBuyWaitTask(string code,int ratio,
                                             Task<HttpResponseMessage> queryAskBidTask, 
                                             Task<HttpResponseMessage> queryMaxCanBuy, 
                                             Task<HttpResponseMessage> queryMinUnit)
    {
        m_code = code;
        m_ratio = ratio;
        m_queryAskBidTask = queryAskBidTask;
        m_queryMaxCanBuy = queryMaxCanBuy;
        m_queryMinUnit = queryMinUnit;
    }

    public void Execute()
    {
        try
        {
            m_queryAskBidTask.Result.EnsureSuccessStatusCode();
            m_queryMaxCanBuy.Result.EnsureSuccessStatusCode();
            m_queryMinUnit.Result.EnsureSuccessStatusCode();

            string askBidResultJson = m_queryAskBidTask.Result.Content.ReadAsStringAsync().Result;
            string canBuyResultJson = m_queryMaxCanBuy.Result.Content.ReadAsStringAsync().Result;
            string minUnitResultJson = m_queryMinUnit.Result.Content.ReadAsStringAsync().Result;

            string askBidResultStrippedJson = JsonStripperUtil.GetEastMoneyStrippedJson(askBidResultJson);
            float price = JObject.Parse(askBidResultStrippedJson)["fivequote"]["buy" + AppSettings.Instance.bidLevel].SafeToObject<float>();
            float money = JObject.Parse(canBuyResultJson)["Data"][0]["AvailableMoney"].SafeToObject<float>();
            int minUnit = JObject.Parse(minUnitResultJson)["Data"][0]["Szxdw"].SafeToObject<int>();

            float count = (money / price) / m_ratio;
            int shouldBuyUnitCount = (int)Math.Floor(count / minUnit) * minUnit;

            var validateKey = EastMoneyTradeManager.Instance.validateKey;
            var httpClient = EastMoneyTradeManager.Instance.httpClient;

            var url = "https://jywg.18.cn/HKTrade/SubmitTrade?validateKey={validateKey}";
            var requestBody = new StringContent($"stockCode={m_code}&price={price}&amount={shouldBuyUnitCount}&tradeType=3B", Encoding.UTF8, "application/x-www-form-urlencoded");
            var response = httpClient.PostAsync(url, requestBody).Result;
            response.EnsureSuccessStatusCode();

            var resultStr = response.Content.ReadAsStringAsync().Result;

            WidgetManagement.GetWidget<MessageBox>().SetContent("买入结果提示", "委托成功");
        }
        catch (Exception ex)
        {
            WidgetManagement.GetWidget<MessageBox>().SetContent("买入结果提示", "报错了:" + ex.ToString());
        }
    }

    public EastMonsterTradeWaitTaskStatus GetStatus()
    {
        if (m_queryAskBidTask == null || m_queryMaxCanBuy == null || m_queryMinUnit == null)
            return EastMonsterTradeWaitTaskStatus.Failed;

        if(m_queryAskBidTask.Exception != null || m_queryMaxCanBuy.Exception != null || m_queryMinUnit.Exception != null)
            return EastMonsterTradeWaitTaskStatus.Failed;

        return (m_queryAskBidTask.IsCompleted && m_queryMaxCanBuy.IsCompleted && m_queryMinUnit.IsCompleted) ? 
            EastMonsterTradeWaitTaskStatus.Done : EastMonsterTradeWaitTaskStatus.NotReady;
    }
}

// 不同EastMoneyTradeMode下，不同的交易功能 有不同的请求url，这里用一个字典保存
public class EastMoneyTradeUrlGetter:Singleton<EastMoneyTradeUrlGetter>
{
    private Dictionary<EastMoneyTradeMode, Dictionary<EastMoneyTradeFunctionType, string>> m_map = new Dictionary<EastMoneyTradeMode, Dictionary<EastMoneyTradeFunctionType, string>>()
    {
        { EastMoneyTradeMode.Normal,new Dictionary<EastMoneyTradeFunctionType, string>()
        {
            { EastMoneyTradeFunctionType.Position,"https://jywg.18.cn/Search/Position" },
            { EastMoneyTradeFunctionType.Buy,"https://jywg.18.cn/Trade/Buy" },
            { EastMoneyTradeFunctionType.Sell,"https://jywg.18.cn/Trade/Sale" },
            { EastMoneyTradeFunctionType.Revoke,"https://jywg.18.cn/Trade/Revoke" },
            { EastMoneyTradeFunctionType.TodayOrder,"https://jywg.18.cn/Search/Orders" },
            { EastMoneyTradeFunctionType.TodayDeal,"https://jywg.18.cn/Search/Deal" },
            { EastMoneyTradeFunctionType.HistoryOrder,"https://jywg.18.cn/Search/HisOrders" },
            { EastMoneyTradeFunctionType.HistoryDeal,"https://jywg.18.cn/Search/HisDeal" },
        } },

        { EastMoneyTradeMode.HKLink,new Dictionary<EastMoneyTradeFunctionType, string>()
        {
            { EastMoneyTradeFunctionType.Position,"https://jywg.18.cn/Search/Position" },
            { EastMoneyTradeFunctionType.Buy,"https://jywg.18.cn/HKTrade/HKBuy" },
            { EastMoneyTradeFunctionType.Sell,"https://jywg.18.cn/HKTrade/HKSale" },
            { EastMoneyTradeFunctionType.Revoke,"https://jywg.18.cn/HKTrade/Revoke" },
            { EastMoneyTradeFunctionType.TodayOrder,"https://jywg.18.cn/HKTrade/QueryTodayOrder" },
            { EastMoneyTradeFunctionType.TodayDeal,"https://jywg.18.cn/HKTrade/QueryTodayDeal" },
            { EastMoneyTradeFunctionType.HistoryOrder,"https://jywg.18.cn/HKTrade/QueryHistoryOrder" },
            { EastMoneyTradeFunctionType.HistoryDeal,"https://jywg.18.cn/HKTrade/QueryHistoryDeal" },
        } },
    };

    public string this[EastMoneyTradeMode mode, EastMoneyTradeFunctionType type]
    {
        get
        {
            if(m_map.ContainsKey(mode))
            {
                var subMap = m_map[mode];
                if(subMap.ContainsKey(type))
                {
                    return subMap[type];
                }
            }
            return "";
        }
    }
}
public class EastMoneyTradeManager : Singleton<EastMoneyTradeManager>, ILifecycle
{
    // 东方财富网页交易上的仓位选择，分别对应: 全仓，1/2仓, 1/3仓，1/4仓
    private static Dictionary<int, string> m_ratioCode2Id = new Dictionary<int, string>()
    {
        {1,"radall" },
        {2,"radtwo" },
        {3,"radstree" },
        {4,"radfour" },
    };

    // 浏览器模拟操作
    private BrowserSystem m_browserLogin = new BrowserSystem();

    private HttpClient m_httpClient = new HttpClient();

    public HttpClient httpClient => m_httpClient;

    // 是不是初始化浏览器
    private bool m_isInit = false;

    // 是不是登录了交易系统
    private bool m_isLoggedIn = false;

    public bool isInit => m_isInit;

    public bool isLoggedIn => m_isLoggedIn;

    // 请求参数的validateKey
    private string m_validateKey;

    public string validateKey => m_validateKey;

    // 持仓更新 任务控制
    private bool m_positionUpdateTaskCancellation;

    // 撤单更新 任务控制
    private bool m_revokeUpdateTaskCancellation;

    // 是否要 暂停更新撤单信息，当有撤单操作时为true
    private bool m_shouldPauseRevokeUpdate = false;

    // 循环申报处理定时器
    private int m_circularOrderHnadleTimer = -1;

    private EastMoneyPositionInfo m_positionInfo = new EastMoneyPositionInfo();

    private EastMoneyRevokeInfo m_revokeInfo = new EastMoneyRevokeInfo();

    private Queue<EastMoneyCircularOrderInfo> m_circularOrderQueue = new Queue<EastMoneyCircularOrderInfo>();

    private List<IEastMonsterTradeWaitTask> m_tradeWaitTask = new List<IEastMonsterTradeWaitTask>();

    private List<IEastMonsterTradeWaitTask> m_needRemoveTaskList = new List<IEastMonsterTradeWaitTask>();

    private void OnLoginSuccess(bool withSerializedCookie = false)
    {
        m_positionUpdateTaskCancellation = false;
        m_revokeUpdateTaskCancellation = false;
        m_validateKey = m_browserLogin.GetWebElement(By.CssSelector("input[type='hidden']")).GetAttribute("value");
        WidgetManagement.GetWidget<EastMoneyTradeLoginWindow>().isOpened = false;

        var cookies = m_browserLogin.GetCookies();
        m_httpClient = new HttpClient(CookieUtil.UseHttpClientWithSeleniumCookies(cookies));
        m_httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        m_httpClient.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue("zh-CN"));
        m_httpClient.DefaultRequestHeaders.Add("sec-ch-ua", "\"Google Chrome\";v=\"123\", \"Not:A-Brand\";v=\"8\", \"Chromium\";v=\"123\"");
        m_httpClient.DefaultRequestHeaders.Add("sec-ch-ua-mobile", "?0");
        m_httpClient.DefaultRequestHeaders.Add("sec-ch-ua-platform", "\"Windows\"");
        m_httpClient.DefaultRequestHeaders.Add("sec-fetch-dest", "empty");
        m_httpClient.DefaultRequestHeaders.Add("sec-fetch-mode", "cors");
        m_httpClient.DefaultRequestHeaders.Add("sec-fetch-site", "same-origin");
        m_httpClient.DefaultRequestHeaders.Add("x-requested-with", "XMLHttpRequest");
        m_httpClient.DefaultRequestHeaders.Add("gw_reqtimestamp", "1712479062234");
        m_httpClient.DefaultRequestHeaders.Referrer = new Uri("https://jywg.18.cn/HKTrade/HKBuy");

        m_browserLogin.Close();

        // HandlePositionUpdate();
        // HandleRevokeUpdate();

        // 三小时后登陆失效,10800000 = 3 * 3600 * 1000ms 
        CallTimer.Instance.SetInterval(() =>
        {
            // 记录登陆状态，抛出事件，并关闭所有浏览器窗口,回到登录界面
            WidgetManagement.GetWidget<MessageBox>().SetContent("交易系统提醒", "您已经登录了超过3个小时，该重新登陆了!");
            Instance.m_isLoggedIn = false;
            LifecycleManager.Instance.Get<EventManager>().RaiseEvent(EventType.LoginStateChanged, false);
            ShutDownUpdateTask();
        }, 10800000);

        // 记录登陆状态
        m_isLoggedIn = true;
        LifecycleManager.Instance.Get<EventManager>().RaiseEvent(EventType.LoginStateChanged, true);

        WidgetManagement.GetWidget<MessageBox>().SetContent("交易系统提醒", "登陆成功，准备吃巨面!");
    }

    private void RequestTradeUrl()
    {
        m_browserLogin.Request("https://jywg.18.cn/");
        m_browserLogin.SaveImg(By.Id("imgValidCode"), "imgValidCode.png", false);
        Bitmap bitmap = (Bitmap)Bitmap.FromFile("imgValidCode.png");
        WidgetManagement.GetWidget<EastMoneyTradeLoginWindow>().SetValidCodePNGFilePath("imgValidCode.png");
    }

    [MenuItem("交易/登录")]
    public static void Login()
    {
        // TODO:检查登录Cookie是否过时
        // if (CookieManager.Instance.HasValidCookie(CookieType.EastMoneyTrade))
        // {
        //     Instance.m_httpClient = CookieManager.Instance.GetHttpClientWithCookieType(CookieType.EastMoneyTrade);
        //     return;
        // }

        if (!Instance.isInit)
        {
            Instance.m_browserLogin.Init();
            Instance.m_isInit = true;

            LifecycleManager.Instance.Get<EventManager>().AddEventHandler(EventType.LoginButtonClicked, (object[] parameters) =>
            {
                Instance.HandleLoginButtonClick((string)parameters[0]);
            }
            );
        }

        if (!Instance.isLoggedIn)
        {
            Instance.RequestTradeUrl();
        }
        else
        {
            WidgetManagement.GetWidget<MessageBox>().SetContent("交易登陆", "你已经登陆了，不需要再次登陆等着吃肉吧");
        }
    }

    public void ExecuteBuyByRatio(string code, int ratio)
    {
        // 请求买卖五档行情
        var urlAskBid = $"https://hkmarketzp.eastmoney.com/api/HKQuoteSnapshot?id=HK|{code}&auth=5&type=1&DC_APP_KEY=dcquotes-service-tweb&DC_TIMESTAMP=1712677182450&DC_SIGN=3C8A0614551E0CC5BF6185D5ACCFA030&callback=jQuery18307597280834087143_1712677174833&_=1712677182450";
        var maxCanBuy = $"https://jywg.18.cn/HKTrade/GetHKTradeTip?validatekey={m_validateKey}";
        var minUnit = $"https://jywg.18.cn/Com/GetZqInfo?validatekey={m_validateKey}";

        var minUnitContent = new StringContent($"zqdm={code}&market=5", Encoding.UTF8, "application/x-www-form-urlencoded");

        var waitTask = new EastMonsterTradeHKLinkBuyWaitTask(code,ratio,
                                                             m_httpClient.GetAsync(urlAskBid), 
                                                             m_httpClient.PostAsync(maxCanBuy, null), 
                                                             m_httpClient.PostAsync(minUnit, minUnitContent));

        m_tradeWaitTask.Add(waitTask);
    }

    public void ExecuteBuyByCount(string code, int count)
    {
        if (count <= 0)
        {
            WidgetManagement.GetWidget<MessageBox>().SetContent("委托买入结果", "搞笑吗，委托数量必须大于0");
            return;
        }
    }

    public void ExecuteSellByRatio(string code, int ratio)
    {
        
    }

    public void ExecuteSellByCount(string code, int count)
    {
       
    }

    // 执行撤单指令，返回撤单的详情，如果撤单失败(如委托号不存在)，则返回null
    public EastMoneyRevokeStockInfo ExecuteRevoke(int orderId)
    {
        return null;
    }

    private void HandleLoginButtonClick(string validCode)
    {
        try
        {
            m_browserLogin.Click(By.CssSelector(".btn-orange.vbtn-confirm"));
        }
        catch (Exception) { }

        m_browserLogin.Input(By.Id("txtZjzh"), TradeLoginInfo.Instance.account);
        m_browserLogin.Input(By.Id("txtPwd"), TradeLoginInfo.Instance.password);
        m_browserLogin.Input(By.Id("txtValidCode"), validCode);
        m_browserLogin.Click(By.Id("rdsc45"));
        m_browserLogin.Click(By.Id("btnConfirm"));

        try
        {
            // if (!string.IsNullOrEmpty(HKLinkTradeManager.Instance.m_browser.GetText(By.Id("ertips"))))
            if (m_browserLogin.WaitElement(By.Id("ertips"), 1))
            {
                if (!string.IsNullOrEmpty(m_browserLogin.GetText(By.Id("ertips"))))
                {
                    RequestTradeUrl();
                    WidgetManagement.GetWidget<EastMoneyTradeLoginWindow>().isWrongInfo = true;
                }
                else
                {
                    OnLoginSuccess();
                }
            }
            else
            {
                OnLoginSuccess();
            }
        }
        catch (Exception ex)
        {
            // DoSomethingAfterLoginSuccess();
        }
    }

    private void HandlePositionUpdate()
    {
        // LifecycleManager.Instance.Get<EventManager>().RaiseEvent(EventType.PositionUpdate, m_positionInfo);
    }

    private void HandleRevokeUpdate()
    {
        // LifecycleManager.Instance.Get<EventManager>().RaiseEvent(EventType.RevokeUpdate, m_revokeInfo);
    }

    private void ShutDownUpdateTask()
    {
        m_positionUpdateTaskCancellation = true;
        m_revokeUpdateTaskCancellation = true;
    }

    public void OnStart()
    {

    }

    public void OnUpdate(float dTime)
    {
        m_needRemoveTaskList.Clear();
        for (int i = 0; i < m_tradeWaitTask.Count;i++)
        {
            IEastMonsterTradeWaitTask waitTask = m_tradeWaitTask[i];
            EastMonsterTradeWaitTaskStatus status = waitTask.GetStatus();
            if(status == EastMonsterTradeWaitTaskStatus.Done)
            {
                waitTask.Execute();
                m_needRemoveTaskList.Add(waitTask);
            }
            else if (status == EastMonsterTradeWaitTaskStatus.NotReady)
            {
                continue;
            }
            else if (status == EastMonsterTradeWaitTaskStatus.Failed)
            {
                // 报错
                m_needRemoveTaskList.Add(waitTask);
            }
        }

        foreach(var task in m_needRemoveTaskList) { m_tradeWaitTask.Remove(task); }

        // 处理循环申报
        while (m_circularOrderQueue.TryDequeue(out var orderInfo))
        {
            // 先执行撤单
            EastMoneyRevokeStockInfo stockInfo = ExecuteRevoke(orderInfo.previousOrderId);

            if (stockInfo != null)
            {
                if (orderInfo.isBuy)
                {
                    ExecuteBuyByCount(stockInfo.stockCode, stockInfo.orderCount - stockInfo.dealCount);
                }
                else
                {
                    ExecuteSellByCount(stockInfo.stockCode, stockInfo.orderCount - stockInfo.dealCount);
                }
            }
        }
    }

    public void OnDestroy()
    {

    }
}

