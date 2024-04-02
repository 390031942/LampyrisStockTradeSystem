/*
** Author: wushuhong
** Contact: gameta@qq.com
** Description: 任务栏图标闪烁提醒实现
*/
using System.Runtime.InteropServices;

namespace LampyrisStockTradeSystem;

public static class IconFlashTips
{
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool FlashWindowEx(ref FLASHWINFO pwfi);

    [StructLayout(LayoutKind.Sequential)]
    public struct FLASHWINFO
    {
        public uint cbSize;
        public IntPtr hwnd;
        public uint dwFlags;
        public uint uCount;
        public uint dwTimeout;
    }

    public const uint FLASHW_STOP = 0;
    public const uint FLASHW_CAPTION = 1;
    public const uint FLASHW_TRAY = 2;
    public const uint FLASHW_ALL = 3;
    public const uint FLASHW_TIMER = 4;
    public const uint FLASHW_TIMERNOFG = 12;

    public static bool Flash(IntPtr hWnd, uint type)
    {
        FLASHWINFO fInfo = new FLASHWINFO();
        fInfo.cbSize = Convert.ToUInt32(Marshal.SizeOf(fInfo));
        fInfo.hwnd = hWnd;//要闪烁的窗口的句柄，该窗口可以是打开的或最小化的
        fInfo.dwFlags = (uint)type;//闪烁的类型
        fInfo.uCount = UInt32.MaxValue;//闪烁窗口的次数
        fInfo.dwTimeout = 0; //窗口闪烁的频度，毫秒为单位；若该值为0，则为默认图标的闪烁频度
        return FlashWindowEx(ref fInfo);
    }
}
