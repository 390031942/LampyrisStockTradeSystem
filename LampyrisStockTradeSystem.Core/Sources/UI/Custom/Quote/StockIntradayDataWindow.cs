/*
** Author: wushuhong
** Contact: gameta@qq.com
** Description: 股票分时走势窗口
*/
using ImGuiNET;
using LafpyrisStockTradeSystef;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Numerics;

namespace LampyrisStockTradeSystem;

// 记录了不同市场开市时间段数据，如：
// 1. 沪深A股，9:30-11:30 13:00-15:00,需要在时间坐标上显示的是10:30,11:30,13:30和14:30
// 2. 港股, 9:30-12:00 13:00-16:00,需要在时间坐标上显示的是10:00,11:00,12:00,14:00,15:00和16:00
// 等等
public abstract class MarketTimePeriodData
{
    // 时间段信息, 记录了开市时间段的信息，如[09:30,12:00],[13:00,16:00]
    protected List<Tuple<DateTime, DateTime>> m_periodDataList;

    protected List<int> m_needDisplayTimeCoordinateIndex;

    protected int m_minuteCount = -1;

    protected Dictionary<int, string> m_needShowTimeCoordinateDataMap;

    protected abstract void Register();

    public MarketTimePeriodData()
    {
        Register();
    }

    public int minuteCount
    {
        get
        {
            if(m_minuteCount == -1)
            {
                m_minuteCount = 0;
                for (int i = 0; i < m_periodDataList.Count; i++)
                {
                    var period = m_periodDataList[i];
                    m_minuteCount += (int)(period.Item2 - period.Item1).TotalMinutes;

                    if(i == 0) // 开盘价的数据会变成独立一分钟的数据，比如09:30的数据就是开盘价的数据，所以这里数量得+1
                    {
                        m_minuteCount += 1;
                    }
                }
            }

             return m_minuteCount;
        }
    }

    public string GetMinuteStringByIndex(int index)
    {
        if(index >= 0 && index <= minuteCount - 1)
        {
            int sum = 0;
            for(int i = 0;i < minuteCount; i++) 
            {
                var period = m_periodDataList[i];
                int diffMin = (period.Item2 - period.Item1).Minutes;
                if(index < (sum + diffMin))
                {
                    // 以港股为例，index = 0取到的是开盘价的成交数据，也就是09:30的数据
                    // index = 1，拿到的是09:31的数据
                    // index = 150,拿到的是12:00的数据，
                    // 注意：index = 151,拿到的不是13:00的数据，而应该是13:01的数据，因为第二段开市时间段没有开盘价
                    return (period.Item1.AddMinutes(index - sum + ((i != 0) ? 1 : 0))).ToString("mm:ss");
                }
                sum += diffMin;
            }
        }

        return "";
    }

    public Dictionary<int,string> GetNeedShowTimeCoordinateDataMap()
    {
        if(m_needShowTimeCoordinateDataMap == null)
        {
            m_needShowTimeCoordinateDataMap = new Dictionary<int, string>()
            {
                {1,"10:00"},
                {3,"11:00"},
                {5,"12:00"},
                {7,"14:00"},
                {9,"15:00"},
                {11,"16:00"},
            };
        }

        return m_needShowTimeCoordinateDataMap;
    }
}

public class HKMarketTimePeriodData: MarketTimePeriodData
{
    public HKMarketTimePeriodData():base() { }

    protected override void Register()
    {
        m_periodDataList = new List<Tuple<DateTime, DateTime>>
        {
            new Tuple<DateTime, DateTime>(DateTime.Parse("09:30"),DateTime.Parse("12:00")),
            new Tuple<DateTime, DateTime>(DateTime.Parse("13:00"),DateTime.Parse("16:00")),
        };

        m_needDisplayTimeCoordinateIndex = new List<int>()
        {
            1,3,5,7,9,11
        };
    }
}

[UniqueWidget]
public class StockIntradayDataWindow : Widget
{
    private List<string> m_strings = new List<string>();

    private uint m_whiteColorId = ImGui.ColorConvertFloat4ToU32(AppUIStyle.Instance.normalWhiteColor);
    
    private uint m_gridColorId = ImGui.ColorConvertFloat4ToU32(new Vector4(0.235f, 0.235f, 0.235f, 1.0f));

    private uint m_focusPanelColorId = ImGui.ColorConvertFloat4ToU32(new Vector4(0.168f, 0.168f, 0.168f, 1.0f));

    private uint m_redColorId = ImGui.ColorConvertFloat4ToU32(AppUIStyle.Instance.quoteRiseColor);

    private uint m_greenColorId = ImGui.ColorConvertFloat4ToU32(AppUIStyle.Instance.quoteFallColor);

    private uint m_darkRedColorId = ImGui.ColorConvertFloat4ToU32(new Vector4(0.5f,0.137f,0.137f,1.0f));

    private uint m_darkGreenColorId = ImGui.ColorConvertFloat4ToU32(new Vector4(0.275f, 0.612f, 0.102f, 1.0f));

    private uint m_moneyColor = ImGui.ColorConvertFloat4ToU32(new Vector4(0.0f, 1.0f, 1.0f, 1.0f));

    private float m_gridWidth = 520;

    private float m_pricePlotHeight = 250;

    private float m_volumnPlotHeight = 115;

    private int m_pricePlotGridCountX = 11;

    private int m_pricePlotGridCountY = 10;

    private int m_volumnPlotGridCountY = 6;

    private Vector2 m_textRelativePos = new Vector2(120, 28);

    private float m_pricePlotRelativePosY = 50;

    private float m_volumnPlotRelativePosY = 320;

    private float m_bottomTextRelativePosY = 480;

    private string m_code;

    private string m_name;

    private MarketTimePeriodData m_marketTimePeriodData = new HKMarketTimePeriodData();

    private StockRealTimeQuoteData m_realTimeQuoteData;

    private bool m_needSetFocus = true;

    // 行情总分钟数量
    private int m_quoteMinuteCount => m_marketTimePeriodData.minuteCount;

    // 需要在时间坐标轴上显示的时刻
    private Dictionary<int,string> m_timeStringMap => m_marketTimePeriodData.GetNeedShowTimeCoordinateDataMap();


    public override ImGuiWindowFlags windowFlags => ImGuiWindowFlags.NoResize| ImGuiWindowFlags.NoSavedSettings| ImGuiWindowFlags.NoDocking;

    private string m_tipMessage = "暂无股票数据，请检查数据传递是否正确";

    private Vector2 m_tipMessageTextSize;

    private float m_priceDelta;

    private int m_timerId = -1;

    private Vector2 m_mousePosLast;

    private int m_foucsIndexLsst = -1;

    public override void OnAwake()
    {
        base.OnAwake();
        size = new Vector2(710, 513);
        m_tipMessageTextSize = ImGui.CalcTextSize(m_tipMessage);
        CallTimer.Instance.SetInterval(RequestQuote, 3000);
    }

    public override void OnDestroy()
    {
        if(m_timerId > 0)
        {
            CallTimer.Instance.ClearTimer(m_timerId);
        }
        base.OnDestroy();
    }

    public void SetQuoteData(string code,string name, StockRealTimeQuoteData realTimeQuoteData)
    {
        m_code = code;
        m_name = name;

        m_realTimeQuoteData = realTimeQuoteData;
        if(m_realTimeQuoteData != null)
        {
            m_priceDelta = HKStockPriceRange.GetPriceRange(m_realTimeQuoteData.kLineData.lastClosePrice).priceDelta;
        }
        m_needSetFocus = true;
    }

    public override void OnBeforeGUI()
    {
        base.OnBeforeGUI();
        if (m_needSetFocus)
        { 
            ImGui.SetNextWindowFocus();
            m_needSetFocus = false;
        }
    }

    public override void OnGUI()
    {
        var windowPos = ImGui.GetWindowPos();
        var windowSize = ImGui.GetWindowSize();
        var drawList = ImGui.GetWindowDrawList();

        if(m_realTimeQuoteData == null)
        {
            float x = windowPos.X + windowSize.X / 2 - m_tipMessageTextSize.X / 2;
            float y = windowPos.Y + windowSize.Y / 2 - m_tipMessageTextSize.Y / 2;

            drawList.AddText(new Vector2(x,y), m_whiteColorId, m_tipMessage);

            return;
        }

        // 计算最大涨跌额 绝对值 和最大 涨跌幅的绝对值
        float maxAbsPriceChange = Math.Max(Math.Abs(m_realTimeQuoteData.kLineData.highestPrice - m_realTimeQuoteData.kLineData.lastClosePrice),
                                           Math.Abs(m_realTimeQuoteData.kLineData.lowestPrice - m_realTimeQuoteData.kLineData.lastClosePrice));

        float maxAbsPercentage = Math.Max((m_realTimeQuoteData.kLineData.highestPrice - m_realTimeQuoteData.kLineData.lastClosePrice) / m_realTimeQuoteData.kLineData.lastClosePrice,
                                       (m_realTimeQuoteData.kLineData.lowestPrice - m_realTimeQuoteData.kLineData.lastClosePrice) / m_realTimeQuoteData.kLineData.lastClosePrice);

        // 最小的最大 涨跌额/幅 应该根据网格的数量以及最小价差来计算
        float minMaxAbsPriceChange = (m_pricePlotGridCountY - 1) / 2 * m_priceDelta;
        float minMaxAbsPercentage = ((m_realTimeQuoteData.kLineData.lastClosePrice + minMaxAbsPriceChange) / m_realTimeQuoteData.kLineData.lastClosePrice) - 1;

        // maxAbsPriceChange = Math.Max(maxAbsPriceChange, minMaxAbsPriceChange);
        // maxAbsPercentage = Math.Max(maxAbsPercentage, minMaxAbsPercentage);

        /* START OF 绘制顶部价格信息 */
        m_strings.Clear();
        m_strings.Add(m_code + " " + m_name);
        m_strings.Add("价格:");
        m_strings.Add(m_realTimeQuoteData.kLineData.closePrice.ToString());
        m_strings.Add("涨幅:");
        m_strings.Add(m_realTimeQuoteData.kLineData.percentage + "%");
        m_strings.Add("成交额:");
        m_strings.Add(StringUtility.GetMoneyString(m_realTimeQuoteData.kLineData.money));
        m_strings.Add("换手率:");
        m_strings.Add(m_realTimeQuoteData.kLineData.turnOverRate + "%");

        var textDrawStartPos = windowPos + m_textRelativePos;

        foreach (var str in m_strings)
        {
            drawList.AddText(textDrawStartPos, m_whiteColorId, str);
            textDrawStartPos.X += 2 + ImGui.CalcTextSize(str).X;
        }
        /* END OF 绘制顶部价格信息 */

        /* START OF 绘制分时折线图 */
        float pricePlotCellWidth = m_gridWidth / m_pricePlotGridCountX;
        float pricePlotCellHeight = m_pricePlotHeight / m_pricePlotGridCountY;

        float pricePlotStartX = (windowSize.X - m_gridWidth) / 2.0f;
        float pricePlotEndX = windowSize.X - pricePlotStartX;

        for (int i = 0; i <= m_pricePlotGridCountX; i++)
        {
            float x = windowPos.X + pricePlotStartX + i * pricePlotCellWidth;
            drawList.AddLine(new Vector2(x, windowPos.Y + m_pricePlotRelativePosY), new Vector2(x, windowPos.Y + m_pricePlotRelativePosY + m_pricePlotHeight), m_gridColorId);
        }

        for (int j = 0; j <= m_pricePlotGridCountY; j++)
        {
            float y = windowPos.Y + m_pricePlotRelativePosY + j * pricePlotCellHeight;
            drawList.AddLine(new Vector2(windowPos.X + pricePlotStartX, y), new Vector2(windowPos.X + pricePlotStartX + m_gridWidth, y), m_gridColorId);
        }

        // 价格于百分比刻度
        drawList.AddText(new Vector2(windowPos.X + pricePlotStartX - 45, windowPos.Y + m_pricePlotRelativePosY - 5), m_redColorId, Math.Round(m_realTimeQuoteData.kLineData.lastClosePrice + maxAbsPriceChange,3).ToString());
        drawList.AddText(new Vector2(windowPos.X + pricePlotEndX + 5, windowPos.Y + m_pricePlotRelativePosY - 5), m_redColorId, (float)Math.Round(maxAbsPercentage * 100, 2) + "%");

        drawList.AddText(new Vector2(windowPos.X + pricePlotStartX - 45, windowPos.Y + m_pricePlotRelativePosY + (m_pricePlotGridCountY / 2) * pricePlotCellHeight - 5), m_whiteColorId, m_realTimeQuoteData.kLineData.lastClosePrice.ToString());
        drawList.AddText(new Vector2(windowPos.X + pricePlotEndX + 5, windowPos.Y + m_pricePlotRelativePosY + (m_pricePlotGridCountY / 2) * pricePlotCellHeight - 5), m_whiteColorId, "0.00%");

        drawList.AddText(new Vector2(windowPos.X + pricePlotStartX - 45, windowPos.Y + m_pricePlotRelativePosY + (m_pricePlotGridCountY) * pricePlotCellHeight - 5), m_greenColorId,  Math.Round(m_realTimeQuoteData.kLineData.lastClosePrice - maxAbsPriceChange,3).ToString());
        drawList.AddText(new Vector2(windowPos.X + pricePlotEndX + 5, windowPos.Y + m_pricePlotRelativePosY + (m_pricePlotGridCountY) * pricePlotCellHeight - 5), m_greenColorId, (-(float)Math.Round(maxAbsPercentage * 100, 2)) + "%");

        /* END OF 绘制分时折线图 */

        float maxVol = -1.0f;
        for (int i = 0; i < m_realTimeQuoteData.minuteData.Count; i++)
        {
            maxVol = Math.Max(maxVol, m_realTimeQuoteData.minuteData[i].volume);
        }

        /* START OF 绘制量能图 */
        float volumnPlotCellWidth = m_gridWidth / m_pricePlotGridCountX;
        float volumnPlotCellHeight = m_volumnPlotHeight / m_volumnPlotGridCountY;

        for (int i = 0; i <= m_pricePlotGridCountX; i++)
        {
            float x = windowPos.X + pricePlotStartX + i * volumnPlotCellWidth;
            drawList.AddLine(new Vector2(x, windowPos.Y + m_volumnPlotRelativePosY), new Vector2(x, windowPos.Y + m_volumnPlotRelativePosY + m_volumnPlotHeight), m_gridColorId);
            // 时间刻度
            if (m_timeStringMap.ContainsKey(i))
            {
                drawList.AddText(new Vector2(x - 17, windowPos.Y + m_volumnPlotRelativePosY + m_volumnPlotHeight), m_whiteColorId, m_timeStringMap[i]);
            }
        }

        for (int j = 0; j <= m_volumnPlotGridCountY; j++)
        {
            float y = windowPos.Y + m_volumnPlotRelativePosY + j * volumnPlotCellHeight;
            drawList.AddLine(new Vector2(windowPos.X + pricePlotStartX, y), new Vector2(windowPos.X + pricePlotStartX + m_gridWidth, y), m_gridColorId);
            // 成交量刻度
            if (j <= m_volumnPlotGridCountY)
            {
                drawList.AddText(new Vector2(windowPos.X + pricePlotStartX - 45, y), m_whiteColorId, StringUtility.GetMoneyString((m_volumnPlotGridCountY - j )* maxVol / m_volumnPlotGridCountY));
            }
        }

        /* END OF 绘制量能图 */

        int mouseAtIndex = -1;
        // 绘制鼠标焦点线
        Vector2 mousePos = ImGui.GetIO().MousePos;
        float mousePosX = mousePos.X;
        if ((mousePosX >= windowPos.X + pricePlotStartX) && mousePosX <= windowPos.X + pricePlotEndX)
        {
            // 计算鼠标位置所在分钟数，也就是说竖线的位置必须与某一分钟的行情数据对齐
            mouseAtIndex = (int)Math.Round((mousePosX - pricePlotStartX - windowPos.X) / m_gridWidth * m_quoteMinuteCount);

            if(m_mousePosLast == mousePos)
            {
                if (ImGui.IsKeyPressed(ImGuiKey.RightArrow))
                {
                    mouseAtIndex = Math.Max(0, m_foucsIndexLsst + 1);
                }
                else if (ImGui.IsKeyPressed(ImGuiKey.LeftArrow))
                {
                    mouseAtIndex = Math.Min(m_realTimeQuoteData.minuteData.Count, m_foucsIndexLsst - 1);
                }
                else
                {
                    mouseAtIndex = m_foucsIndexLsst;
                }
            }
            if (mouseAtIndex < m_realTimeQuoteData.minuteData.Count)
            {
                mousePosX = (mouseAtIndex / (float)m_quoteMinuteCount * m_gridWidth) + pricePlotStartX + windowPos.X;

                // 走势部分
                drawList.AddLine(new Vector2(mousePosX, windowPos.Y + m_pricePlotRelativePosY), new Vector2(mousePosX, windowPos.Y + m_pricePlotRelativePosY + m_pricePlotHeight), m_whiteColorId);
                // 成交额部分
                drawList.AddLine(new Vector2(mousePosX, windowPos.Y + m_volumnPlotRelativePosY), new Vector2(mousePosX, windowPos.Y + m_volumnPlotRelativePosY + m_volumnPlotHeight), m_whiteColorId);
                // 信息面板部分
                drawList.AddRectFilled(new Vector2(windowPos.X, windowPos.Y + m_pricePlotRelativePosY - 3), new Vector2(windowPos.X + 95, windowPos.Y + m_pricePlotRelativePosY + m_pricePlotHeight), m_focusPanelColorId);

                var data = m_realTimeQuoteData.minuteData[mouseAtIndex];

                float percentage = (float)Math.Round((data.closePrice / m_realTimeQuoteData.kLineData.lastClosePrice - 1.0f) * 100f, 2);
                uint riseDownColor = ImGui.ColorConvertFloat4ToU32(AppUIStyle.Instance.GetRiseFallColor(percentage));

                // 价格
                drawList.AddText(new Vector2(windowPos.X + 15, windowPos.Y + m_pricePlotRelativePosY + 0), m_whiteColorId, "时间");
                drawList.AddText(new Vector2(windowPos.X + 15, windowPos.Y + m_pricePlotRelativePosY + 20), m_whiteColorId, data.date.ToString("HH:mm"));

                drawList.AddText(new Vector2(windowPos.X + 15, windowPos.Y + m_pricePlotRelativePosY + 40), m_whiteColorId, "价格");
                drawList.AddText(new Vector2(windowPos.X + 15, windowPos.Y + m_pricePlotRelativePosY + 60), m_whiteColorId, data.closePrice.ToString());

                drawList.AddText(new Vector2(windowPos.X + 15, windowPos.Y + m_pricePlotRelativePosY + 80), m_whiteColorId, "涨跌额");
                drawList.AddText(new Vector2(windowPos.X + 15, windowPos.Y + m_pricePlotRelativePosY + 100), riseDownColor, Math.Round((data.closePrice - m_realTimeQuoteData.kLineData.lastClosePrice),3).ToString());

                drawList.AddText(new Vector2(windowPos.X + 15, windowPos.Y + m_pricePlotRelativePosY + 120), m_whiteColorId, "涨跌幅");
                drawList.AddText(new Vector2(windowPos.X + 15, windowPos.Y + m_pricePlotRelativePosY + 140), riseDownColor, Math.Round((data.closePrice / m_realTimeQuoteData.kLineData.lastClosePrice - 1.0f) * 100f,2) + "%");

                drawList.AddText(new Vector2(windowPos.X + 15, windowPos.Y + m_pricePlotRelativePosY + 160), m_whiteColorId, "成交量");
                drawList.AddText(new Vector2(windowPos.X + 15, windowPos.Y + m_pricePlotRelativePosY + 180), m_whiteColorId, StringUtility.GetMoneyString(data.volume) + "股");

                drawList.AddText(new Vector2(windowPos.X + 15, windowPos.Y + m_pricePlotRelativePosY + 200), m_whiteColorId, "成交额");
                drawList.AddText(new Vector2(windowPos.X + 15, windowPos.Y + m_pricePlotRelativePosY + 220), m_moneyColor, StringUtility.GetMoneyString(data.money));

                m_foucsIndexLsst = mouseAtIndex;
            }
        }
        else
        {
            m_foucsIndexLsst = -1;
        }

        // 底部操作提示
        string bottomStr = "用鼠标可以选中其所在位置的分钟数据，用键盘【左右键】选择前一/后一分钟的数据";
        float textWidth = ImGui.CalcTextSize(bottomStr).X;
        drawList.AddText(new Vector2(windowPos.X + (windowSize.X - textWidth) / 2.0f, windowPos.Y + m_bottomTextRelativePosY), m_whiteColorId, bottomStr);

        // 折线图

        for (int i = 0; i < m_realTimeQuoteData.minuteData.Count; i++)
        {
            float x1 = windowPos.X + pricePlotStartX + (i - 1) / (float)m_quoteMinuteCount * m_gridWidth;
            float x2 = windowPos.X + pricePlotStartX + (i) / (float)m_quoteMinuteCount * m_gridWidth;

            float c1 = 0.0f;

            if (i == 0)
            {
                c1 = 0.5f + 0.5f * (m_realTimeQuoteData.kLineData.openPrice - m_realTimeQuoteData.kLineData.lastClosePrice) / m_realTimeQuoteData.kLineData.lastClosePrice / maxAbsPercentage;
            }
            else
            {
                c1 = 0.5f + 0.5f * ((m_realTimeQuoteData.minuteData[i - 1].closePrice - m_realTimeQuoteData.kLineData.lastClosePrice) / m_realTimeQuoteData.kLineData.lastClosePrice / maxAbsPercentage);
            }
            float c2 = 0.5f + 0.5f * ((m_realTimeQuoteData.minuteData[i].closePrice - m_realTimeQuoteData.kLineData.lastClosePrice) / m_realTimeQuoteData.kLineData.lastClosePrice / maxAbsPercentage);

            float y1 = windowPos.Y + m_pricePlotRelativePosY + (1 - c1) * m_pricePlotHeight;
            float y2 = windowPos.Y + m_pricePlotRelativePosY + (1 - c2) * m_pricePlotHeight;

            drawList.AddLine(new Vector2(x1, y1), new Vector2(x2, y2), m_whiteColorId);
        }

        for (int i = 0; i < m_realTimeQuoteData.minuteData.Count; i++)
        {
            float x1 = windowPos.X + pricePlotStartX + (i) / (float)m_quoteMinuteCount * m_gridWidth;
            float x2 = windowPos.X + pricePlotStartX + (i + 1) / (float)m_quoteMinuteCount * m_gridWidth;

            float y1 = windowPos.Y + m_volumnPlotRelativePosY + (1 - m_realTimeQuoteData.minuteData[i].volume / maxVol) * m_volumnPlotHeight;
            float y2 = windowPos.Y + m_volumnPlotRelativePosY + m_volumnPlotHeight;

            drawList.AddRectFilled(new Vector2(x1, y1), new Vector2(x2, y2), m_realTimeQuoteData.minuteData[i].percentage >= 0f ? (mouseAtIndex == i ? m_darkRedColorId : m_redColorId) : (mouseAtIndex == i ? m_darkGreenColorId : m_greenColorId));
        }

        m_mousePosLast = mousePos;
    }

    private void RequestQuote()
    {
        if (m_realTimeQuoteData == null)
            return;

        string url = StockQuoteInterface.Instance.GetQuoteUrl(StockQuoteInterfaceType.KLineData, "1", UrlUtil.GetStockCodeParam(m_code), "19900101", "20991231");

        HttpRequest.Get(url, (json) =>
        {
            string strippedJson = JsonStripperUtil.GetEastMoneyStrippedJson(json);
            JObject jsonRoot = JObject.Parse(strippedJson);

            JArray stockDataArray = jsonRoot?["data"]?["klines"]?.ToObject<JArray>();
            if (stockDataArray != null)
            {
                m_realTimeQuoteData.kLineData.lastClosePrice = jsonRoot?["data"]?["preKPrice"]?.SafeToObject<float>() ?? 0.0f;
                m_realTimeQuoteData.minuteData.Clear();
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

                        m_realTimeQuoteData.minuteData.Add(kLineData);
                    }
                }
            }
        });
    }

    [MenuItem("调试/分时图")]
    public static void ShowWin()
    {
        WidgetManagement.GetWidget<StockIntradayDataWindow>();
    }
}
