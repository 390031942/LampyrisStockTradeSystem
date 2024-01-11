/*
** Author: wushuhong
** Contact: gameta@qq.com
** Description: 日期实用工具类
*/

namespace LampyrisStockTradeSystem;

public static class DateUtil
{
    public static string GetCurrentDateString()
    {
        DateTime now = DateTime.Now;
        string dateString = now.ToString("yyyyMMdd");

        return dateString;
    }

    public static bool ParseDateString(string dateString, out DateTime validTime)
    {
        return DateTime.TryParseExact(dateString, "HH:mm", null, System.Globalization.DateTimeStyles.None, out validTime);
    }
}
