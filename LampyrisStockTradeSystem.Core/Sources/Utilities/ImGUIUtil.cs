/*
** Author: wushuhong
** Contact: gameta@qq.com
** Description: IMGUI实用工具类
*/

using ImGuiNET;
using System.Numerics;
namespace LampyrisStockTradeSystem;

public static class ImGUIUtil
{
    public static void DrawSeparator()
    {

        // 获取当前位置
        var cursorPos = ImGui.GetCursorScreenPos();

        var lineHeight = ImGui.GetFrameHeight();
        // 绘制竖直分隔线
        ImGui.GetWindowDrawList().AddRect(new Vector2(cursorPos.X, cursorPos.Y),
                                          new Vector2(cursorPos.X + 1, cursorPos.Y + lineHeight),
                                          ImGui.GetColorU32(new Vector4(0.15f, 0.15f, 0.15f, 1.0f))); // 使用当前ImGui主题的按钮颜色
        ImGui.SameLine();

        ImGui.Spacing();
        ImGui.SameLine();
    }
}
