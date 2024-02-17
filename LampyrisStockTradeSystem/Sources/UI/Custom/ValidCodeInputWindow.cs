/*
** Author: wushuhong
** Contact: gameta@qq.com
** Description: 验证码输入窗口
*/
namespace LampyrisStockTradeSystem;

using ImGuiNET;
using System;

[UniqueWidget]
public class ValidCodeInputWindow : Widget
{
    private int m_textureID = -1;

    private string m_inputText = "";

    public override void OnGUI()
    { 
        if (m_textureID > 0)
        {
            ImGui.Image((IntPtr)m_textureID, new System.Numerics.Vector2(100, 100));
        }

        ImGui.InputText("Valid Code:", ref m_inputText, 100);
    }

    public void SetValidCodePNGFilePath(string path)
    {
        if (m_textureID > 0)
        {
            Resources.FreeTexture(m_textureID);
        }
        m_textureID = Resources.LoadTexture(path);
    }
}
