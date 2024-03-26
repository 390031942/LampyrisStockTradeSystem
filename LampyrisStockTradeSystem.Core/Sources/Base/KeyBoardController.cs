/*
** Author: wushuhong
** Contact: gameta@qq.com
** Description: 程序的 键盘逻辑处理
*/
using ImGuiNET;

namespace LampyrisStockTradeSystem;

public static class KeyBoardController
{
    private static List<ImGuiKey> m_letterKeyCodeList = new List<ImGuiKey>()
    {
        ImGuiKey.A,ImGuiKey.B,ImGuiKey.C, ImGuiKey.D,ImGuiKey.E,ImGuiKey.F, ImGuiKey.G,
        ImGuiKey.H,ImGuiKey.I, ImGuiKey.J,ImGuiKey.K,ImGuiKey.L,ImGuiKey.M,ImGuiKey.N,
        ImGuiKey.O, ImGuiKey.P,ImGuiKey.Q,ImGuiKey.R, ImGuiKey.S,ImGuiKey.T, ImGuiKey.U,
        ImGuiKey.V,ImGuiKey.W, ImGuiKey.X,  ImGuiKey.Y,ImGuiKey.Z
    };

    private static List<ImGuiKey> m_numberKeyCodeList = new List<ImGuiKey>()
    {
        ImGuiKey._0,ImGuiKey._1,ImGuiKey._2,ImGuiKey._3,ImGuiKey._4,ImGuiKey._5,
        ImGuiKey._6,ImGuiKey._7,ImGuiKey._8,ImGuiKey._9,
    };

    public static void Update()
    {
        if (!ImGui.GetIO().WantTextInput)
        {
            if (ImGui.IsKeyDown(ImGuiKey.A))
            {
                WidgetManagement.GetWidget<SearchWindow>();
            }
        }
    }

    /// <summary>
    /// 处理字符串的输入，包括了输入法的输入和按键输入
    /// </summary>
    /// <param name="asString"></param>
    public static void HandleTextInput(string inputString)
    {
        // 如果没有在输入框的焦点上,那么就要弹出股票代码搜索窗口
        if (!ImGui.GetIO().WantTextInput)
        {
            SearchWindow window = WidgetManagement.GetWidget<SearchWindow>();
            window.SetInputCodeAndFoucs(inputString);
        }
    }
}
