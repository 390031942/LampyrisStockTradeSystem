using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenTK.Compute.OpenCL;
using System;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;

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
    private BrowserSystem m_browserBuy = new BrowserSystem();
    private BrowserSystem m_browserSell = new BrowserSystem();
    private BrowserSystem m_browserRevoke = new BrowserSystem();
    private BrowserSystem m_browserQuery = new BrowserSystem();

    private HttpClient m_httpClient = new HttpClient();

    private List<BrowserSystem> m_browserList = new List<BrowserSystem>();

    // 是不是初始化浏览器
    private bool m_isInit = false;

    // 是不是登录了交易系统
    private bool m_isLoggedIn = false;

    public bool isInit => m_isInit;

    public bool isLoggedIn => m_isLoggedIn;

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

    public EastMoneyTradeManager()
    {
        m_browserList.Add(m_browserBuy);
        m_browserList.Add(m_browserSell);
        m_browserList.Add(m_browserRevoke);
        m_browserList.Add(m_browserQuery);
    }

    private void DoSomethingAfterLoginSuccess()
    {
        m_positionUpdateTaskCancellation = false;
        m_revokeUpdateTaskCancellation = false;

        WidgetManagement.GetWidget<EastMoneyTradeLoginWindow>().isOpened = false;

        MessageBox msgBox = (MessageBox)WidgetManagement.GetWidget<MessageBox>();
        msgBox.SetContent("交易登录", "登陆成功，准备吃巨肉");

        // 登陆成功后，右上角会有 退出按钮
        m_browserBuy.WaitElement(By.XPath("//p[@class='pr10 lh40']/span/a[contains(text(), '退出')]"), 5);

        // 从index = 1的浏览器开始请求交易网页，因为第0个已经打开并登陆交易了
        for (int i = 1; i < m_browserList.Count; i++)
        {
            // 打开网页
            m_browserList[i].Request("https://jywg.18.cn");

            // 设置Cookie，这样其它浏览器实例就不需要重新登陆了
            m_browserList[i].SetCookiesFromOther(m_browserBuy, (domain) => { return domain.Contains("18.cn"); }, ".18.cn");
        }

        var mode = (EastMoneyTradeMode)EastMoneyTradeModeSetting.Instance.activeMode;
        m_browserSell.Request(EastMoneyTradeUrlGetter.Instance[mode, EastMoneyTradeFunctionType.Sell]);

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
    }

    private void RequestTradeUrl()
    {
        m_browserBuy.Request("https://jywg.18.cn/");
        m_browserBuy.SaveImg(By.Id("imgValidCode"), "imgValidCode.png", false);
        Bitmap bitmap = (Bitmap)Bitmap.FromFile("imgValidCode.png");
        WidgetManagement.GetWidget<EastMoneyTradeLoginWindow>().SetValidCodePNGFilePath("imgValidCode.png");
    }

    [MenuItem("交易/登录")]
    public static void Login()
    {
        if (!Instance.isInit)
        {
            for (int i = 0; i < Instance.m_browserList.Count; i++)
            {
                Instance.m_browserList[i].Init();
            }
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
        var mode = (EastMoneyTradeMode)EastMoneyTradeModeSetting.Instance.activeMode;
        m_browserBuy.Request(EastMoneyTradeUrlGetter.Instance[mode, EastMoneyTradeFunctionType.Buy]);
        Thread.Sleep(100);

        By by1 = By.CssSelector($"[id*='{code}']");
        By by2 = By.Id("btnConfirm");

        m_browserBuy.Input(By.Id("stockCode"), code);
        m_browserBuy.WaitElementWithReturnValue(by1, 5)?.Click();

        string buyPrice = m_browserBuy.GetWebElement(By.Id("s" + AppSettings.Instance.bidLevel)).Text;
        m_browserBuy.Click(By.Id("iptPrice"));
        m_browserBuy.Input(By.Id("iptPrice"), OpenQA.Selenium.Keys.Control + "a");
        m_browserBuy.Input(By.Id("iptPrice"), buyPrice);
        m_browserBuy.Click(By.Id(m_ratioCode2Id[ratio]));
        m_browserBuy.Click(By.Id(m_ratioCode2Id[ratio]));

        try
        {
            m_browserBuy.Click(by2);
            m_browserBuy.Click(By.CssSelector("a[data-role='confirm'].btn_jh"));
            string info = m_browserBuy.GetText(By.ClassName("cxc_bd"));
            m_browserBuy.Click(By.Id("btnCxcConfirm"));

            WidgetManagement.GetWidget<MessageBox>().SetContent("委托买入结果", info);
        }
        catch (Exception)
        {
            WidgetManagement.GetWidget<MessageBox>().SetContent("委托买入结果", "根本买不了啊，是不是没钱了");
        }
    }

    public void ExecuteBuyByCount(string code, int count)
    {
        if (count <= 0)
        {
            WidgetManagement.GetWidget<MessageBox>().SetContent("委托买入结果", "搞笑吗，委托数量必须大于0");
            return;
        }

        By by1 = By.CssSelector($"[id*='{code}']");
        By by2 = By.Id("btnConfirm");

        m_browserBuy.Input(By.Id("stockCode"), code);
        m_browserBuy.WaitElementWithReturnValue(by1, 5)?.Click();
        m_browserBuy.Input(By.Id("iptCount"), count.ToString());

        try
        {
            m_browserBuy.Click(by2);
            m_browserBuy.Click(By.CssSelector("a[data-role='confirm'].btn_jh"));
            string info = m_browserBuy.GetText(By.ClassName("cxc_bd"));
            m_browserBuy.Click(By.Id("btnCxcConfirm"));

            WidgetManagement.GetWidget<MessageBox>().SetContent("委托买入结果", info);
        }
        catch (Exception)
        {
            WidgetManagement.GetWidget<MessageBox>().SetContent("委托买入结果", "根本买不了啊，是不是没钱了");
        }
    }

    public void ExecuteSellByRatio(string code, int ratio)
    {
        var mode = (EastMoneyTradeMode)EastMoneyTradeModeSetting.Instance.activeMode;
        m_browserSell.Request(EastMoneyTradeUrlGetter.Instance[mode, EastMoneyTradeFunctionType.Sell]);
        Thread.Sleep(100);

        By by1 = By.CssSelector($"[id*='{code}']");
        By by2 = By.Id("btnConfirm");

        m_browserSell.Input(By.Id("stockCode"), code);
        m_browserSell.WaitElementWithReturnValue(by1, 5)?.Click();
        string sellPrice = m_browserSell.GetWebElement(By.Id("b" + AppSettings.Instance.askLevel)).Text;
        m_browserSell.Click(By.Id("iptPrice"));
        m_browserSell.Input(By.Id("iptPrice"), OpenQA.Selenium.Keys.Control + "a");
        m_browserSell.Input(By.Id("iptPrice"), sellPrice);
        m_browserSell.Click(By.Id(m_ratioCode2Id[ratio]));
        m_browserSell.Click(By.Id(m_ratioCode2Id[ratio]));

        try
        {
            m_browserSell.Click(by2);
            m_browserSell.Click(By.CssSelector("a[data-role='confirm'].btn_jh"));
            string info = m_browserSell.GetText(By.ClassName("cxc_bd"));
            m_browserSell.Click(By.Id("btnCxcConfirm"));

            WidgetManagement.GetWidget<MessageBox>().SetContent("委托卖出结果", info);
        }
        catch (Exception)
        {
            WidgetManagement.GetWidget<MessageBox>().SetContent("委托卖出结果", "根本买不了啊，是不是卡bug了");
        }
    }

    public void ExecuteSellByCount(string code, int count)
    {
        if (count <= 0)
        {
            WidgetManagement.GetWidget<MessageBox>().SetContent("委托卖出结果", "搞笑吗，委托数量必须大于0");
            return;
        }

        By by1 = By.CssSelector($"[id*='{code}']");
        By by2 = By.Id("btnConfirm");

        m_browserSell.Input(By.Id("stockCode"), code);
        m_browserSell.WaitElementWithReturnValue(by1, 5)?.Click();
        m_browserSell.Input(By.Id("iptCount"), count.ToString());

        try
        {
            m_browserSell.Click(by2);
            m_browserSell.Click(By.CssSelector("a[data-role='confirm'].btn_jh"));
            string info = m_browserSell.GetText(By.ClassName("cxc_bd"));
            m_browserSell.Click(By.Id("btnCxcConfirm"));

            WidgetManagement.GetWidget<MessageBox>().SetContent("委托卖出结果", info);
        }
        catch (Exception)
        {
            WidgetManagement.GetWidget<MessageBox>().SetContent("委托卖出结果", "根本买不了啊，是不是卡bug了");
        }
    }

    // 执行撤单指令，返回撤单的详情，如果撤单失败(如委托号不存在)，则返回null
    public EastMoneyRevokeStockInfo ExecuteRevoke(int orderId)
    {
        m_shouldPauseRevokeUpdate = true;
        var mode = (EastMoneyTradeMode)EastMoneyTradeModeSetting.Instance.activeMode;
        m_browserRevoke.Request(EastMoneyTradeUrlGetter.Instance[mode, EastMoneyTradeFunctionType.Revoke]);
        Thread.Sleep(110);

        // 定位持仓股票信息 表格元素
        IWebElement tableElement = m_browserRevoke.GetWebElement(By.Id("tabBody"));
        // 获取所有的行元素
        IList<IWebElement> tableRows = tableElement.FindElements(By.TagName("tr"));

        foreach (var row in tableRows)
        {
            // 对于每一行，获取所有的列元素
            IList<IWebElement> rowTds = row.FindElements(By.TagName("td"));
            int id = ConvertUtil.SafeParse<int>(rowTds[11].Text);

            if (orderId == id)
            {
                rowTds[12].FindElement(By.TagName("button"))?.Click();
                m_browserRevoke.Click(By.Id("btnCxcConfirm"));

                m_shouldPauseRevokeUpdate = false;

                return new EastMoneyRevokeStockInfo()
                {
                    timeString = rowTds[1].Text,
                    stockCode = rowTds[2].FindElement(By.TagName("a")).Text,
                    stockName = rowTds[3].FindElement(By.TagName("a")).Text,
                    isBuy = rowTds[4].Text.Contains("买"),
                    orderCount = ConvertUtil.SafeParse<int>(rowTds[5].Text),
                    status = rowTds[6].Text,
                    orderPrice = ConvertUtil.SafeParse<float>(rowTds[7].Text),
                    dealCount = ConvertUtil.SafeParse<int>(rowTds[8].Text),
                    dealMoney = ConvertUtil.SafeParse<float>(rowTds[9].Text),
                    dealPrice = ConvertUtil.SafeParse<float>(rowTds[10].Text),
                    id = ConvertUtil.SafeParse<int>(rowTds[11].Text),
                };
            }
        }

        m_shouldPauseRevokeUpdate = false;

        return null;
    }

    private void HandleLoginButtonClick(string validCode)
    {
        try
        {
            Instance.m_browserBuy.Click(By.CssSelector(".btn-orange.vbtn-confirm"));
        }
        catch (Exception) { }

        m_browserBuy.Input(By.Id("txtZjzh"), TradeLoginInfo.Instance.account);
        m_browserBuy.Input(By.Id("txtPwd"), TradeLoginInfo.Instance.password);
        m_browserBuy.Input(By.Id("txtValidCode"), validCode);
        m_browserBuy.Click(By.Id("rdsc45"));
        m_browserBuy.Click(By.Id("btnConfirm"));

        try
        {
            // if (!string.IsNullOrEmpty(HKLinkTradeManager.Instance.m_browser.GetText(By.Id("ertips"))))
            if (m_browserBuy.WaitElement(By.Id("ertips"), 1))
            {
                if (!string.IsNullOrEmpty(m_browserBuy.GetText(By.Id("ertips"))))
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
            //DoSomethingAfterLoginSuccess();
        }
    }

    public void ExecutePositionUpdate() 
    {
        var mode = (EastMoneyTradeMode)EastMoneyTradeModeSetting.Instance.activeMode;
        m_browserQuery.Request(EastMoneyTradeUrlGetter.Instance[mode, EastMoneyTradeFunctionType.Position]);
        m_browserRevoke.Request(EastMoneyTradeUrlGetter.Instance[mode, EastMoneyTradeFunctionType.Revoke]);

        Thread.Sleep(100);

        HandlePositionUpdate();
    }

    private void HandlePositionUpdate()
    {
        // 定位资产表格元素
        IWebElement zichanElement = m_browserQuery.GetWebElement(By.ClassName("zichan"));

        m_positionInfo = new EastMoneyPositionInfo();

        m_positionInfo.totalMoney = zichanElement.FindElement(By.XPath("//span[text()='总资产']/following-sibling::span[1]")).Text;
        m_positionInfo.positionMoney = zichanElement.FindElement(By.XPath("//span[text()='总市值']/following-sibling::span[1]")).Text;
        m_positionInfo.positionProfitLose = zichanElement.FindElement(By.XPath("//span[text()='持仓盈亏']/following-sibling::span[1]")).Text;
        m_positionInfo.todayProfitLose = zichanElement.FindElement(By.XPath("//span[text()='当日盈亏']/following-sibling::span[1]")).Text;
        m_positionInfo.canUseMoney = zichanElement.FindElement(By.XPath("//span[text()='可用资金']/following-sibling::span[1]")).Text;


        // 定位持仓股票信息 表格元素
        IWebElement tableElement = m_browserQuery.GetWebElement(By.Id("tabBody"));
        // 获取所有的行元素
        IList<IWebElement> tableRows = tableElement.FindElements(By.TagName("tr"));

        foreach (var row in tableRows)
        {
            // 对于每一行，获取所有的列元素
            IList<IWebElement> rowTds = row.FindElements(By.TagName("td"));

            if (rowTds.Count < 11 || Convert.ToInt32(rowTds[2].Text) <= 0)
                continue;

            m_positionInfo.stockInfos.Add(new EastMoneyPositionStockInfo()
            {
                stockCode = rowTds[0].FindElement(By.TagName("a")).Text,
                stockName = rowTds[1].FindElement(By.TagName("a")).Text,
                count = rowTds[2].Text,
                useableCount = rowTds[3].Text,
                costPrice = rowTds[4].Text,
                currentPrice = rowTds[5].Text,
                money = rowTds[6].Text,
                profitLose = rowTds[7].Text,
                profitLoseRatio = rowTds[8].Text,
                todayProfitLost = rowTds[9].Text,
                todayProfitLostRatio = rowTds[10].Text,
            });
        }
        LifecycleManager.Instance.Get<EventManager>().RaiseEvent(EventType.PositionUpdate, m_positionInfo);
    }

    private void HandleRevokeUpdate()
    {
        // 定位持仓股票信息 表格元素
        IWebElement tableElement = m_browserRevoke.GetWebElement(By.Id("tabBody"));
        // 获取所有的行元素
        IList<IWebElement> tableRows = tableElement.FindElements(By.TagName("tr"));

        m_revokeInfo = new EastMoneyRevokeInfo();

        foreach (var row in tableRows)
        {
            // 对于每一行，获取所有的列元素
            IList<IWebElement> rowTds = row.FindElements(By.TagName("td"));

            if (rowTds.Count < 13 || Convert.ToInt32(rowTds[2].Text) <= 0)
                continue;

            m_revokeInfo.stockInfos.Add(new EastMoneyRevokeStockInfo()
            {
                timeString = rowTds[1].Text,
                stockCode = rowTds[2].FindElement(By.TagName("a")).Text,
                stockName = rowTds[3].FindElement(By.TagName("a")).Text,
                isBuy = rowTds[4].Text.Contains("买"),
                orderCount = ConvertUtil.SafeParse<int>(rowTds[5].Text),
                status = rowTds[6].Text,
                orderPrice = ConvertUtil.SafeParse<float>(rowTds[7].Text),
                dealCount = ConvertUtil.SafeParse<int>(rowTds[8].Text),
                dealMoney = ConvertUtil.SafeParse<float>(rowTds[9].Text),
                dealPrice = ConvertUtil.SafeParse<float>(rowTds[10].Text),
                id = ConvertUtil.SafeParse<int>(rowTds[11].Text),
            });
        }
        LifecycleManager.Instance.Get<EventManager>().RaiseEvent(EventType.RevokeUpdate, m_revokeInfo);
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
        m_browserList.ForEach((browser) => { browser.Close(); });
    }

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
        var cookies = Instance.m_browserBuy.GetCookies();
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

