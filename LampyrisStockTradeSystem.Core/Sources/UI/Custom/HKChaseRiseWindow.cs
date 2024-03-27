/*
** Author: wushuhong
** Contact: gameta@qq.com
** Description: 港股追涨窗口，窗口内会实时更新，并显示符合追涨策略的股票，并提供快速响应的界面交互的功能
*/

using ImGuiNET;
using Newtonsoft.Json.Linq;

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
    /// 
    /// </summary>
    private static Dictionary<string,HKChaseRiseQuoteData> m_code2stockData = new Dictionary<string,HKChaseRiseQuoteData>();

    /// <summary>
    /// 刷新港股通所有股票的行情
    /// </summary>
    [PlannedTask(executeTime ="09:29-16:10",executeMode = PlannedTaskExecuteMode.ExecuteDuringTime)]
    public static void RefreshHKStockQuote()
    {
        HttpRequest.Get(StockQuoteInterface.Instance.GetQuoteUrl(StockQuoteInterfaceType.HKLink), (string json) => {
            string strippedJson = JsonStripperUtil.GetEastMoneyStrippedJson(json);
            try
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
                                        quoteData = new QuoteData(),
                                    };
                                }
                                else
                                {
                                    stockData = m_code2stockData[code];
                                }

                                // 更新股票的实时行情
                                // StockRealTimeQuoteData realTimeQuoteData = (StockRealTimeQuoteData)stockData.realTimeQuoteData;
                                // realTimeQuoteData.kLineData.closePrice = stockObject["f2"].SafeToObject<float>(); // 现价
                                // realTimeQuoteData.kLineData.percentage = stockObject["f3"].SafeToObject<float>(); // 涨幅
                                // realTimeQuoteData.kLineData.priceChange = stockObject["f4"].SafeToObject<float>(); // 涨跌额
                                // realTimeQuoteData.kLineData.volume = stockObject["f5"].SafeToObject<float>(); // 成交量
                                // realTimeQuoteData.kLineData.money = stockObject["f6"].SafeToObject<float>(); // 成交额
                                // realTimeQuoteData.kLineData.turnOverRate = stockObject["f8"].SafeToObject<float>(); // 换手率
                                // realTimeQuoteData.kLineData.highestPrice = stockObject["f15"].SafeToObject<float>(); // 最高
                                // realTimeQuoteData.kLineData.lowestPrice = stockObject["f16"].SafeToObject<float>(); // 最低
                                // realTimeQuoteData.kLineData.openPrice = stockObject["f17"].SafeToObject<float>(); // 今开
                                // realTimeQuoteData.riseSpped = stockObject["f22"].SafeToObject<float>(); // 涨速
                                // realTimeQuoteData.bidAskData.theCommittee = stockObject["f31"].SafeToObject<float>(); // 买一价,可能是"-"
                                // realTimeQuoteData.buyPrice = stockObject["f32"].SafeToObject<float>(); // 卖一价,可能是"-"
                                // realTimeQuoteData.sellPrice = stockObject["f33"].SafeToObject<float>(); // 委比
                                // realTimeQuoteData.transactionSumData.buyCount = stockObject["f34"].SafeToObject<float>(); // 外盘
                                // realTimeQuoteData.transactionSumData.sellCount = stockObject["f35"].SafeToObject<float>(); // 内盘
                            }
                        }
                    }

                    LifecycleManager.Instance.Get<EventManager>().RaiseEvent(EventType.UpdateRealTimeQuotes);
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
        ImGui.Begin("港股通追涨全景图"); // 开始一个新的ImGui窗口

        if (ImGui.BeginTable("Your Table", 3)) // 创建一个有3列的表格
        {
            ImGui.TableSetupColumn("股票名称");
            ImGui.TableSetupColumn("分时/K线图");
            ImGui.TableSetupColumn("操作按钮");
            ImGui.TableHeadersRow();
            /*
            foreach (var item in m_stockData)
            {
                ImGui.TableNextRow();

                ImGui.TableNextColumn();
                ImGui.Text(item.quoteData.name); // 显示股票名称

                ImGui.TableNextColumn();
                if (image != null)
                {
                    IntPtr imageId = image.ToImGuiTexture(); // 这需要你实现一个扩展方法来将System.Drawing的Image转换为ImGui可用的纹理
                    ImGui.Image(imageId, new System.Numerics.Vector2(50, 50)); // 显示图片，这里的尺寸你可以根据需要调整
                }

                ImGui.TableNextColumn();
                if (ImGui.Button("Download Image")) // 显示按钮
                {
                    // 下载图片
                    var imageUrl = item.ImageUrl; // 假设每个item都有一个ImageUrl属性
                    var imageBytes = httpClient.GetByteArrayAsync(imageUrl).Result ;
                    using (var ms = new MemoryStream(imageBytes))
                    {
                        Bitmap bitmap = (Bitmap)Bitmap.FromStream(ms);

                    }
                }
            }
            */
            ImGui.EndTable();
        }

        ImGui.End(); // 结束ImGui窗口
    }

}
