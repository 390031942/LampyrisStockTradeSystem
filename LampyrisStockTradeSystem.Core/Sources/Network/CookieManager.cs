/*
** Author: wushuhong
** Contact: gameta@qq.com
** Description: Cookie管理类，支持将Cookie序列化保存在本地，或加载本地Cookie并应用在HttpClient上
*/
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Http;

namespace LampyrisStockTradeSystem;

public enum CookieType
{
    EastMoneyTrade = 0,
    Count = 1,
}

public static class CookieUtil
{
    // 将Selenium的Cookie设置到HttpClientHandler里，供HttpClient使用
    public static HttpClientHandler UseHttpClientWithSeleniumCookies(ReadOnlyCollection<OpenQA.Selenium.Cookie> seleniumCookies)
    {
        var handler = new HttpClientHandler();

        var cookieContainer = new CookieContainer();
        handler.CookieContainer = cookieContainer;

        // 转换Cookies并添加到CookieContainer中
        foreach (var seleniumCookie in seleniumCookies)
        {
            System.Net.Cookie cookie = new System.Net.Cookie
            {
                Name = seleniumCookie.Name,
                Value = seleniumCookie.Value,
                Domain = seleniumCookie.Domain,
                Path = seleniumCookie.Path,
                Expires = seleniumCookie.Expiry.GetValueOrDefault(DateTime.Now.AddYears(1)), // 设置一个默认的过期时间
                Secure = seleniumCookie.Secure,
                HttpOnly = seleniumCookie.IsHttpOnly
            };
            cookieContainer.Add(cookie);
        }

        return handler;
    }

    // 将Selenium的Cookie设置到HttpClientHandler里，供HttpClient使用
    public static HttpClientHandler UseHttpClientWithCookies(List<Cookie> cookies)
    {
        var handler = new HttpClientHandler();

        var cookieContainer = new CookieContainer();
        handler.CookieContainer = cookieContainer;

        // 转换Cookies并添加到CookieContainer中
        foreach (var cookie in cookies)
        {
            cookieContainer.Add(cookie);
        }

        return handler;
    }
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

    public bool HasValidCookie(CookieType cookieType)
    {
        return m_type2CookieCollectionMap[cookieType].expires < DateTime.Now;
    }

    public HttpClient GetHttpClientWithCookieType(CookieType cookieType)
    {
        var cookies = m_type2CookieCollectionMap[cookieType].cookies;
        return new HttpClient(CookieUtil.UseHttpClientWithCookies(cookies));
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
