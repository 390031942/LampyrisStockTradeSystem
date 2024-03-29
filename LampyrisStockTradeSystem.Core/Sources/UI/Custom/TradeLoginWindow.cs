/*
** Author: wushuhong
** Contact: gameta@qq.com
** Description: 东方财富网页端交易登录
*/
namespace LampyrisStockTradeSystem;

using ImGuiNET;
using System;

[Serializable]
public class TradeLoginInfo:SerializableSingleton<TradeLoginInfo>
{
    public string account = "";
    public string password = "";
}

[UniqueWidget]
public class TradeLoginWindow : Widget
{
    private int m_textureID = -1;

    private string m_inputText = "";

    public override void OnGUI()
    { 
        ImGui.InputText("账号", ref TradeLoginInfo.Instance.account, 100);
        ImGui.InputText("密码:", ref TradeLoginInfo.Instance.password, 100);
        ImGui.InputText("验证码:", ref m_inputText, 100);
        ImGui.SameLine();
        if (m_textureID > 0)
        {
            ImGui.Image((IntPtr)m_textureID, new System.Numerics.Vector2(100, 100));
        }
        if(ImGui.Button("登录"))
        {
            LifecycleManager.Instance.Get<EventManager>().RaiseEvent(EventType.LoginButtonClicked,new object[] { m_inputText });
        }
    }

    public void SetValidCodePNGFilePath(string path)
    {
        if (m_textureID > 0)
        {
            Resources.FreeTexture(m_textureID);
        }
        m_textureID = Resources.LoadTexture(path);
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        if (m_textureID > 0)
        {
            Resources.FreeTexture(m_textureID);
        }
    }
}
