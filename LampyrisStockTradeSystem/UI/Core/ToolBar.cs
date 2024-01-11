/*
** Author: wushuhong
** Contact: gameta@qq.com
** Description: 主界面 底部工具栏的实现(还在探索中)
*/
namespace LampyrisStockTradeSystem;

using ImGuiNET;

public class ToolBar
{
    private const ImGuiWindowFlags c_imGuiWindowFlags = ImGuiWindowFlags.NoTitleBar |
                                                        ImGuiWindowFlags.NoResize |
                                                        ImGuiWindowFlags.NoScrollbar |
                                                        ImGuiWindowFlags.NoScrollWithMouse |
                                                        ImGuiWindowFlags.NoMove;
    public void OnUpdate()
    {
        // 创建一个停靠在底部的窗口作为工具栏
        ImGui.SetNextWindowDockID(ImGui.GetID("LampyrisDockSpace"), ImGuiCond.FirstUseEver);
        ImGui.Begin("Toolbar", c_imGuiWindowFlags);
        {
            // 设置窗口底部的工具栏
            ImGui.SetCursorPosY(ImGui.GetWindowSize().Y - ImGui.GetFrameHeightWithSpacing());

            // 使用 Columns 来创建一排按钮
            ImGui.Columns(3, "##buttons", false);  // 3个按钮，不显示边界

            // 按钮
            if (ImGui.Button("Button1")) { /* Do something */ }
            ImGui.NextColumn();
            if (ImGui.Button("Button2")) { /* Do something */ }
            ImGui.NextColumn();
            if (ImGui.Button("Button3")) { /* Do something */ }
            ImGui.NextColumn();

            ImGui.Columns(1);  // 切换回一列

            // 显示当前系统时间，靠右显示
            ImGui.SameLine(ImGui.GetWindowWidth() - 150);
            ImGui.Text(DateTime.Now.ToString());
        }
        ImGui.End();
    }
}
