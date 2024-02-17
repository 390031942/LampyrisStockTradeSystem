/*
** Author: wushuhong
** Contact: gameta@qq.com
** Description: K线走势窗口
*/
namespace LampyrisStockTradeSystem;

using ImGuiNET;
using System.Drawing;
using Vector2 = System.Numerics.Vector2;

public class KLineWindowDisplayParam
{
    public QuoteData quoteData;

    public int startIndex;
    public int endIndex;

    public float maxIndex;
    public float maxValue;

    public float minIndex;
    public float minValue;
}

public class KLineWindow:Widget
{
    /// <summary>
    /// K线红色U32值
    /// </summary>
    private uint m_redColor = 0u;

    /// <summary>
    /// K线绿色U32值
    /// </summary>
    private uint m_greenColor = 0u;

    /// <summary>
    /// K线平盘颜色U32值
    /// </summary>
    private uint m_whiteColor = 0u;

    /// <summary>
    /// K线宽度 默认值
    /// </summary>
    private const float c_kLineWidthDefault = 30f;

    /// <summary>
    /// K线间距 默认值
    /// </summary>
    private const float c_kLineSpacingDefault = 5f;

    /// <summary>
    /// K线宽度
    /// </summary>
    private float m_kLineWidth = c_kLineWidthDefault;

    /// <summary>
    /// K线间距
    /// </summary>
    private float m_kLineSpacing = c_kLineSpacingDefault;

    /// <summary>
    /// 当前正在展示的数据的开始和结束索引
    /// </summary>
    private QuotePriceSegmentTree m_segmentTree;
    private KLineWindowDisplayParam m_kLineWindowDisplayParam = new KLineWindowDisplayParam();

    private bool m_needRebuild = false;

    private bool IsMouseHoverWindow
    {
        get
        {
            Vector2 windowPos = ImGui.GetWindowPos();
            Vector2 windowSize = ImGui.GetWindowSize();
            Vector2 pos = ImGui.GetIO().MousePos;
            if (pos.X >= windowPos.X && pos.X <= windowPos.X + windowSize.X &&
                pos.Y >= windowPos.Y && pos.Y <= windowPos.Y + windowSize.Y)
            {
                return true;
            }
            return false;
        }
    }

    private void HandleKeyDown()
    {
        bool hovered = IsMouseHoverWindow;

        if (ImGui.IsKeyDown(ImGuiKey.LeftArrow))
        {
            if (hovered)
            {

            }
        }
        else if (ImGui.IsKeyDown(ImGuiKey.RightArrow))
        {
            if (hovered)
            {

            }
        }
        else if (ImGui.IsKeyDown(ImGuiKey.UpArrow))
        {

        }
        else if (ImGui.IsKeyDown(ImGuiKey.DownArrow))
        {

        }
    }

    public override void OnAwake()
    {
        base.OnAwake();
        m_redColor = ImGui.GetColorU32(new System.Numerics.Vector4(255, 0, 0, 255));
        m_greenColor = ImGui.GetColorU32(new System.Numerics.Vector4(0, 255, 0, 255));
        m_whiteColor = ImGui.GetColorU32(new System.Numerics.Vector4(255, 255, 255, 255));
    }

    public void ResetToDefaultScale()
    {
        m_kLineWidth = c_kLineWidthDefault;
        m_kLineSpacing = c_kLineSpacingDefault;
    }

    public void OnReset()
    {
        ResetToDefaultScale();
    }

    public override void OnGUI()
    {
        // 获取窗口位置和高度
        Vector2 windowPos = ImGui.GetWindowPos();
        Vector2 windowSize = ImGui.GetWindowSize();

        float width = windowSize.X; 
        float height = windowSize.Y;

        if(m_needRebuild)
        {
            int count = (int)Math.Floor(width / m_kLineWidth);

            m_kLineWindowDisplayParam.startIndex = Math.Max(0, m_kLineWindowDisplayParam.quoteData.perDayKLineList.Count - count);
            m_kLineWindowDisplayParam.endIndex = m_kLineWindowDisplayParam.quoteData.perDayKLineList.Count - 1;

            // 判断当前可以显示的K线数量
            (float minValue, int minIndex) = m_segmentTree.QueryMin(m_kLineWindowDisplayParam.startIndex, m_kLineWindowDisplayParam.endIndex);
            (float maxValue, int maxIndex) = m_segmentTree.QueryMax(m_kLineWindowDisplayParam.startIndex, m_kLineWindowDisplayParam.endIndex);

            m_kLineWindowDisplayParam.maxIndex = maxIndex;
            m_kLineWindowDisplayParam.maxValue = maxValue;
            m_kLineWindowDisplayParam.minIndex = minIndex;
            m_kLineWindowDisplayParam.minValue = minValue;

            m_needRebuild = false;
        }

        float startPos = 1.0f;

        for (int i = m_kLineWindowDisplayParam.startIndex; i <= m_kLineWindowDisplayParam.endIndex; i++)
        {
            KLineData kLineData = m_kLineWindowDisplayParam.quoteData.perDayKLineList[i];
            float delta = (m_kLineWindowDisplayParam.maxValue - m_kLineWindowDisplayParam.minValue);

            // 归一化高度
            float highestNormalizedPosY = 1 - (kLineData.highestPrice - m_kLineWindowDisplayParam.minValue) / delta;
            float lowestNormalizedPosY = 1- (kLineData.lowestPrice - m_kLineWindowDisplayParam.minValue) / delta;
            float openNormalizedPosY = 1 - (kLineData.openPrice - m_kLineWindowDisplayParam.minValue) / delta;
            float closeNormalizedPosY = 1 - (kLineData.closePrice - m_kLineWindowDisplayParam.minValue) / delta;

            highestNormalizedPosY = 0.8f * highestNormalizedPosY + 0.2f;
            lowestNormalizedPosY = 0.8f * lowestNormalizedPosY + 0.2f;
            openNormalizedPosY = 0.8f * openNormalizedPosY + 0.2f;
            closeNormalizedPosY = 0.8f * closeNormalizedPosY + 0.2f;

            float highestPosY = height * highestNormalizedPosY;
            float lowestPosY = height * lowestNormalizedPosY;
            float openPosY = height * openNormalizedPosY;
            float closePosY = height * closeNormalizedPosY;

            // 绘制矩形表示开盘价和收盘价
            Vector2 rectMin = new Vector2(startPos, openPosY) + windowPos;
            Vector2 rectMax = new Vector2(startPos + m_kLineWidth, closePosY) + windowPos;

            // 股票k线颜色
            uint color = 0u;

            if (kLineData.openPrice < kLineData.closePrice)
                color = m_redColor;
            else if (kLineData.openPrice > kLineData.closePrice)
                color = m_greenColor;
            else
                color = m_whiteColor;

            // 绘制实体

            // 如果开盘价 == 收盘价，就直接绘制一条白线
            if(color == m_whiteColor)
            {
                ImGui.GetWindowDrawList().AddLine(new Vector2(startPos + windowPos.X, highestPosY + windowPos.Y),
                                                  new Vector2(startPos + m_kLineWidth + windowPos.X, highestPosY + windowPos.Y),
                                                  color);
            }
            else
            {
                ImGui.GetWindowDrawList().AddRectFilled(rectMin, rectMax, color);
            }

            // 绘制上下影线
            if (highestPosY != lowestPosY)
            {
                ImGui.GetWindowDrawList().AddLine(new Vector2(startPos + 0.5f * m_kLineWidth + windowPos.X, highestPosY + windowPos.Y),
                                                  new Vector2(startPos + 0.5f * m_kLineWidth + windowPos.X, lowestPosY + windowPos.Y),
                                                  color);
            }

            // 设置下一根K线的位置
            startPos = startPos + m_kLineWidth + m_kLineSpacing;
        }
    }

    public void ShowQuoteByCode(string code)
    {
        QuoteData quoteData = QuoteDatabase.Instance.QueryQuoteData(code);
        m_kLineWindowDisplayParam.quoteData = quoteData;

        // 构造线段树，方便查询区间内价格的最大最小值
        m_segmentTree = new QuotePriceSegmentTree(quoteData);
        m_needRebuild = true;
    }
}
