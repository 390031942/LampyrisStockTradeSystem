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

    public void OnStatusBarGUI()
    {
        float windowWidth = ImGui.GetIO().DisplaySize.X;
        float windowHeight = ImGui.GetIO().DisplaySize.Y;

        ImGui.SetNextWindowPos(new System.Numerics.Vector2(0, windowHeight - 30));
        ImGui.SetNextWindowSize(new System.Numerics.Vector2(windowWidth, 20));

        ImGui.Begin("StatusBar", windowFlags);
        {
            ImGui.Text("上证指数");
            ImGui.SameLine();
            ImGui.Text("3069.30");
            ImGui.SameLine();
            ImGui.Text("-5.66");
            ImGui.SameLine();
            ImGui.Text("-0.18%");
            ImGui.SameLine();
            ImGui.Text("3927.0亿");
            ImGui.SameLine();

            ImGui.Text("深证成指");
            ImGui.SameLine();
            ImGui.Text("9544.77");
            ImGui.SameLine();
            ImGui.Text("-42.18");
            ImGui.SameLine();
            ImGui.Text("-0.44%");
            ImGui.SameLine();
            ImGui.Text("5252.4亿");
            ImGui.SameLine();

            ImGui.Text("恒生指数");
            ImGui.SameLine();
            ImGui.Text("16725.10");
            ImGui.SameLine();
            ImGui.Text("-206.42");
            ImGui.SameLine();
            ImGui.Text("-1.22%");
            ImGui.SameLine();
            ImGui.Text("997.87亿");

            ImGui.SameLine();
            // 显示系统时间
            ImGui.SameLine(ImGui.GetIO().DisplaySize.X - 100); // 根据需要调整位置
            ImGui.Text(DateTime.Now.ToString("HH:mm:ss"));
        }
        ImGui.End();
    }
}
