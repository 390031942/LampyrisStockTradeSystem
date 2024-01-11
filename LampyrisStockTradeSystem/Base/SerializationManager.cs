/*
** Author: wushuhong
** Contact: gameta@qq.com
** Description: 序列化工具类，负责将App设置，股票分析数据等保存到本地磁盘
*/

namespace LampyrisStockTradeSystem;

public class SerializationManager : Singleton<SerializationManager>, ILifecycle
{
    public void OnDestroy()
    {
    }

    public void OnStart()
    {
    }

    public void OnUpdate(float dTime)
    {
    }

    public void Register<T>(T ms_instance) where T : class, new()
    {
        
    }
}
