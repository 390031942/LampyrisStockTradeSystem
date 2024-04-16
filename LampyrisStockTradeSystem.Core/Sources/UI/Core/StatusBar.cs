/*
** Author: wushuhong
** Contact: gameta@qq.com
** Description: 底部状态栏
*/
using ImGuiNET;

namespace LampyrisStockTradeSystem;

public class StatusBar:Singleton<StatusBar>
{
    public ImGuiWindowFlags windowFlags => ImGuiWindowFlags.NoFocusOnAppearing|ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoScrollbar;

    // 需要展示在状态栏的指数
    private List<string> m_indexBriefCodeList = new List<string>()
    {
        "000001",// 上证指数
        "399001",// 深证成指
        "HSI", // 恒生指数
        "NDX", // 纳斯达克
    };

    public void OnStatusBarGUI()
    {
        float windowWidth = ImGui.GetIO().DisplaySize.X;
        float windowHeight = ImGui.GetIO().DisplaySize.Y;

        ImGui.SetNextWindowPos(new System.Numerics.Vector2(0, windowHeight - 30));
        ImGui.SetNextWindowSize(new System.Numerics.Vector2(windowWidth, 20));

        ImGui.Begin("StatusBar", windowFlags);
        {
            foreach(string code in m_indexBriefCodeList)
            {
                var data = QuoteDatabase.Instance.QueryIndexBriefQuoteData(code);
                if (data != null)
                {
                    ImGui.Text(data.name);
                    ImGui.SameLine();

                    ImGui.PushStyleColor(ImGuiCol.Text,AppUIStyle.Instance.GetRiseFallColor(data.percentage));

                    string priceSign = "";
                    string percentageSign = "";
                    if (data.percentage > 0)
                    {
                        priceSign = "↑ ";
                        percentageSign = "+";
                    }
                    else if (data.percentage < 0)
                    {
                        priceSign = "↓ ";
                    }

                    ImGui.Text(data.currentPrice.ToString());
                    ImGui.SameLine();
                    ImGui.Text(priceSign + data.priceChange.ToString());
                    ImGui.SameLine();
                    ImGui.Text(percentageSign + data.percentage.ToString() + "%%");
                    ImGui.SameLine();

                    ImGui.PopStyleColor();
                }
            }

            // 日止按钮
            if(ImGui.Button("Show Log"))
            {
                WidgetManagement.GetWidget<DebugLogConsoleWindow>();
            }

            // 显示系统时间
            ImGui.SameLine(ImGui.GetIO().DisplaySize.X - 50); // 根据需要调整位置
            ImGui.Text(DateTime.Now.ToString("HH:mm:ss"));
        }
        ImGui.End();
    }
}
