using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
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

public class EastMoneyTradeManager:Singleton<EastMoneyTradeManager>
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
    private BrowserSystem m_browser = new BrowserSystem();

    private BrowserSystem m_browser1 = new BrowserSystem();

    // 是不是初始化浏览器
    private bool m_isInit = false;

    // 是不是登录了交易系统
    private bool m_isLoggedIn = false;

    public bool isInit => m_isInit;

    public bool isLoggedIn => m_isLoggedIn;

    // 持仓更新定时器
    private int m_positionUpdateTimer = -1;

    // 撤单更新定时器
    private int m_revokeUpdateTimer = -1;

    private static void DoSomethingAfterLoginSuccess()
    {
        WidgetManagement.GetWidget<EastMoneyTradeLoginWindow>().isOpened = false;
        MessageBox msgBox = (MessageBox)WidgetManagement.GetWidget<MessageBox>();
        msgBox.SetContent("交易登录", "登陆成功，准备吃巨肉");

        // 登陆成功后，右上角会有 退出按钮
        Instance.m_browser.WaitElement(By.XPath("//p[@class='pr10 lh40']/span/a[contains(text(), '退出')]"), 5);

        // 根据交易功能数量 打开多个网页窗口
        var mode = (EastMoneyTradeMode)EastMoneyTradeModeSetting.Instance.activeMode;
        for(int i = 0; i < (int)EastMoneyTradeFunctionType.Count;i++)
        {
            Instance.m_browser.OpenNewWindow(EastMoneyTradeUrlGetter.Instance[mode, (EastMoneyTradeFunctionType)i]);
        }

        Instance.m_browser1.Request("https://jywg.18.cn");

        Instance.m_browser1.SetCookiesFromOther(Instance.m_browser, (domain) => { return domain.Contains("18.cn");  },".18.cn");

        for (int i = 0; i < (int)EastMoneyTradeFunctionType.Count; i++)
        {
            Instance.m_browser1.OpenNewWindow(EastMoneyTradeUrlGetter.Instance[mode, (EastMoneyTradeFunctionType)i]);
        }

        // 关闭A股的买入页面，登录以后会默认跳转这个页面
        Instance.m_browser.CloseFirstWindowByUrl("https://jywg.18.cn/Trade/Buy");

        // 构造定时器
        Instance.m_positionUpdateTimer = CallTimer.Instance.SetInterval(() =>
        {
            Instance.m_browser.SwitchToUrl(EastMoneyTradeUrlGetter.Instance[mode, EastMoneyTradeFunctionType.Position]);

            var positionInfo = new EastMoneyPositionInfo();

            // 定位资产表格元素
            IWebElement zichanElement = Instance.m_browser.GetWebElement(By.ClassName("zichan"));

            positionInfo.totalMoney = zichanElement.FindElement(By.XPath("//span[text()='总资产']/following-sibling::span[1]")).Text;
            positionInfo.positionMoney = zichanElement.FindElement(By.XPath("//span[text()='总市值']/following-sibling::span[1]")).Text;
            positionInfo.positionProfitLose = zichanElement.FindElement(By.XPath("//span[text()='持仓盈亏']/following-sibling::span[1]")).Text;
            positionInfo.todayProfitLose = zichanElement.FindElement(By.XPath("//span[text()='当日盈亏']/following-sibling::span[1]")).Text;
            positionInfo.canUseMoney = zichanElement.FindElement(By.XPath("//span[text()='可用资金']/following-sibling::span[1]")).Text;

            // 定位持仓股票信息 表格元素
            IWebElement tableElement = Instance.m_browser.GetWebElement(By.Id("tabBody"));
            // 获取所有的行元素
            IList<IWebElement> tableRows = tableElement.FindElements(By.TagName("tr"));


            foreach (var row in tableRows)
            {
                // 对于每一行，获取所有的列元素
                IList<IWebElement> rowTds = row.FindElements(By.TagName("td"));

                positionInfo.stockInfos.Add(new EastMoneyPositionStockInfo()
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
            LifecycleManager.Instance.Get<EventManager>().RaiseEvent(EventType.PositionUpdate, positionInfo);
        }, 3000);

        // Instance.m_revokeUpdateTimer = CallTimer.Instance.SetInterval(() =>
        // {
        //     Instance.m_browser.SwitchToUrl(EastMoneyTradeUrlGetter.Instance[mode, EastMoneyTradeFunctionType.Revoke]);
        // 
        //     LifecycleManager.Instance.Get<EventManager>().RaiseEvent(EventType.RevokeUpdate, new object[] { });
        // }, 1000);

        // 三小时后登陆失效,10800000 = 3 * 3600 * 1000ms 
        CallTimer.Instance.SetInterval(() =>
        {
            // 记录登陆状态，抛出事件，并关闭所有浏览器窗口,回到登录界面
            WidgetManagement.GetWidget<MessageBox>().SetContent("交易系统提醒", "您已经登录了超过3个小时，该重新登陆了!");
            Instance.m_isLoggedIn = false;
            LifecycleManager.Instance.Get<EventManager>().RaiseEvent(EventType.LoginStateChanged, false);
        }, 10800000);

        // 记录登陆状态
        Instance.m_isLoggedIn = true;
        LifecycleManager.Instance.Get<EventManager>().RaiseEvent(EventType.LoginStateChanged, true);
    }

    private static void RequestTradeUrl()
    {
        Instance.m_browser.Request("https://jywg.18.cn/Login?el=1&clear=&returl=%2fTrade%2fBuy");
        Instance.m_browser.SaveImg(By.Id("imgValidCode"), "imgValidCode.png", false);
        Bitmap bitmap = (Bitmap)Bitmap.FromFile("imgValidCode.png");
        WidgetManagement.GetWidget<EastMoneyTradeLoginWindow>().SetValidCodePNGFilePath("imgValidCode.png");
    }

    [MenuItem("交易/登录")]
    public static void Login()
    {
        if (!Instance.isInit)
        {
            Instance.m_browser.Init();
            Instance.m_browser1.Init();

            Instance.m_isInit = true;

            LifecycleManager.Instance.Get<EventManager>().AddEventHandler(EventType.LoginButtonClicked, (object[] parameters) => 
            {
                try
                {
                    Instance.m_browser.Click(By.CssSelector(".btn-orange.vbtn-confirm"));
                }
                catch(Exception) { }
                
                Instance.m_browser.Input(By.Id("txtZjzh"), TradeLoginInfo.Instance.account);
                Instance.m_browser.Input(By.Id("txtPwd"), TradeLoginInfo.Instance.password);
                Instance.m_browser.Input(By.Id("txtValidCode"), (string)parameters[0]);
                Instance.m_browser.Click(By.Id("rdsc45"));
                Instance.m_browser.Click(By.Id("btnConfirm"));

                try
                {
                    // if (!string.IsNullOrEmpty(HKLinkTradeManager.Instance.m_browser.GetText(By.Id("ertips"))))
                    if(Instance.m_browser.WaitElement(By.Id("ertips"),1))
                    {
                        if(!string.IsNullOrEmpty(Instance.m_browser.GetText(By.Id("ertips"))))
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
                    DoSomethingAfterLoginSuccess();
                }
            });
;        }

        RequestTradeUrl();
    }

    [MenuItem("交易/尝试购买")]
    public static void TryBuy()
    {
        Instance.ExecuteBuyByRatio("600000", 1);
    }

    public void ExecuteBuyByRatio(string code,int ratio)
    {
        By by1 = By.CssSelector($"[id*='{code}']");
        By by2 = By.Id("btnConfirm");

        Instance.m_browser.Input(By.Id("stockCode"), code);
        Instance.m_browser.WaitElementWithReturnValue(by1, 5)?.Click();
 
        Instance.m_browser.Click(By.Id(m_ratioCode2Id[ratio]));

        try
        {
            Instance.m_browser.Click(by2);
            Instance.m_browser.Click(By.CssSelector("a[data-role='confirm'].btn_jh"));
            string info = Instance.m_browser.GetText(By.ClassName("cxc_bd"));
            Instance.m_browser.Click(By.Id("btnCxcConfirm"));

            WidgetManagement.GetWidget<MessageBox>().SetContent("委托买入结果", info);
        }
        catch(Exception)
        {
            WidgetManagement.GetWidget<MessageBox>().SetContent("委托买入结果", "根本买不了啊，是不是没钱了");
        }
    }
}