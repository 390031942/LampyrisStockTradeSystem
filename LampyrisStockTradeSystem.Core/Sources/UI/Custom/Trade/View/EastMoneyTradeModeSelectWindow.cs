using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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


    public override void OnGUI()
    {
        int activeIndex = 0;
        for(int i = 0; i < (int)EastMoneyTradeMode.Count; i++)
        {
            if (ImGui.RadioButton(EastMoneyTradeModeName.Instance[(EastMoneyTradeMode)i],EastMoneyTradeModeSetting.Instance.activeMode == i))
            {
                activeIndex = i;
            }
            if ((i + 1) < (int)EastMoneyTradeMode.Count)
            {
                ImGui.SameLine();
            }
        }

        if(ImGui.Button("保存"))
        {
            EastMoneyTradeModeSetting.Instance.activeMode = activeIndex;
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
