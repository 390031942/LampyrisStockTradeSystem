using Newtonsoft.Json.Linq;

namespace LampyrisStockTradeSystem;

public static class JsonUtil
{
    public static T SafeToObject<T>(this JToken token,T defaultValue = default)
    {
        if (token == null)
            return defaultValue;

        try
        {
            return token.ToObject<T>();
        }
        catch 
        {
            return defaultValue;
        }
    }
}
