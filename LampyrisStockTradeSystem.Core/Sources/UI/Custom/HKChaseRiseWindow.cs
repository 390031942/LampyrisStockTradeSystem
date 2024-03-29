/*
** Author: wushuhong
** Contact: gameta@qq.com
** Description: 港股追涨窗口，窗口内会实时更新，并显示符合追涨策略的股票，并提供快速响应的界面交互的功能
*/

using ImGuiNET;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium;

namespace LampyrisStockTradeSystem;

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

    public override void OnGUI()
    {
        if (ImGui.BeginTable("Your Table", 3)) // 创建一个有3列的表格
        {
            ImGui.TableSetupColumn("股票名称");
            ImGui.TableSetupColumn("分时/K线图");
            ImGui.TableSetupColumn("操作按钮");
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
    [MenuItem("交易/登录")]
    public static void Login()
    {
        if(!HKLinkTradeManager.Instance.isInit)
        {
            HKLinkTradeManager.Instance.m_browser.Init();
            HKLinkTradeManager.Instance.m_isInit = true;

            LifecycleManager.Instance.Get<EventManager>().AddEventHandler(EventType.LoginButtonClicked, (object[] parameters) => 
            {
                HKLinkTradeManager.Instance.m_browser.Input(By.Id("txtZjzh"), TradeLoginInfo.Instance.account);
                HKLinkTradeManager.Instance.m_browser.Input(By.Id("txtPwd"), TradeLoginInfo.Instance.password);
                HKLinkTradeManager.Instance.m_browser.Input(By.Id("txtValidCode"), (string)parameters[0]);
                HKLinkTradeManager.Instance.m_browser.Click(By.Id("rdsc45"));
                HKLinkTradeManager.Instance.m_browser.Click(By.Id("btnConfirm"));
                WidgetManagement.GetWidget<TradeLoginWindow>().isOpened = false;
            });
;        }

        HKLinkTradeManager.Instance.m_browser.Request("https://jywg.18.cn/Login?el=1&clear=&returl=%2fTrade%2fBuy");
        HKLinkTradeManager.Instance.m_browser.SaveImg(OpenQA.Selenium.By.Id("imgValidCode"), "imgValidCode.png", false);

        Bitmap bitmap = (Bitmap)Bitmap.FromFile("imgValidCode.png");

        WidgetManagement.GetWidget<TradeLoginWindow>().SetValidCodePNGFilePath("imgValidCode.png");
    }

    private BrowserSystem m_browser = new BrowserSystem();

    private bool m_isInit = false;

    public bool isInit => m_isInit;
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