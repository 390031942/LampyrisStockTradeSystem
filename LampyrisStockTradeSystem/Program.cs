/*
** Author: wushuhong
** Contact: gameta@qq.com
** Description: LampyrisStockTradeSystem入口类
*/
namespace LampyrisStockTradeSystem;

class Program
{
    static void Main()
    {
        // 初始化生命周期管理
        LifecycleManager.Instance.StartUp();

        // 初始化系统托盘程序
        SystemTrayIcon.Instance.Create();

        ProgramWindow wnd = new ProgramWindow();
        wnd.Run();

        LifecycleManager.Instance.ShutDown();
    }
}