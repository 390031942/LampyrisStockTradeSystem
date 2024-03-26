/*
** Author: wushuhong
** Contact: gameta@qq.com
** Description: 序列化工具类，负责将App设置，股票分析数据等保存到本地磁盘
*/

using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace LampyrisStockTradeSystem;

public class SerializationManager: ILifecycle
{
    private List<object> m_serializableObjectList = new List<object>();

    public void OnDestroy()
    {
        BinaryFormatter bin = new BinaryFormatter();
        foreach (object serializableObject in m_serializableObjectList)
        {
            string filePath = Path.Combine(PathUtil.SerializedDataSavePath, serializableObject.GetType().Name + ".bin");
            using (Stream stream = File.Open(filePath, FileMode.OpenOrCreate))
            {
                bin.Serialize(stream, serializableObject);
            }
        }
    }

    public void OnStart()
    {

    }

    // Unused
    public void OnUpdate(float dTime)
    {

    }

    public void Register(object serializableObject)
    {
        if (serializableObject != null)
        {
            m_serializableObjectList.Add(serializableObject);
        }
    }

    public T TryDeserializeObjectFromFile<T>()
    {
        string filePath = Path.Combine(PathUtil.SerializedDataSavePath, typeof(T).Name + ".bin");

        if(File.Exists(filePath))
        {
            using (Stream stream = File.Open(filePath, FileMode.Open))
            {
                try
                {
                    BinaryFormatter bin = new BinaryFormatter();
                    return (T)bin.Deserialize(stream);
                }
                catch(SerializationException ex)
                {
                    return default(T);
                }
            }
        }

        return default(T);
    }
}
