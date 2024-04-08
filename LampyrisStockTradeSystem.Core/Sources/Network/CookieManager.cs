/*
** Author: wushuhong
** Contact: gameta@qq.com
** Description: Cookie管理类，支持将Cookie序列化保存在本地，或加载本地Cookie并应用在HttpClient上
*/
using System.Linq;
using System.Net;

namespace LampyrisStockTradeSystem;

public enum CookieType
{
    EastMoneyTrade = 0,
    Count = 1,
}

[Serializable]
public class CookieCollection
{
    // Cookie类型
    public CookieType cookieType;

    // 过期时间
    public DateTime expires;

    // Cookie列表
    public List<Cookie> cookies = new List<Cookie>();
}

[Serializable]
public class CookieManager:SerializableSingleton<CookieManager>,IPostSerializationHandler
{
    public List<CookieCollection> m_cookieCollections = new List<CookieCollection>();

    private Dictionary<CookieType, CookieCollection> m_type2CookieCollectionMap = new Dictionary<CookieType, CookieCollection>();

    public void RecordCookieCollection(CookieCollection cookieCollection)
    {
        if (cookieCollection == null)
            return;

        if(!m_type2CookieCollectionMap.ContainsKey(cookieCollection.cookieType))
        {
            m_type2CookieCollectionMap[cookieCollection.cookieType] = cookieCollection;
            m_cookieCollections.Add(cookieCollection);
        }
        else
        {
            var old = m_type2CookieCollectionMap[cookieCollection.cookieType];
            if (old.expires <cookieCollection.expires)
            {
                m_cookieCollections.Remove(old);
                m_cookieCollections.Add(cookieCollection);
                m_type2CookieCollectionMap[cookieCollection.cookieType] = cookieCollection;
            }
        }
    }

    public override void PostSerialization()
    {
        List<CookieCollection> cookieCollectionsTemp = new List<CookieCollection>();
        cookieCollectionsTemp.AddRange(m_cookieCollections);
        foreach(CookieCollection cookieCollection in cookieCollectionsTemp)
        {
            if(DateTime.Now > cookieCollection.expires)
            {
                m_cookieCollections.Remove(cookieCollection);
            }
        }
    }
}
