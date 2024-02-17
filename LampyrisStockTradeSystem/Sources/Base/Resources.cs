/*
** Author: wushuhong
** Contact: gameta@qq.com
** Description: 资源管理系统，负责纹理资源，字体资源的卸载
*  TODO: 实现资源引用计数；提供卸载未使用资源的方法
*/
namespace LampyrisStockTradeSystem;

using ImGuiNET;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using PixelFormat = OpenTK.Graphics.OpenGL4.PixelFormat;

public static class Resources
{
    private static Dictionary<string,int>  ms_resPath2resIdDict = new Dictionary<string,int>();
    private static Dictionary<int, string> ms_resId2resPathDict = new Dictionary<int, string>();

    /// <summary>
    /// 加载一个纹理资源
    /// </summary>
    /// <param name="path">纹理所在的路径</param>
    /// <returns>OpenGL纹理ID</returns>
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

    /// <summary>
    /// 释放一个纹理资源
    /// </summary>
    /// <param name="path">OpenGL纹理ID<</param>
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

   /// <summary>
   /// 动态加载字体
   /// </summary>
   /// <returns></returns>
    public unsafe static ImFontPtr LoadFont()
    {
        // 创建一个字体集
        var fontAtlas = ImGui.GetIO().Fonts;

        // 加载字体文件,并设置大小
        string fontPath = Path.Combine("C:\\Windows\\Fonts", "simsun.ttc");

        // 加载字体并设置大小
        var font = fontAtlas.AddFontFromFileTTF(fontPath, 10);

        if (font.NativePtr == null)
        {
            // TODO: 日志打印报错信息
            // throw new Exception($"Failed to load font at '{fontPath}'");
        }

        // 构建字体集
        fontAtlas.Build();

        return font;
    }
}
