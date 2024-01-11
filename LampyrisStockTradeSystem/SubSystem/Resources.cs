using ImGuiNET;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PixelFormat = OpenTK.Graphics.OpenGL4.PixelFormat;

namespace LampyrisStockTradeSystem;

public static class Resources
{
    private static Dictionary<string,int>  ms_resPath2resIdDict = new Dictionary<string,int>();
    private static Dictionary<int, string> ms_resId2resPathDict = new Dictionary<int, string>();

    public static int LoadTexture(string path)
    {
        int textureID;

        // 存在则直接返回
        if(ms_resPath2resIdDict.TryGetValue(path, out textureID))
        {
            return textureID;
        }

        try
        {
            Bitmap bitmap = new Bitmap(path);
            BitmapData data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            // GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);

            GL.GenTextures(1, out textureID);
            GL.BindTexture(TextureTarget.Texture2D, textureID);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
            bitmap.UnlockBits(data);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        }
        catch (Exception ex)
        {
            Console.Write(ex.ToString());
            return -1;
        }

        ms_resPath2resIdDict[path] = textureID;
        ms_resId2resPathDict[textureID] = path;

        return textureID;
    }

    public static void FreeTexture(int textureID)
    {
        if(ms_resId2resPathDict.ContainsKey(textureID))
        {
            GL.DeleteTextures(1, ref textureID);

            string path = ms_resId2resPathDict[textureID];
            ms_resPath2resIdDict.Remove(path);
            ms_resId2resPathDict.Remove(textureID);
        }
    }

    public unsafe static ImFontPtr LoadFont()
    {
        // 首先，你需要创建一个字体集
        var fontAtlas = ImGui.GetIO().Fonts;

        // 然后，你可以使用AddFontFromFileTTF方法来加载字体文件
        // 注意，你需要提供字体文件的完整路径，这里假设字体文件在你的项目根目录下
        string fontPath = Path.Combine("C:\\Windows\\Fonts", "simsun.ttc");

        // 加载字体并设置大小
        var font = fontAtlas.AddFontFromFileTTF(fontPath, 10);

        // 如果字体加载失败，AddFontFromFileTTF方法会返回null
        if (font.NativePtr == null)
        {
            throw new Exception($"Failed to load font at '{fontPath}'");
        }

        // 最后，你需要构建字体集
        fontAtlas.Build();

        return font;
    }
}
