/*
** Author: wushuhong
** Contact: gameta@qq.com
** Description: LampyrisStockTradeSystem入口类
*/
namespace LampyrisStockTradeSystem;

using System;

class Program
{
    static void Main()
    {
        try
        {
            LifecycleManager.Instance.StartUp();

            // 初始化系统托盘程序
            SystemTrayIcon.Instance.Create();

            RuntimeContext.mainWindow = new ProgramWindow();
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
}