/*
** Author: wushuhong
** Contact: gameta@qq.com
** Description: 可序列化的单例类，比如App设置等
*/

namespace LampyrisStockTradeSystem;

using System.Runtime.Serialization.Formatters.Binary;

public class SerializableSingletonBase
{
    public void Save()
    {

    }
}

[Serializable]
public class SerializableSingleton<T> : SerializableSingletonBase where T : class, new()
{
    private static T ms_instance;

    public static T Instance
    {
        get
        {
            if (ms_instance == null)
            {
                ms_instance = SerializationManager.Instance.TryDeserializeObjectFromFile<T>();
                if(ms_instance == null)
                {
                    ms_instance = new T();
                }
            }
            return ms_instance;
        }
    }
}