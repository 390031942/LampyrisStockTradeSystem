/*
** Author: wushuhong
** Contact: gameta@qq.com
** Description: OpenGL异常检查
*/

namespace LampyrisStockTradeSystem;

using OpenTK.Graphics.OpenGL4;

public class OpenGLErrorCheckScope : IDisposable
{
    public void Dispose()
    {
        ErrorCode errorCode = GL.GetError();
        if (errorCode != ErrorCode.NoError)
        {
            // TODO:LogError
        }
    }
}

public static class Error
{
    public static void Check()
    {
        ErrorCode errorCode = GL.GetError();
        if (errorCode != ErrorCode.NoError)
        {
            throw new InvalidOperationException();
        }
    }
}