/*
** Author: wushuhong
** Contact: gameta@qq.com
** Description: 东方财富通交易管理器
*/
using OpenQA.Selenium;
using OpenTK.Compute.OpenCL;
using System.Collections.ObjectModel;
using System.Net;
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

    // 是不是初始化浏览器
    private bool m_isInit = false;

    // 是不是登录了交易系统
    private bool m_isLoggedIn = false;

    public bool isInit => m_isInit;

    public bool isLoggedIn => m_isLoggedIn;

    // 请求参数的validateKey
    private string m_validateKey;

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

    private void DoSomethingAfterLoginSuccess()
    {
        m_positionUpdateTaskCancellation = false;
        m_revokeUpdateTaskCancellation = false;
        m_validateKey = m_browserLogin.GetWebElement(By.Id("em_validateKey")).Text;

        WidgetManagement.GetWidget<EastMoneyTradeLoginWindow>().isOpened = false;

        var cookies = Instance.m_browserLogin.GetCookies();
        m_httpClient = new HttpClient(UseHttpClientWithCookies(cookies));
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
        // m_httpClient.DefaultRequestHeaders.Referrer = new Uri("https://jywg.18.cn/Search/GetHisOrdersData");

        HandlePositionUpdate();
        HandleRevokeUpdate();

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
        // 卖三
        float price = 0;
        float amount = 0;

        // 获取档位数据

        // 获取最大可买数量
        {
            var requestBody = new StringContent($"https://jywg.18.cn/HKTrade/GetMaxTradeCount?validateKey={m_validateKey}", Encoding.UTF8, "application/x-www-form-urlencoded");

            // 发送POST请求
            var response = m_httpClient.PostAsync("https://jywg.18.cn/Search/GetHisOrdersData", requestBody).Result;

            // 确保请求成功
            response.EnsureSuccessStatusCode();

        }
   
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
                    DoSomethingAfterLoginSuccess();
                }
            }
            else
            {
                DoSomethingAfterLoginSuccess();
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

    // 将Selenium的Cookie设置到HttpClientHandler里，供HttpClient使用
    private static HttpClientHandler UseHttpClientWithCookies(ReadOnlyCollection<OpenQA.Selenium.Cookie> seleniumCookies)
    {
        var handler = new HttpClientHandler();

        var cookieContainer = new CookieContainer();
        handler.CookieContainer = cookieContainer;

        // 转换Cookies并添加到CookieContainer中
        foreach (var seleniumCookie in seleniumCookies)
        {
            System.Net.Cookie cookie = new System.Net.Cookie
            {
                Name = seleniumCookie.Name,
                Value = seleniumCookie.Value,
                Domain = seleniumCookie.Domain,
                Path = seleniumCookie.Path,
                Expires = seleniumCookie.Expiry.GetValueOrDefault(DateTime.Now.AddYears(1)), // 设置一个默认的过期时间
                Secure = seleniumCookie.Secure,
                HttpOnly = seleniumCookie.IsHttpOnly
            };
            cookieContainer.Add(cookie);
        }

        return handler;
    }

    // [MenuItem("交易/测试POST")]
    private static void TestPost()
    {
        var cookies = Instance.m_browserLogin.GetCookies();
        Instance.m_httpClient = new HttpClient(UseHttpClientWithCookies(cookies));
        var url = "https://jywg.18.cn/Search/Position";

        var client = Instance.m_httpClient;

        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        client.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue("zh-CN"));
        client.DefaultRequestHeaders.Add("sec-ch-ua", "\"Google Chrome\";v=\"123\", \"Not:A-Brand\";v=\"8\", \"Chromium\";v=\"123\"");
        client.DefaultRequestHeaders.Add("sec-ch-ua-mobile", "?0");
        client.DefaultRequestHeaders.Add("sec-ch-ua-platform", "\"Windows\"");
        client.DefaultRequestHeaders.Add("sec-fetch-dest", "empty");
        client.DefaultRequestHeaders.Add("sec-fetch-mode", "cors");
        client.DefaultRequestHeaders.Add("sec-fetch-site", "same-origin");
        client.DefaultRequestHeaders.Add("x-requested-with", "XMLHttpRequest");
        client.DefaultRequestHeaders.Add("gw_reqtimestamp", "1712479062234");
        client.DefaultRequestHeaders.Referrer = new Uri("https://jywg.18.cn/Search/GetHisOrdersData");

        // 设置请求的Body
        var requestBody = new StringContent("st=2024-03-31&et=2024-04-07&qqhs=20&dwc=", Encoding.UTF8, "application/x-www-form-urlencoded");

        // 发送POST请求
        var response = client.PostAsync("https://jywg.18.cn/Search/GetHisOrdersData", requestBody).Result;

        // 确保请求成功
        response.EnsureSuccessStatusCode();

        // 读取并打印返回的结果
        var responseBody = response.Content.ReadAsStringAsync().Result;
        Console.WriteLine(responseBody);
    }
}

