/*
** Author: wushuhong
** Contact: gameta@qq.com
** Description: 主界面 进度条窗口的实现
*/
namespace LampyrisStockTradeSystem;

using ImGuiNET;
using System.Numerics;

public class AsyncOperationProgressShowDialog : Widget
{
    private string m_title = "ProgressShowDialog Default Title";

    private string m_message = "ProgressShowDialog Default Message";

    public override string Name => !string.IsNullOrEmpty(m_title) ? m_title : "ProgressShowDialog Default Title";

    public override WidgetModel widgetModel => WidgetModel.PopupModal;

    public override ImGuiWindowFlags windowFlags => base.windowFlags | ImGuiWindowFlags.NoTitleBar;

    private AsyncOperation m_asyncOperation;

    public override void OnBeforeGUI()
    {
        size = new Vector2(450, 150);
    }

    public override void OnGUI()
    {
        ImGui.Text(m_message);

        if (m_asyncOperation == null)
        {
            ImGui.Text("没有指定AsyncOperation");
            if (ImGui.Button("关闭"))
            {
                this.isOpened = false;
            }
            return;
        }
        ImGui.Text("进度");
        ImGui.SameLine();
        ImGui.ProgressBar(m_asyncOperation.progress,new Vector2(300,24));
        ImGui.SameLine();

        if (!m_asyncOperation.finished) ImGui.BeginDisabled();
        {
            if (ImGui.Button("关闭"))
            {
                this.isOpened = false;
            }
        }
        if (!m_asyncOperation.finished) ImGui.EndDisabled();
    }

    public void SetContent(string title, string content)
    {
        this.m_title = title;
        this.m_message = content;
    }

    public void SetAsyncOperation(AsyncOperation asyncOperation)
    {
        m_asyncOperation = asyncOperation;  
    }
}
