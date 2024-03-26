/*
** Author: wushuhong
** Contact: gameta@qq.com
** Description: 可序列化的键值对，方便对Dictioanry进行序列化
*/

namespace LampyrisStockTradeSystem;

[Serializable]
public class SerializableKeyValuePair<TKey, TValue>
{
    private TKey m_key;
    private TValue m_value;

    public TKey Key { get; }
    public TValue Value { get; }

    public SerializableKeyValuePair(TKey key, TValue value)
    {
        m_key = key;
        m_value = value;
    }
}
