using OpenQA.Selenium;

namespace LampyrisStockTradeSystem;

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
}

public class EastMoneyTradeGetter
{

}
public class EastMoneyTradeManager:Singleton<EastMoneyTradeManager>
{
    private static Dictionary<int, string> m_ratioCode2Id = new Dictionary<int, string>()
    {
        {1,"radall" },
        {2,"radtwo" },
        {3,"radstree" },
        {4,"radfour" },
    };

    private static void DoSomethingAfterLoginSuccess()
    {
        WidgetManagement.GetWidget<EastMoneyTradeLoginWindow>().isOpened = false;
        MessageBox msgBox = (MessageBox)WidgetManagement.GetWidget<MessageBox>();
        msgBox.SetContent("交易登录", "登陆成功，准备吃巨肉");

        // 登陆成功后，右上角会有 退出按钮
        Instance.m_browser.WaitElement(By.XPath("//p[@class='pr10 lh40']/span/a[contains(text(), '退出')]"), 5);

        Instance.m_browser.OpenNewWindow("https://jywg.18.cn/Search/Position");
        Instance.m_browser.OpenNewWindow("https://jywg.18.cn/HKTrade/HKBuy");
        Instance.m_browser.OpenNewWindow("https://jywg.18.cn/HKTrade/HKSale");
        Instance.m_browser.OpenNewWindow("https://jywg.18.cn/HKTrade/Revoke");
        Instance.m_browser.OpenNewWindow("https://jywg.18.cn/HKTrade/QueryTodayDeal");

        // A股的：
        // Instance.m_browser.OpenNewWindow("https://jywg.18.cn/Trade/Sale");
        // Instance.m_browser.OpenNewWindow("https://jywg.18.cn/Trade/Revoke");
        // Instance.m_browser.OpenNewWindow("https://jywg.18.cn/Search/Deal");

        // 关闭A股的买入页面，登录以后会默认跳转这个页面
        Instance.m_browser.CloseFirstWindowByUrl("https://jywg.18.cn/Trade/Buy");

        // 构造定时器
        Instance.m_positionUpdateTimer = CallTimer.Instance.SetInterval(() =>
        {
            Instance.m_browser.OpenNewWindow("https://jywg.18.cn/Search/Position");

            var positionInfo = new EastMoneyPositionInfo();

            // 定位资产表格元素
            IWebElement zichanElement = Instance.m_browser.GetWebElement(By.Id("zichan"));

            positionInfo.totalMoney = zichanElement.FindElement(By.XPath("//span[text()='总资产']/following-sibling::span[1]")).Text;
            positionInfo.positionMoney = zichanElement.FindElement(By.XPath("//span[text()='持仓市值']/following-sibling::span[1]")).Text;
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
        }, 1000);

        Instance.m_revokeUpdateTimer = CallTimer.Instance.SetInterval(() =>
        {
            Instance.m_browser.OpenNewWindow("https://jywg.18.cn/HKTrade/Revoke");
            LifecycleManager.Instance.Get<EventManager>().RaiseEvent(EventType.RevokeUpdate, new object[] { });
        }, 1000);
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

    private BrowserSystem m_browser = new BrowserSystem();

    private bool m_isInit = false;

    public bool isInit => m_isInit;

    // 持仓更新定时器
    private int m_positionUpdateTimer = -1;

    // 撤单更新定时器
    private int m_revokeUpdateTimer = -1;

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