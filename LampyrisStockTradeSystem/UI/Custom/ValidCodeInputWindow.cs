namespace LampyrisStockTradeSystem;

using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[UniqueWidget]
public class ValidCodeInputWindow : Widget
{
    private int m_textureID = -1;

    private string m_inputText = "";

    public override void OnGUI()
    {
        // 然后，你可以使用Image函数来显示一个图片。
        // 注意，你需要先把图片加载为一个Texture，然后传递给Image函数。
        // 这里假设你已经有了一个名为myTexture的Texture对象。
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
