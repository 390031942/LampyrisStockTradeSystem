/*
** Author: wushuhong
** Contact: gameta@qq.com
** Description:东方财富通交易模式选择 窗口
*/
using ImGuiNET;

namespace LampyrisStockTradeSystem;

[Serializable]
public class EastMoneyTradeModeSetting:SerializableSingleton<EastMoneyTradeModeSetting>
{
    public int activeMode = 1;
}

[UniqueWidget]
public class EastMoneyTradeModeSelectWindow:Widget
{
    public override string Name => "东方财富通交易模式选择";

    public override WidgetModel widgetModel => WidgetModel.PopupModal;

    private int m_tempIndex;

    public override void OnAwake()
    {
        base.OnAwake();
        m_tempIndex = EastMoneyTradeModeSetting.Instance.activeMode;
    }


    public override void OnGUI()
    {
        for(int i = 0; i < (int)EastMoneyTradeMode.Count; i++)
        {
            if (ImGui.RadioButton(EastMoneyTradeModeName.Instance[(EastMoneyTradeMode)i],m_tempIndex == i))
            {
                m_tempIndex = i;
            }
            if ((i + 1) < (int)EastMoneyTradeMode.Count)
            {
                ImGui.SameLine();
            }
        }

        if(ImGui.Button("保存"))
        {
            EastMoneyTradeModeSetting.Instance.activeMode = m_tempIndex ;
            this.isOpened = false;
        }

        ImGui.SameLine();

        if (ImGui.Button("取消"))
        {
            this.isOpened = false;
        }
    }

    [MenuItem("交易/交易模式选择")]
    public static void SelectTradeMode()
    {
         WidgetManagement.GetWidget<EastMoneyTradeModeSelectWindow>();
    }
}
