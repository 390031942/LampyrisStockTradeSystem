/*
** Author: wushuhong
** Contact: gameta@qq.com
** Description: LampyrisStockTradeSystem入口类
*/
namespace LampyrisStockTradeSystem;

using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Runtime.InteropServices;
using static LampyrisStockTradeSystem.IconFlashTips;

class Program
{
    [DllImport("user32.dll", SetLastError = true)]
    static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    static unsafe void Main()
    {
        try
        {
            LifecycleManager.Instance.StartUp();

            // 初始化系统托盘程序
            SystemTrayIcon.Instance.Create();

            RuntimeContext.mainWindow = new ProgramWindow();
            RuntimeContext.mainWindowPtr = FindWindow("GLFW30", "OpenTK Window");
            RuntimeContext.mainWindow.Run();
        }
        catch (Exception ex)
        {
            System.Windows.Forms.MessageBox.Show(ex.ToString(), "Crash!!!");
        }
        finally
        {
            LifecycleManager.Instance.ShutDown();
        }
    }

    [MenuItem("调试/闪烁窗口")]
    public static void Flash()
    {
        CallTimer.Instance.SetInterval(() => {
            IconFlashTips.Flash(RuntimeContext.mainWindowPtr, IconFlashTips.FLASHW_TIMER);
        }, 3000, 1);
    }
}