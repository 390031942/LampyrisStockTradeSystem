/*
** Author: wushuhong
** Contact: gameta@qq.com
** Description: 系统托盘图标，加上Windows右下角弹窗的实现
*/
namespace LampyrisStockTradeSystem;

using System.Windows.Forms;

public class SystemTrayIcon : Singleton<SystemTrayIcon>
{
    private NotifyIcon? m_notifyIcon = null;

    public void Create()
    {
        if (m_notifyIcon == null)
        {
            m_notifyIcon = new NotifyIcon();
            m_notifyIcon.Icon = SystemIcons.Application;
            m_notifyIcon.Visible = true;
        }
    }

    public void ShowMessage(string message, ToolTipIcon icon = ToolTipIcon.Info)
    {
        m_notifyIcon?.ShowBalloonTip(3000, "Lampyris股票行情交易系统", "", icon);
    }
}
