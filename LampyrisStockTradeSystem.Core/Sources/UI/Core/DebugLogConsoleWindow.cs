using ImGuiNET;

namespace LampyrisStockTradeSystem;

[UniqueWidget]
public class DebugLogConsoleWindow : Widget
{
    private List<string> logs = new List<string>();

    private string searchText = string.Empty;

    public override void OnGUI()
    {
        // 搜索框
        ImGui.InputText("##SearchText", ref searchText, 255);

        // 清除按钮
        if (ImGui.Button("清除"))
        {
            DebugConsole.Instance.Clear();
        }

        // 滚动区域
        ImGui.BeginChild("滚动区域");
        foreach (var log in logs)
        {
            if (string.IsNullOrEmpty(searchText) || log.Contains(searchText, StringComparison.OrdinalIgnoreCase))
            {
                ImGui.TextUnformatted(log);
            }
        }
        ImGui.EndChild();
    }
}
