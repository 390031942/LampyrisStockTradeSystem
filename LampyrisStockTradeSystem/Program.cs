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

        // BrowserSystem.Instance.OnStart();
        // {
        //     BrowserSystem.Instance.Request("https://jywg.18.cn/Trade/Buy");
        //     BrowserSystem.Instance.Input(By.Id("txtZjzh"), "541220062779");
        //     BrowserSystem.Instance.Input(By.Id("txtPwd"), "092712");
        //     BrowserSystem.Instance.SaveImg(By.Id("imgValidCode"), "D:\\imgValidCode.png", false);
        // }
        // BrowserSystem.Instance.OnDestroy();

        ProgramWindow wnd = new ProgramWindow();
        wnd.Run();

        LifecycleManager.Instance.ShutDown();
    }
}