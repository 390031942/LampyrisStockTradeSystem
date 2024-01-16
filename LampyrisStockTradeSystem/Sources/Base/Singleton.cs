/*
** Author: wushuhong
** Contact: gameta@qq.com
** Description: 单例类基类
*/

namespace LampyrisStockTradeSystem;

public class Singleton<T> where T : class, new()
{
    private static T ms_instance;

    public static T Instance
    {
        get
        {
            if(ms_instance == null)
            {
                ms_instance = new T();
            }
            return ms_instance;
        }
    }
}
