/*
** Author: wushuhong
** Contact: gameta@qq.com
** Description: 可序列化的单例类，比如App设置等
*/

namespace LampyrisStockTradeSystem;

using System.Runtime.Serialization.Formatters.Binary;

[Serializable]
public class SerializableSingleton<T> where T : class, new()
{
    private static readonly string filePath = "singletonData.bin";
    private static T ms_instance;

    public static T Instance
    {
        get
        {
            if (ms_instance == null)
            {
                if (File.Exists(filePath))
                {
                    try
                    {
                        using (Stream stream = File.Open(filePath, FileMode.Open))
                        {
                            BinaryFormatter bin = new BinaryFormatter();
                            ms_instance = (T)bin.Deserialize(stream);
                            SerializationManager.Instance.Register(ms_instance);
                        }
                    }
                    catch
                    {
                        ms_instance = new T();
                    }
                }
                else
                {
                    ms_instance = new T();
                }
            }
            return ms_instance;
        }
    }

    public static void Save()
    {
        if (ms_instance != null)
        {
            using (Stream stream = File.Open(filePath, FileMode.Create))
            {
                BinaryFormatter bin = new BinaryFormatter();
                bin.Serialize(stream, ms_instance);
            }
        }
    }
}