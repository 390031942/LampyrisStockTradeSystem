/*
** Author: wushuhong
** Contact: gameta@qq.com
** Description: 港股追涨窗口，窗口内会实时更新，并显示符合追涨策略的股票，并提供快速响应的界面交互的功能
*/

using ImGuiNET;
using Newtonsoft.Json.Linq;
using System.Numerics;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace LampyrisStockTradeSystem;

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

    private static List<HKChaseRiseQuoteData> m_hkStockList = null;

    private HttpClient httpClient = new HttpClient();

    private float m_imageUpdateTimeCounter = 0.0f;

    // 仓位选择
    private int m_tradeOrderRatio = 1;

    // 成交剩余策略
    private int m_tradeOrderLeftStrategy = 1;

    // 异动筛选策略
    private int m_unusualStrategy = 1;

    // 卖出档位
    private int m_askLevel = 3;

    // 买入档位
    private int m_bidLevel = 3;

    public override string Name => "港股通追涨全景图";

    [PlannedTask(executeMode = PlannedTaskExecuteMode.ExecuteOnLaunch)]
    public static void RefreshHKStockQuoteOnLaunched()
    {
        RefreshHKStockQuote();
    }

    /// <summary>
    /// 刷新港股通所有股票的行情
    /// </summary>
    [PlannedTask(executeTime ="09:29-16:10",executeMode = PlannedTaskExecuteMode.ExecuteDuringTime | PlannedTaskExecuteMode.ExecuteOnLaunch,intervalMs = 1000)]
    public static void RefreshHKStockQuote()
    {
        HttpRequest.Get(StockQuoteInterface.Instance.GetQuoteUrl(StockQuoteInterfaceType.HKLink), (string json) => {
            string strippedJson = JsonStripperUtil.GetEastMoneyStrippedJson(json);
            try
            {
                lock (m_code2stockData)
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
                                    realTimeQuoteData.riseSpeed = stockObject["f22"].SafeToObject<float>(); // 涨速
                                    realTimeQuoteData.bidAskData.theCommittee = stockObject["f31"].SafeToObject<float>(); // 买一价,可能是"-"
                                    realTimeQuoteData.buyPrice = stockObject["f32"].SafeToObject<float>(); // 卖一价,可能是"-"
                                    realTimeQuoteData.sellPrice = stockObject["f33"].SafeToObject<float>(); // 委比
                                    realTimeQuoteData.transactionSumData.buyCount = stockObject["f34"].SafeToObject<float>(); // 外盘
                                    realTimeQuoteData.transactionSumData.sellCount = stockObject["f35"].SafeToObject<float>(); // 内盘                    
                                }
                            }
                        }

                        // 这个列表一般不会变，只需要获取一次就行
                        if (m_hkStockList == null)
                            m_hkStockList = m_code2stockData.Values.ToList();

                        // 将Value从大到小排序
                        m_hkStockList.Sort((a, b) =>
                        {
                            return (int)(b.quoteData.realTimeQuoteData.kLineData.money - a.quoteData.realTimeQuoteData.kLineData.money);
                        });

                        for (int i = 0; i < m_hkStockList.Count; i++)
                        {
                            m_hkStockList[i].moneyRank = Math.Round((double)i / m_hkStockList.Count, 2);

                            // 对于不在全景图中的股票代码,执行选股逻辑
                            if (!m_stockCode2DisplayingDataIndex.ContainsKey(m_hkStockList[i].quoteData.code))
                            {
                                HKChaseRiseQuoteData stockData = m_hkStockList[i];
                                StockRealTimeQuoteData realTimeQuoteData = (StockRealTimeQuoteData)(stockData.quoteData.realTimeQuoteData);

                                // if ((realTimeQuoteData.kLineData.closePrice > 1.5f ? (realTimeQuoteData.riseSpeed > 1.5f) : (realTimeQuoteData.riseSpeed > 2.0f)))
                                if (realTimeQuoteData.riseSpeed > 1.5f)
                                {
                                    // if (stockData.moneyRank < 1.5f)
                                    {
                                        int ms = DateTime.Now.Millisecond;
                                        if (ms - stockData.lastUnusualTimestamp > 300 * 1000 || stockData.lastUnusualTimestamp <= 0)
                                        {
                                            m_displayingStockData.Add(stockData);
                                            m_stockCode2DisplayingDataIndex[stockData.quoteData.code] = m_displayingStockData.Count - 1;
                                            stockData.lastUnusualTimestamp = ms;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception) { }
        });
    }

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
        m_imageUpdateTimeCounter += ImGui.GetIO().DeltaTime;

        bool needRefreshImage = false;
        if(m_imageUpdateTimeCounter >= 1f)
        {
            needRefreshImage = true;
            m_imageUpdateTimeCounter = 0.0f;
        }

        // 计算窗口的内容区域大小
        Vector2 contentSize = ImGui.GetWindowSize();

        // 顶部面板，展示：策略筛选，购买策略等UI
        ImGui.BeginChild("##HKChaseRiseWindowTopPanel", new Vector2(contentSize.X, 40), true);
        {
            // 跟进仓位

            ImGui.Text("跟进仓位");
            ImGui.SameLine();
            for (int i = 1; i <= (int)TradeOrderRatio.Count; i++)
            {
                if (ImGui.RadioButton(EnumNameManager.GetName((TradeOrderRatio)(i)), m_tradeOrderRatio == i))
                {
                    m_tradeOrderRatio = i;
                }
                ImGui.SameLine();
            }

            ImGui.Text("成交剩余策略");
            ImGui.SameLine();
            for (int i = 1; i <= (int)TradeOrderLeftStrategy.Count; i++)
            {
                if (ImGui.RadioButton(EnumNameManager.GetName((TradeOrderLeftStrategy)(i)), m_tradeOrderLeftStrategy == i))
                {
                    m_tradeOrderLeftStrategy = i;
                }
                ImGui.SameLine();
            }

            // 筛选策略
            // ImGui.Text()

            // 买入申报
            ImGui.SameLine();
            ImGui.Text("买入申报");
            ImGui.SameLine();

            ImGui.SetNextItemWidth(100);

            if (ImGui.BeginCombo("##HKCheaseRiseWindowBidCombo",EnumNameManager.GetName((TradeAskBidLevel)m_bidLevel)))
            {
                for (int i = 1; i <= (int)TradeAskBidLevel.Count; i++)
                {
                    bool isSelected = (m_bidLevel == i);
                    string name = EnumNameManager.GetName((TradeAskBidLevel)i);
                    if (ImGui.Selectable(name, isSelected))
                    {
                        m_bidLevel = i;
                    }

                }
                ImGui.EndCombo();
            }

            ImGui.SameLine();
            ImGui.Text("卖出申报");
            ImGui.SameLine();

            ImGui.SetNextItemWidth(100);

            if (ImGui.BeginCombo("##HKCheaseRiseWindowAskCombo", EnumNameManager.GetName((TradeAskBidLevel)m_askLevel)))
            {
                for (int i = 1; i <= (int)TradeAskBidLevel.Count; i++)
                {
                    bool isSelected = (m_askLevel == i);
                    if (ImGui.Selectable(EnumNameManager.GetName((TradeAskBidLevel)i), isSelected))
                    {
                        m_askLevel = i;
                    }

                    // 设置默认选中的项
                    if (isSelected)
                    {
                        ImGui.SetItemDefaultFocus();
                    }
                }
                ImGui.EndCombo();
            }

            ImGui.SameLine();
            ImGui.Text("筛选策略");
            ImGui.SameLine();

            ImGui.SetNextItemWidth(450);

            if (ImGui.BeginCombo("##HKCheaseRiseWindowUnusualStrategyh", EnumNameManager.GetName((HKStockUnusualStrategy)m_unusualStrategy)))
            {
                for (int i = 1; i <= (int)HKStockUnusualStrategy.Count; i++)
                {
                    bool isSelected = (m_unusualStrategy == i);
                    if (ImGui.Selectable(EnumNameManager.GetName((HKStockUnusualStrategy)i), isSelected))
                    {
                        m_unusualStrategy = i;
                    }

                    // 设置默认选中的项
                    if (isSelected)
                    {
                        ImGui.SetItemDefaultFocus();
                    }
                }
                ImGui.EndCombo();
            }
        }
        ImGui.EndChild();

        ImGui.BeginChild("##HKChaseRiseWindowBottomPanel", new Vector2(contentSize.X, contentSize.Y - 40), true);
        {
            if (ImGui.BeginTable("HKChaseRiseTotalView", 4))
            {
                ImGui.TableSetupColumn("股票名称", ImGuiTableColumnFlags.WidthFixed, 200);
                ImGui.TableSetupColumn("分时", ImGuiTableColumnFlags.WidthStretch);
                ImGui.TableSetupColumn("K线", ImGuiTableColumnFlags.WidthStretch);
                ImGui.TableSetupColumn("操作按钮", ImGuiTableColumnFlags.WidthFixed, 100);
                ImGui.TableHeadersRow();

                for (int i = 0; i < m_displayingStockData.Count; i++)
                {
                    HKChaseRiseQuoteData quoteData = m_displayingStockData[i];
                    ImGui.TableNextRow();

                    ImGui.TableNextColumn();

                    string displayInfo = quoteData.quoteData.code + " " +
                                         quoteData.quoteData.name + "\n" +
                                         "现价:" + quoteData.quoteData.realTimeQuoteData.kLineData.closePrice + "\n" +
                                         "涨幅:" + quoteData.quoteData.realTimeQuoteData.kLineData.percentage + "%%\n" +
                                         "涨速:" + ((StockRealTimeQuoteData)(quoteData.quoteData.realTimeQuoteData)).riseSpeed + "%%\n" +
                                         "成交额:" + StringUtility.GetMoneyString(quoteData.quoteData.realTimeQuoteData.kLineData.money) + "\n" +
                                         "成交额排位: 前" + (int)(100 * quoteData.moneyRank) + "%%";

                    ImGui.Text(displayInfo); // 显示股票名称

                    ImGui.TableNextColumn();

                    // 分时图
                    // if (quoteData.displayingToday)
                    {
                        if (quoteData.todayImageTextureId <= 0)
                        {
                            if (quoteData.loadTodayImageTask == null)
                            {
                                quoteData.loadTodayImageTask = httpClient.GetByteArrayAsync($"http://webquotepic.eastmoney.com/GetPic.aspx?imageType=r&nid=116.{quoteData.quoteData.code}&timespan=1712254059");
                            }
                            else if (quoteData.loadTodayImageTask.IsCompleted)
                            {
                                quoteData.todayImageTextureId = Resources.LoadTextureFromBytes(quoteData.loadTodayImageTask.Result);
                                quoteData.loadTodayImageTask = null;
                            }
                        }
                        else if (quoteData.isReloadToday)
                        {
                            if (quoteData.loadTodayImageTask.IsCompleted)
                            {
                                // 先释放旧的
                                Resources.FreeTexture(quoteData.todayImageTextureId);

                                // 加载新的
                                quoteData.todayImageTextureId = Resources.LoadTextureFromBytes(quoteData.loadTodayImageTask.Result);

                                // 移除掉任务
                                quoteData.loadTodayImageTask = null;
                                quoteData.isReloadToday = false;
                            }

                            ImGui.Image((IntPtr)quoteData.todayImageTextureId, 1.5f * new System.Numerics.Vector2(286, 150));
                        }
                        else
                        {
                            // 新开一个 异步加载 任务
                            if (needRefreshImage)
                            {
                                quoteData.isReloadToday = true;
                                quoteData.loadTodayImageTask = httpClient.GetByteArrayAsync($"http://webquotepic.eastmoney.com/GetPic.aspx?imageType=r&nid=116.{quoteData.quoteData.code}&timespan=1712254059");
                            }
                            ImGui.Image((IntPtr)quoteData.todayImageTextureId, 1.5f * new System.Numerics.Vector2(286, 150));
                        }
                    }
                    ImGui.TableNextColumn();

                    // else
                    {
                        if (quoteData.klineTextureId <= 0)
                        {
                            if (quoteData.loadkLineImageTask == null)
                            {
                                quoteData.loadkLineImageTask = httpClient.GetByteArrayAsync($"http://webquoteklinepic.eastmoney.com/GetPic.aspx?nid=116.{quoteData.quoteData.code}&type=&unitWidth=-5&ef=&formula=RSI&AT=1&imageType=KXL&timespan=1712334914");
                            }
                            else if (quoteData.loadkLineImageTask.IsCompleted)
                            {
                                quoteData.klineTextureId = Resources.LoadTextureFromBytes(quoteData.loadkLineImageTask.Result, new Rectangle(0, 0, 520, 285));
                                quoteData.loadkLineImageTask = null;
                            }
                        }
                        else if (quoteData.isReloadKline)
                        {
                            if (quoteData.loadkLineImageTask.IsCompleted)
                            {
                                // 先释放旧的
                                Resources.FreeTexture(quoteData.klineTextureId);

                                // 加载新的
                                quoteData.klineTextureId = Resources.LoadTextureFromBytes(quoteData.loadkLineImageTask.Result, new Rectangle(0, 0, 520, 285));

                                // 移除掉任务
                                quoteData.loadkLineImageTask = null;
                                quoteData.isReloadKline = false;
                            }

                            ImGui.Image((IntPtr)quoteData.klineTextureId, 1.5f * new System.Numerics.Vector2(286, 150));
                        }
                        else
                        {
                            // 新开一个 异步加载 任务
                            if (needRefreshImage)
                            {
                                quoteData.isReloadKline = true;
                                quoteData.loadkLineImageTask = httpClient.GetByteArrayAsync($"http://webquoteklinepic.eastmoney.com/GetPic.aspx?nid=116.{quoteData.quoteData.code}&type=&unitWidth=-5&ef=&formula=RSI&AT=1&imageType=KXL&timespan=1712334914");
                            }
                            ImGui.Image((IntPtr)quoteData.klineTextureId, 1.5f * new System.Numerics.Vector2(286, 150));
                        }
                    }

                    ImGui.TableNextColumn();

                    ImGui.PushID($"HKChaseRiseTotalViewBuyBtn{i}");
                    if (ImGui.Button("跟进"))
                    {
                        if (EastMoneyTradeManager.Instance.isLoggedIn)
                        {
                            EastMoneyTradeManager.Instance.ExecuteBuyByRatio(quoteData.quoteData.code, 1);
                        }
                        else
                        {
                            WidgetManagement.GetWidget<MessageBox>().SetContent("跟进股票", "你都没有登录，跟进个锤子哦");
                        }
                    }
                    ImGui.PopID();

                    ImGui.PushID($"HKChaseRiseTotalViewIgnoreBtn{i}");
                    if (ImGui.Button("忽略"))
                    {
                        m_displayingStockData.Remove(quoteData);
                    }
                    ImGui.PopID();
                }

                ImGui.EndTable();
            }
        }
        ImGui.EndChild();
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