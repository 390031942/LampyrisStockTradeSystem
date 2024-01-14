/*
** Author: wushuhong
** Contact: gameta@qq.com
** Description: 主界面 MessageBox的实现
*/
namespace LampyrisStockTradeSystem;

using ImGuiNET;

public class MessageBox : Widget
{
    private string m_title = "MessageBox Default Title";

    private string m_message = "MessageBox Default Message";

    public override string Name => !string.IsNullOrEmpty(m_title) ? m_title:"MessageBox Default Title";

    public override WidgetModel widgetModel => WidgetModel.Normal;

    public override void OnGUI()
    {
        float textWidth = ImGui.CalcTextSize(m_message).X;
        ImGui.SetCursorPosX((ImGui.GetWindowSize().X - textWidth) / 2);

        ImGui.Text(m_message); // 显示内容
        ImGui.Spacing();
        ImGui.SetCursorPosX(ImGui.GetWindowSize().X / 2 - 70);
        if (ImGui.Button("OK")) // 确定按钮
        {
            ImGui.CloseCurrentPopup();
            isOpened = false;
        }

        ImGui.SameLine();

        ImGui.SetCursorPosX(ImGui.GetWindowSize().X / 2 + 70);
        if (ImGui.Button("Cancel")) // 取消按钮
        {
            ImGui.CloseCurrentPopup();
            isOpened = false;
        }

        ImGui.EndPopup();
    }

    public void SetContent(string title, string content)
    {
        this.m_title = title;
        this.m_message = content;
    }
}
