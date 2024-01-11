namespace LampyrisStockTradeSystem;

using ImGuiNET;
using Vector2 = System.Numerics.Vector2;

public class StockKLineWindow
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
    /// K线宽度 默认值
    /// </summary>
    private const float c_kLineWidthDefault = 30f;

    /// <summary>
    /// K线间距 默认值
    /// </summary>
    private const float c_kLineSpacingDefault = 12f;

    /// <summary>
    /// K线宽度
    /// </summary>
    private float m_kLineWidth = c_kLineWidthDefault;

    /// <summary>
    /// K线间距
    /// </summary>
    private float m_kLineSpacing = c_kLineSpacingDefault;

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

    public void OnStart()
    {
        m_redColor = ImGui.GetColorU32(new System.Numerics.Vector4(255, 0, 0, 255));
        m_greenColor = ImGui.GetColorU32(new System.Numerics.Vector4(0, 255, 0, 255));
    }

    public void OnUpdate()
    {
        var kLineDataList = StockDatabase.GetStockData("000001");

        // 绘制K线图
        ImGui.Begin("K线行情");
        ImGui.BeginGroup();
        Vector2 windowPos = ImGui.GetWindowPos();
        Vector2 windowSize = ImGui.GetWindowSize();

        if (kLineDataList != null)
        {
            for (int i = 0; i < kLineDataList.Count; i++)
            {
                // 股票k线数据
                StockKLineData data = kLineDataList[i];

                // 股票k线颜色
                uint color = data.openPrice <= data.closePrice ? m_redColor : m_greenColor;

                // 绘制线条表示最高价和最低价
                Vector2 p1 = new Vector2(i * 10 - 0.55f, data.highestPrice);
                Vector2 p2 = new Vector2(i * 10 - 0.55f, data.lowestPrice);
                ImGui.GetWindowDrawList().AddLine(windowPos + p1, windowPos + p2, color);

                // 绘制矩形表示开盘价和收盘价
                Vector2 rectMin = new Vector2(i * 10 - 2, Math.Min(data.openPrice, data.closePrice)) + windowPos;
                Vector2 rectMax = new Vector2(i * 10 + 2, Math.Max(data.openPrice, data.closePrice)) + windowPos;
                ImGui.GetWindowDrawList().AddRectFilled(rectMin, rectMax, color);
            }
        }

        var pos = ImGui.GetIO().MousePos;
        if (pos.X >= windowPos.X && pos.X <= windowPos.X + windowSize.X &&
           pos.Y >= windowPos.Y && pos.Y <= windowPos.Y + windowSize.Y)
        {
            // 十字线 横向
            ImGui.GetWindowDrawList().AddLine(new Vector2(windowPos.X, pos.Y),
                                              new Vector2(windowPos.X + windowSize.X, pos.Y),
                                              ImGui.GetColorU32(ImGuiCol.PlotLines));
            // 十字线 竖向
            ImGui.GetWindowDrawList().AddLine(new Vector2(pos.X, windowPos.Y),
                                              new Vector2(pos.X, windowPos.Y + windowSize.Y),
                                              ImGui.GetColorU32(ImGuiCol.PlotLines));
        }
        ImGui.EndGroup();
        ImGui.End();
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

}
