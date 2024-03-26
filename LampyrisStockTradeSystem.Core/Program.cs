/*
** Author: wushuhong
** Contact: gameta@qq.com
** Description: LampyrisStockTradeSystem入口类
*/
namespace LampyrisStockTradeSystem;

using System;
using System.Collections.Generic;

class Program
{
    static void Main()
    {
        LifecycleManager.Instance.StartUp();

        // 初始化系统托盘程序
        SystemTrayIcon.Instance.Create();

        RuntimeContext.mainWindow = new ProgramWindow();
        RuntimeContext.mainWindow.Run();

        LifecycleManager.Instance.ShutDown();

    }
}