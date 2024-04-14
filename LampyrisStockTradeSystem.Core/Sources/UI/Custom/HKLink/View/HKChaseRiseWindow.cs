/*
** Author: wushuhong
** Contact: gameta@qq.com
** Description: 港股追涨窗口，窗口内会实时更新，并显示符合追涨策略的股票，并提供快速响应的界面交互的功能
*/

using ImGuiNET;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Linq;
using System.Numerics;

namespace LampyrisStockTradeSystem;

[UniqueWidget]
public class HKChaseRiseWindow : Widget
{
    [MenuItem("港股/追涨全景图")]
    public static void ShowHKChaseRiseWindow()
    {
        WidgetManagement.GetWidget<HKChaseRiseWindow>();
    }

    /// <summary>
    /// 股票代码 -> 港股行情
    /// </summary>
    private static Dictionary<string, HKChaseRiseQuoteData> m_code2stockData = new Dictionary<string, HKChaseRiseQuoteData>();

    private static List<HKChaseRiseQuoteData> m_displayingStockData = new List<HKChaseRiseQuoteData>();

    private static Dictionary<string, int> m_stockCode2DisplayingDataIndex = new Dictionary<string, int>();

    private static BrowserSystem m_browserSystem;

    private static List<HKChaseRiseQuoteData> m_hkStockList = null;

    private HttpClient httpClient = new HttpClient();

    private float m_imageUpdateTimeCounter = 0.0f;

    // 仓位选择
    private int m_tradeOrderRatio { get => AppSettings.Instance.tradeOrderRatio; set => AppSettings.Instance.tradeOrderRatio = value; }

    // 成交剩余策略
    private int m_tradeOrderLeftStrategy { get => AppSettings.Instance.tradeOrderLeftStrategy; set => AppSettings.Instance.tradeOrderLeftStrategy = value; }

    // 异动筛选策略
    private int m_unusualStrategy { get => AppSettings.Instance.unusualStrategy; set => AppSettings.Instance.unusualStrategy = value; }

    private int m_unusualStrategyPrevious = -1;

    // 卖出档位
    private int m_askLevel { get => AppSettings.Instance.askLevel; set => AppSettings.Instance.askLevel = value; }

    // 买入档位
    private int m_bidLevel { get => AppSettings.Instance.bidLevel; set => AppSettings.Instance.bidLevel = value; }

    public override string Name => "港股通追涨全景图";

    [PlannedTask(executeMode = PlannedTaskExecuteMode.ExecuteOnLaunch)]
    public static void RefreshHKStockQuoteOnLaunched()
    {
        RefreshHKStockQuote();
    }

    /// <summary>
    /// 刷新港股通所有股票的行情
    /// </summary>
    [PlannedTask(executeTime = "09:30-16:00", executeMode = PlannedTaskExecuteMode.ExecuteDuringTime | PlannedTaskExecuteMode.ExecuteOnLaunch, intervalMs = 1000)]
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

                        bool hasNew = false;
                        for (int i = 0; i < m_hkStockList.Count; i++)
                        {
                            m_hkStockList[i].moneyRank = Math.Round((double)i / m_hkStockList.Count, 2);

                            // 对于不在全景图中的股票代码,执行选股逻辑
                            if (!m_stockCode2DisplayingDataIndex.ContainsKey(m_hkStockList[i].quoteData.code))
                            {
                                HKChaseRiseQuoteData stockData = m_hkStockList[i];
                                StockRealTimeQuoteData realTimeQuoteData = (StockRealTimeQuoteData)(stockData.quoteData.realTimeQuoteData);

                                bool satisfield = false;
                                bool satisfieldTotal = false;


                                if ((AppSettings.Instance.unusualStrategy & (1 << (int)HKStockUnusualStrategy.RiseSpeedNormal)) != 0)
                                {
                                    satisfield = ((realTimeQuoteData.kLineData.closePrice > 1.5f ? (realTimeQuoteData.riseSpeed > 1.5f) : (realTimeQuoteData.riseSpeed > 2.0f)));
                                    satisfield &= stockData.moneyRank < 0.5f;
                                    satisfieldTotal |= satisfield;
                                }

                                if ((AppSettings.Instance.unusualStrategy & (1 << (int)HKStockUnusualStrategy.RiseSpeedEx)) != 0)
                                {
                                    satisfield = (realTimeQuoteData.riseSpeed > 5f);
                                    satisfieldTotal |= satisfield;
                                }

                                if ((AppSettings.Instance.unusualStrategy & (1 << (int)HKStockUnusualStrategy.FallSpeed)) != 0)
                                {
                                    satisfield = (realTimeQuoteData.riseSpeed < -5f);
                                    satisfieldTotal |= satisfield;
                                }

                                if ((AppSettings.Instance.unusualStrategy & (1 << (int)HKStockUnusualStrategy.Breakthrough)) != 0)
                                {
                  
                                }

                                if ((AppSettings.Instance.unusualStrategy & (1 << (int)HKStockUnusualStrategy.RisePercentage)) != 0)
                                {
                                    satisfield = (realTimeQuoteData.kLineData.percentage) > 10.0f;
                                    satisfieldTotal |= satisfield;
                                }

                                if ((AppSettings.Instance.unusualStrategy & (1 << (int)HKStockUnusualStrategy.FallPercentage)) != 0)
                                {
                                    satisfield = (realTimeQuoteData.kLineData.percentage) < -10.0f;
                                    satisfieldTotal |= satisfield;
                                }


                                if ((AppSettings.Instance.unusualStrategy & (1 << (int)HKStockUnusualStrategy.RiseSpeedTest)) != 0)
                                {
                                    satisfield = (realTimeQuoteData.riseSpeed > 1.5f);
                                    satisfieldTotal |= satisfield;
                                }

                                if (satisfieldTotal)
                                {
                                    int ms = DateTime.Now.Millisecond;
                                    if (ms - stockData.lastUnusualTimestamp > 300 * 1000 || stockData.lastUnusualTimestamp <= 0)
                                    {
                                        string url = StockQuoteInterface.Instance.GetQuoteUrl(StockQuoteInterfaceType.KLineData, "1", UrlUtil.GetStockCodeParam(stockData.quoteData.code), "19900101", "20991231");
                                        if(realTimeQuoteData.minuteData.Count > 0) // 做增量
                                        {
                                            
                                        }
                                        else // 全量
                                        {
                                            HttpRequest.Get(url, (json) =>
                                            {
                                                string strippedJson = JsonStripperUtil.GetEastMoneyStrippedJson(json);
                                                JObject jsonRoot = JObject.Parse(strippedJson);

                                                JArray stockDataArray = jsonRoot?["data"]?["klines"]?.ToObject<JArray>();
                                                if (stockDataArray != null)
                                                {
                                                    realTimeQuoteData.kLineData.lastClosePrice = jsonRoot?["data"]?["preKPrice"]?.SafeToObject<float>() ?? 0.0f;
                                                    for (int i = 0; i < stockDataArray.Count; i++)
                                                    {
                                                        string kLineDataString = stockDataArray[i].ToString();

                                                        {
                                                            // 每一行的数据格式
                                                            // 时间,开盘价,收盘价,最高价,最低价,成交量,成交额,振幅,涨跌幅,涨跌额,换手率
                                                            string[] strings = kLineDataString.Split(',');

                                                            KLineData kLineData = new KLineData();
                                                            kLineData.date = DateTime.ParseExact(strings[0], "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
                                                            kLineData.openPrice = float.Parse(strings[1]);
                                                            kLineData.closePrice = float.Parse(strings[2]);
                                                            kLineData.highestPrice = float.Parse(strings[3]);
                                                            kLineData.lowestPrice = float.Parse(strings[4]);
                                                            kLineData.volume = float.Parse(strings[5]);
                                                            kLineData.money = float.Parse(strings[6]);
                                                            kLineData.amplitude = float.Parse(strings[7]);
                                                            kLineData.percentage = float.Parse(strings[8]);
                                                            kLineData.priceChange = float.Parse(strings[9]);
                                                            kLineData.turnOverRate = float.Parse(strings[10]);

                                                            realTimeQuoteData.minuteData.Add(kLineData);
                                                        }
                                                    }
                                                }
                                            });
                                        }
                                        m_displayingStockData.Add(stockData);
                                        m_stockCode2DisplayingDataIndex[stockData.quoteData.code] = m_displayingStockData.Count - 1;
                                        stockData.lastUnusualTimestamp = ms;
                                        stockData.lastUnusualTime = DateTime.Now.ToString("hh:mm:ss");
                                        hasNew = true;
                                    }
                                }
                            }
                        }

                        if(hasNew)
                        {
                            SystemTrayIcon.Instance.ShowMessage("有新的拉鸡");
                        }
                    }
                }
            }
            catch (Exception) { }
        });
    }

    public override void OnAwake()
    {
        // WidgetManagement.GetWidget<HKChaseRiseTradeSubWindow>();
        m_unusualStrategyPrevious = m_unusualStrategy;
    }

    public override void OnDestroy()
    {
        // WidgetManagement.GetWidget<HKChaseRiseTradeSubWindow>().isOpened = false;
    }

    public override void OnGUI()
    {
        m_imageUpdateTimeCounter += ImGui.GetIO().DeltaTime;

        bool needRefreshImage = false;
        if (m_imageUpdateTimeCounter >= 1f)
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

            ImGui.Text($"共{m_stockCode2DisplayingDataIndex.Count}只");
            ImGui.SameLine();
            ImGUIUtil.DrawSeparator();

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

            // ImGui.BeginDisabled();
            // ImGui.Text("成交剩余策略");
            // ImGui.SameLine();
            // for (int i = 1; i <= (int)TradeOrderLeftStrategy.Count; i++)
            // {
            //     if (ImGui.RadioButton(EnumNameManager.GetName((TradeOrderLeftStrategy)(i)), m_tradeOrderLeftStrategy == i))
            //     {
            //         m_tradeOrderLeftStrategy = i;
            //     }
            //     ImGui.SameLine();
            // }
            // ImGui.EndDisabled();

            // 筛选策略
            // ImGui.Text()

            // 买入申报
            ImGui.SameLine();
            ImGui.Text("买入申报");
            ImGui.SameLine();

            ImGui.SetNextItemWidth(100);

            if (ImGui.BeginCombo("##HKCheaseRiseWindowBidCombo", EnumNameManager.GetName((TradeAskBidLevel)m_bidLevel)))
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
                }
                ImGui.EndCombo();
            }

            ImGui.SameLine();
            ImGui.Text("筛选策略");
            ImGui.SameLine();

            ImGui.SetNextItemWidth(450);

            string displayName = EnumNameManager.GetName((HKStockUnusualStrategy)m_unusualStrategy);
            if (ImGui.BeginCombo("##HKCheaseRiseWindowUnusualStrategy", displayName))
            {
                int value = 0;
                for (int i = 1; i <= (int)HKStockUnusualStrategy.Count; i++)
                {
                    bool isSelected = (m_unusualStrategy & (1 << i)) != 0;
                    if (ImGui.Checkbox(EnumNameManager.GetName((HKStockUnusualStrategy)i), ref isSelected))
                    {
                    }

                    if(isSelected)
                    {
                        value |= (1 << i);
                    }
                }

                m_unusualStrategy = value;
                if (value != m_unusualStrategyPrevious)
                {
                    m_displayingStockData.Clear();
                    m_stockCode2DisplayingDataIndex.Clear();
                    m_unusualStrategyPrevious = value;
                    foreach (var stockData in m_code2stockData.Values)
                    {
                        stockData.lastUnusualTimestamp = -1;
                    }
                    HKChaseRiseWindow.RefreshHKStockQuote();
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

                for (int i = m_displayingStockData.Count - 1; i >= 0; i--)
                {
                    HKChaseRiseQuoteData quoteData = m_displayingStockData[i];
                    ImGui.TableNextRow();

                    ImGui.TableNextColumn();

                    (float percentage, int score) = HKLinkStockPortraitData.Instance.QueryRecentYearData(quoteData.quoteData.code);

                    string name = (m_displayingStockData.Count - i) + ") " +
                                  quoteData.quoteData.code + " " +
                                  quoteData.quoteData.name;

                    var minuteDataList = ((StockRealTimeQuoteData)(quoteData.quoteData.realTimeQuoteData)).minuteData;

                    string coloredInfo = "现价:" + quoteData.quoteData.realTimeQuoteData.kLineData.closePrice + "\n" +
                                         "涨幅:" + quoteData.quoteData.realTimeQuoteData.kLineData.percentage + "%%";
                    string otherInfo = "涨速:" + ((StockRealTimeQuoteData)(quoteData.quoteData.realTimeQuoteData)).riseSpeed + "%%\n" +
                                         "成交额:" + StringUtility.GetMoneyString(quoteData.quoteData.realTimeQuoteData.kLineData.money) + "\n" +
                                         "成交额排位: 前" + (int)(100 * quoteData.moneyRank) + "%%\n" +
                                         "近一年最大涨幅:" + percentage + "%%\n近一年涨幅评分:" + score + "\n" +
                                         "异动检测时间:" + quoteData.lastUnusualTime + "\n";

                    otherInfo += "上一(第一)分钟成交额:" + StringUtility.GetMoneyString(minuteDataList[minuteDataList.Count == 1 ? 0 : minuteDataList.Count - 2].money,round2:1);

                    var nameColor = AppUIStyle.Instance.normalWhiteColor;
                    if (HKLinkSpeicalStockData.Instance.speicalExStockDataSet.Contains(quoteData.quoteData.name))
                        nameColor = AppUIStyle.Instance.tipRedColor;
                    else if (HKLinkSpeicalStockData.Instance.speicalStockDataSet.Contains(quoteData.quoteData.name))
                        nameColor = AppUIStyle.Instance.tipYellowColor;

                    var quoteColor = AppUIStyle.Instance.normalWhiteColor;
                    ImGui.PushStyleColor(ImGuiCol.Text, nameColor);
                    ImGui.Text(name); // 显示股票名称
                    ImGui.PopStyleColor();

                    ImGui.PushStyleColor(ImGuiCol.Text, AppUIStyle.Instance.GetRiseFallColor(quoteData.quoteData.realTimeQuoteData.kLineData.percentage));
                    ImGui.Text(coloredInfo);
                    ImGui.PopStyleColor();

                    ImGui.Text(otherInfo);

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

                            ImGui.Image((IntPtr)quoteData.todayImageTextureId, 1.5f * new Vector2(286, 150));
                            if(ImGui.IsItemClicked())
                            {
                                WidgetManagement.GetWidget<StockIntradayDataWindow>().SetQuoteData(quoteData.quoteData.code,quoteData.quoteData.name,(StockRealTimeQuoteData)quoteData.quoteData.realTimeQuoteData);
                            }
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
                            if (ImGui.IsItemClicked())
                            {
                                WidgetManagement.GetWidget<StockIntradayDataWindow>().SetQuoteData(quoteData.quoteData.code, quoteData.quoteData.name,(StockRealTimeQuoteData)quoteData.quoteData.realTimeQuoteData);
                            }
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
                            EastMoneyTradeManager.Instance.ExecuteBuyByRatio(quoteData.quoteData.code, m_tradeOrderRatio);
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
                        m_stockCode2DisplayingDataIndex.Remove(quoteData.quoteData.code);
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
        if (m_quoteData == null)
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