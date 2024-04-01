/*
** Author: wushuhong
** Contact: gameta@qq.com
** Description: 港股追涨窗口，窗口内会实时更新，并显示符合追涨策略的股票，并提供快速响应的界面交互的功能
*/

using ImGuiNET;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using System.Numerics;
using System.Security.Cryptography.X509Certificates;

namespace LampyrisStockTradeSystem;

public class EastMoneyPositionStockInfo
{
    public string stockCode;
    public string stockName;
    public string count;
    public string useableCount;
    public string costPrice;
    public string currentPrice;
    public string money;
    public string profitLose;
    public string profitLoseRatio;
    public string todayProfitLost;
    public string todayProfitLostRatio;
}

// 持仓信息
public class EastMoneyPositionInfo
{
    // 总资产
    public string totalMoney;

    // 持仓资产
    public string positionMoney;

    // 持仓盈亏
    public string positionProfitLose;

    // 当日盈亏
    public string todayProfitLose;

    // 可用资金
    public string canUseMoney;

    public List<EastMoneyPositionStockInfo> stockInfos = new List<EastMoneyPositionStockInfo>();
}

// 撤单信息
public class EastMoneyRevokeInfo
{

}

public class HKChaseRiseQuoteData
{
    // 行情数据
    public QuoteData quoteData;

    // K线数据
    public Bitmap klineImage;
    public int klineTextureId;

    // 今日分时走势
    public Bitmap todayImage;
    public int todayImageTextureId;

    // 日内 分钟-分时图 破新高次数
    public int breakthroughTimes;

    public bool displayingToday = true;

    public Task<byte[]>? loadImageTask;

    public int lastUnusualTimestamp = -1;
}

public abstract class HKChaseRiseStrategy
{
    public abstract bool Satisfied();

    public abstract bool Compare(HKChaseRiseQuoteData lhs,HKChaseRiseQuoteData rhs);
}

[UniqueWidget]
public class HKChaseRiseWindow:Widget
{
    [MenuItem("港股/追涨全景图")]
    public static void ShowHKChaseRiseWindow()
    {
        WidgetManagement.GetWidget<HKChaseRiseWindow>();
    }

    /// <summary>
    /// 股票代码 -> 港股行情
    /// </summary>
    private static Dictionary<string,HKChaseRiseQuoteData> m_code2stockData = new Dictionary<string,HKChaseRiseQuoteData>();

    private static List<HKChaseRiseQuoteData> m_displayingStockData = new List<HKChaseRiseQuoteData>();

    private static Dictionary<string,int> m_stockCode2DisplayingDataIndex = new Dictionary<string,int>();

    private static BrowserSystem m_browserSystem;
    
    public override string Name => "港股通追涨全景图";

    [PlannedTask(executeMode = PlannedTaskExecuteMode.ExecuteOnLaunch)]
    public static void RefreshHKStockQuoteOnLaunched()
    {
        RefreshHKStockQuote();
    }

    /// <summary>
    /// 刷新港股通所有股票的行情
    /// </summary>
    [PlannedTask(executeTime ="09:29-16:10",executeMode = PlannedTaskExecuteMode.ExecuteDuringTime,intervalMs = 3000)]
    public static void RefreshHKStockQuote()
    {
        HttpRequest.Get(StockQuoteInterface.Instance.GetQuoteUrl(StockQuoteInterfaceType.HKLink), (string json) => {
            string strippedJson = JsonStripperUtil.GetEastMoneyStrippedJson(json);
            try
            {
                lock(m_code2stockData)
                {
                    JObject jsonRoot = JObject.Parse(strippedJson);

                    JArray stockDataArray = jsonRoot?["data"]?["diff"]?.ToObject<JArray>();
                    if (stockDataArray != null)
                    {
                        for (int i = 0; i < stockDataArray.Count; i++)
                        {
                            JObject stockObject = stockDataArray[i].ToObject<JObject>();

                            if (stockObject != null)
                            {
                                // 这里获取股票代码和名称
                                string name = stockObject["f14"]?.ToString();
                                string code = stockObject["f12"]?.ToString();

                                if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(code))
                                {
                                    HKChaseRiseQuoteData stockData = null;
                                    if (!m_code2stockData.ContainsKey(code))
                                    {
                                        stockData = m_code2stockData[code] = new HKChaseRiseQuoteData()
                                        {
                                            quoteData = new QuoteData()
                                            {
                                                code = code,
                                                name = name,
                                                realTimeQuoteData = new StockRealTimeQuoteData()
                                            }
                                        };
                                    }
                                    else
                                    {
                                        stockData = m_code2stockData[code];
                                    }

                                    // 更新股票的实时行情
                                    StockRealTimeQuoteData realTimeQuoteData = (StockRealTimeQuoteData)stockData.quoteData.realTimeQuoteData;

                                    realTimeQuoteData.kLineData.closePrice = stockObject["f2"].SafeToObject<float>(); // 现价
                                    realTimeQuoteData.kLineData.percentage = stockObject["f3"].SafeToObject<float>(); // 涨幅
                                    realTimeQuoteData.kLineData.priceChange = stockObject["f4"].SafeToObject<float>(); // 涨跌额
                                    realTimeQuoteData.kLineData.volume = stockObject["f5"].SafeToObject<float>(); // 成交量
                                    realTimeQuoteData.kLineData.money = stockObject["f6"].SafeToObject<float>(); // 成交额
                                    realTimeQuoteData.kLineData.turnOverRate = stockObject["f8"].SafeToObject<float>(); // 换手率
                                    realTimeQuoteData.kLineData.highestPrice = stockObject["f15"].SafeToObject<float>(); // 最高
                                    realTimeQuoteData.kLineData.lowestPrice = stockObject["f16"].SafeToObject<float>(); // 最低
                                    realTimeQuoteData.kLineData.openPrice = stockObject["f17"].SafeToObject<float>(); // 今开
                                    realTimeQuoteData.riseSpped = stockObject["f22"].SafeToObject<float>(); // 涨速
                                    realTimeQuoteData.bidAskData.theCommittee = stockObject["f31"].SafeToObject<float>(); // 买一价,可能是"-"
                                    realTimeQuoteData.buyPrice = stockObject["f32"].SafeToObject<float>(); // 卖一价,可能是"-"
                                    realTimeQuoteData.sellPrice = stockObject["f33"].SafeToObject<float>(); // 委比
                                    realTimeQuoteData.transactionSumData.buyCount = stockObject["f34"].SafeToObject<float>(); // 外盘
                                    realTimeQuoteData.transactionSumData.sellCount = stockObject["f35"].SafeToObject<float>(); // 内盘

                                    // 对于不在全景图中的股票代码,执行选股逻辑
                                    if(!m_stockCode2DisplayingDataIndex.ContainsKey(code))
                                    {
                                        if (realTimeQuoteData.riseSpped > 1.5f)
                                        {
                                            int ms = DateTime.Now.Millisecond;
                                            if (ms - stockData.lastUnusualTimestamp > 5000 || stockData.lastUnusualTimestamp <= 0)
                                            {
                                                m_displayingStockData.Add(stockData);
                                                m_stockCode2DisplayingDataIndex[code] = m_displayingStockData.Count - 1;
                                                stockData.lastUnusualTimestamp = ms;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // WidgetManagement.GetWidget<MessageBox>().SetContent("StockQuoteInterfaceType.CurrentQuotes报错", ex.ToString());
            }
        });
    }

    private HttpClient httpClient = new HttpClient();

    public override void OnAwake()
    {
        WidgetManagement.GetWidget<HKChaseRiseTradeSubWindow>();
    }

    public override void OnDestroy()
    {
        WidgetManagement.GetWidget<HKChaseRiseTradeSubWindow>().isOpened = false;
    }

    public override void OnGUI()
    {
        if (ImGui.BeginTable("HKChaseRiseTotalView", 3)) // 创建一个有3列的表格
        {
            ImGui.TableSetupColumn("股票名称", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn("分时/K线图",ImGuiTableColumnFlags.WidthFixed,400);
            ImGui.TableSetupColumn("操作按钮",ImGuiTableColumnFlags.WidthFixed,400);
            ImGui.TableHeadersRow();
            
            foreach (HKChaseRiseQuoteData quoteData in m_displayingStockData)
            {
                ImGui.TableNextRow();

                ImGui.TableNextColumn();
                ImGui.Text(quoteData.quoteData.name); // 显示股票名称

                ImGui.TableNextColumn();

                // 分时图
                if (quoteData.displayingToday)
                {
                    if (quoteData.todayImageTextureId <= 0)
                    {
                        if(quoteData.loadImageTask == null)
                        {
                            quoteData.loadImageTask = httpClient.GetByteArrayAsync($"https://webquotepic.eastmoney.com/GetPic.aspx?nid=116.{quoteData.quoteData.code}&imageType=TADR&token=e424fa7066ff5fe74ceb9708dd59cfc2&v=1711555522790");
                        }
                        else if (quoteData.loadImageTask.IsCompleted)
                        {
                            quoteData.todayImageTextureId = Resources.LoadTextureFromBytes(quoteData.loadImageTask.Result);
                            quoteData.loadImageTask = null;
                        }
                    }
                    else
                    {
                        ImGui.Image((IntPtr)quoteData.todayImageTextureId, new System.Numerics.Vector2(380, 250));
                    }
                }
                else
                {
                    if (quoteData.klineTextureId <= 0)
                    {
                        if (quoteData.loadImageTask == null)
                        {
                            quoteData.loadImageTask = httpClient.GetByteArrayAsync($"https://webquoteklinepic.eastmoney.com/GetPic.aspx?nid={quoteData.quoteData.code}&imageType=TADK&token=e424fa7066ff5fe74ceb9708dd59cfc2&unitwidth=30&v=1711556663209");
                        }
                        else if (quoteData.loadImageTask.IsCompleted)
                        {
                            quoteData.klineTextureId = Resources.LoadTextureFromBytes(quoteData.loadImageTask.Result);
                            quoteData.loadImageTask = null;
                        }
                    }
                    else
                    {
                        ImGui.Image((IntPtr)quoteData.todayImageTextureId, new System.Numerics.Vector2(380, 250));
                    }
                }

                ImGui.TableNextColumn();
                if (ImGui.Button("跟进"))
                {
                    WidgetManagement.GetWidget<HKChaseRiseBuyWindow>().SetStockQuoteData(quoteData);
                }
                if (ImGui.Button("忽略"))
                {
                    m_displayingStockData.Remove(quoteData);
                }
            }
            
            ImGui.EndTable();
        }
    }

}

public class HKLinkTradeManager:Singleton<HKLinkTradeManager>
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
        WidgetManagement.GetWidget<TradeLoginWindow>().isOpened = false;
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
        WidgetManagement.GetWidget<TradeLoginWindow>().SetValidCodePNGFilePath("imgValidCode.png");
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
                            WidgetManagement.GetWidget<TradeLoginWindow>().isWrongInfo = true;
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

[UniqueWidget]
public class HKChaseRiseBuyWindow : Widget
{
    public override string Name => "港股通 跟进委托";

    private HKChaseRiseQuoteData m_quoteData;

    public override void OnGUI()
    {
        if(m_quoteData == null)
        {
            ImGui.Text("啥数据都没有，怎么追涨?");
            return;
        }

    }

    public void SetStockQuoteData(HKChaseRiseQuoteData quoteData)
    {
        m_quoteData = quoteData;
    }
}

[UniqueWidget]
public class HKChaseRiseTradeSubWindow : Widget
{
    private EastMoneyPositionInfo m_positionInfo;
    public override string Name => "港股通交易";


    public override void OnAwake()
    {
        base.OnAwake();
        pos = new Vector2(0, ImGui.GetIO().DisplaySize.Y - 200);
        size = new Vector2(400, 200);

        LifecycleManager.Instance.Get<EventManager>().AddEventHandler(EventType.PositionUpdate, OnPositionUpdate);
        LifecycleManager.Instance.Get<EventManager>().AddEventHandler(EventType.RevokeUpdate, OnRevokeUpdate);
    }

    public override void OnDestroy()
    {
        LifecycleManager.Instance.Get<EventManager>().RemoveEventHandler(EventType.PositionUpdate, OnPositionUpdate);
        LifecycleManager.Instance.Get<EventManager>().RemoveEventHandler(EventType.RevokeUpdate, OnRevokeUpdate);
        base.OnDestroy();
    }

    public void OnPositionUpdate(object[] parameters)
    {
        m_positionInfo = (EastMoneyPositionInfo)parameters[0];
    }

    public void OnRevokeUpdate(object[] parameters)
    {

    }

    public override void OnGUI()
    {
        // 创建滚动区域
        ImGui.BeginChild("滚动区域", new Vector2(0, 0), true);
        {
            if (ImGui.CollapsingHeader("持仓"))
            {
                if(m_positionInfo != null)
                {
                    ImGui.Text("总市值:" + m_positionInfo.totalMoney);
                    ImGui.SameLine();

                    ImGui.Text("持仓市值" + m_positionInfo.positionMoney);
                    ImGui.SameLine();

                    ImGui.Text("持仓盈亏" + m_positionInfo.positionProfitLose);

                    ImGui.Text("当日盈亏" + m_positionInfo.todayProfitLose);
                    ImGui.SameLine();
                    ImGui.Text("可用资金" + m_positionInfo.canUseMoney);

                    if (m_positionInfo.stockInfos.Count > 0)
                    {
                        if (ImGui.BeginTable("HKChaseRiseSubWinOrder", 6)) // 创建一个有3列的表格
                        {
                            ImGui.TableSetupColumn("代码");
                            ImGui.TableSetupColumn("名称");
                            ImGui.TableSetupColumn("成本");
                            ImGui.TableSetupColumn("数量");
                            ImGui.TableSetupColumn("浮盈");
                            ImGui.TableSetupColumn("");
                            ImGui.TableHeadersRow();

                            foreach(EastMoneyPositionStockInfo stockInfo in m_positionInfo.stockInfos)
                            {
                                ImGui.TableNextRow();

                                ImGui.TableNextColumn();
                                ImGui.Text(stockInfo.stockCode);
                                ImGui.TableNextColumn();
                                ImGui.Text(stockInfo.stockName);
                                ImGui.TableNextColumn();
                                ImGui.Text(stockInfo.costPrice);
                                ImGui.TableNextColumn();
                                ImGui.Text(stockInfo.count);
                                ImGui.TableNextColumn();
                                ImGui.Text(stockInfo.profitLose);
                                ImGui.TableNextColumn();
                                ImGui.Button("卖出");
                            }

                            ImGui.EndTable();
                        }
                    }
                    else
                    {
                        ImGui.Text("暂无持仓股票");
                    }
                }
                else
                {

                }
            }
            if (ImGui.CollapsingHeader("委托"))
            {
                if (ImGui.BeginTable("HKChaseRiseSubWinOwnStock", 6)) // 创建一个有3列的表格
                {
                    ImGui.TableSetupColumn("代码");
                    ImGui.TableSetupColumn("名称");
                    ImGui.TableSetupColumn("方向");
                    ImGui.TableSetupColumn("价格");
                    ImGui.TableSetupColumn("数量");
                    ImGui.TableSetupColumn("");
                    ImGui.TableHeadersRow();

                    ImGui.TableNextRow();

                    ImGui.TableNextColumn();
                    ImGui.Text("600000");
                    ImGui.TableNextColumn();
                    ImGui.Text("浦发银行");
                    ImGui.TableNextColumn();
                    ImGui.Text("买入");
                    ImGui.TableNextColumn();
                    ImGui.Text("7.00");
                    ImGui.TableNextColumn();
                    ImGui.Text("60000");
                    ImGui.TableNextColumn();
                    ImGui.Button("撤单");

                    ImGui.EndTable();
                }
            }
        }
        // 结束滚动区域
        ImGui.EndChild();
    }
}