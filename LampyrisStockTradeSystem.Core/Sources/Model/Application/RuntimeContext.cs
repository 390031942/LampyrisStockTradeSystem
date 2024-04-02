/*
** Author: wushuhong
** Contact: gameta@qq.com
** Description: 运行时上下文信息，如当前浏览的股票，显示的窗口状态等。
*/
namespace LampyrisStockTradeSystem;

public static class RuntimeContext
{
    // 正在浏览的股票代码
    public static string browsingStockCode = "";

    // 主窗口类对象
    public static ProgramWindow mainWindow;

    // 主窗口句柄
    public static IntPtr mainWindowPtr;
}
