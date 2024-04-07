/*
** Author: wushuhong
** Contact: gameta@qq.com
** Description: 类型转换实用工具类
*/
using System.Globalization;

namespace LampyrisStockTradeSystem;

public static class ConvertUtil
{
    public static T SafeParse<T>(string s) where T : IConvertible
    {
        Type targetType = typeof(T);
        if (!typeof(IConvertible).IsAssignableFrom(targetType))
        {
            return default(T);
        }
        try
        {
            if (targetType == typeof(int))
            {
                return (T)Convert.ChangeType(int.Parse(s, CultureInfo.InvariantCulture), typeof(T));
            }
            else if (targetType == typeof(float))
            {
                return (T)Convert.ChangeType(float.Parse(s, CultureInfo.InvariantCulture), typeof(T));
            }
            else if (targetType == typeof(double))
            {
                return (T)Convert.ChangeType(double.Parse(s, CultureInfo.InvariantCulture), typeof(T));
            }
            else
            {
                return default(T);
            }
        }
        catch (Exception ex)
        {
            return default(T);
        }
    }
}
