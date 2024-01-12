/*
** Author: wushuhong
** Contact: gameta@qq.com
** Description: 序列化工具类，负责将App设置，股票分析数据等保存到本地磁盘
*/

using System.Runtime.Serialization.Formatters.Binary;

namespace LampyrisStockTradeSystem;

public class SerializationManager : Singleton<SerializationManager>, ILifecycle
{

    public void OnDestroy()
    {

    }

    public void OnStart()
    {

    }

    // Unused
    public void OnUpdate(float dTime)
    {

    }

    public void Register(object serializedObject)
    {
        if (serializedObject != null)
        {
            string filePath = Path.Combine(PathUtil.SerializedDataSavePath, serializedObject.GetType().Name + ".bin");
            using (Stream stream = File.Open(filePath, FileMode.Create))
            {
                BinaryFormatter bin = new BinaryFormatter();
                bin.Serialize(stream, serializedObject);
            }
        }
    }

    public T TryDeserializeObjectFromFile<T>()
    {
        string filePath = Path.Combine(PathUtil.SerializedDataSavePath, typeof(T).Name + ".bin");

        using (Stream stream = File.Open(filePath, FileMode.Open))
        {
            BinaryFormatter bin = new BinaryFormatter();
            return (T)bin.Deserialize(stream);
        }

        return default(T);
    }
}
