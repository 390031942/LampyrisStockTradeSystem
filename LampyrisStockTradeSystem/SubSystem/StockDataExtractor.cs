using System;
using System.Collections.Generic;
using System.Data;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace LampyrisStockTradeSystem;

public class StockListDataExtractor
{

}

public static class StockDataExtractor
{
    private static string ms_jquery = "jQuery1123008330414708828249_1669967900108";
    private static string ms_field1= "f1%2Cf2%2Cf3%2Cf4%2Cf5%2Cf6";
    private static string ms_field2= "f51%2Cf52%2Cf53%2Cf54%2Cf55%2Cf56%2Cf57%2Cf58%2Cf59%2Cf60%2Cf61";
    private static string ms_beg = "19900101";
    private static string ms_end = "20990101";
    private static string ms_klt = "101";
    private static string ms_fqt = "1";
    /// <summary>
    /// 东方财富通k线数据源
    /// </summary>
    private static string ms_url = "https://push2his.eastmoney.com/api/qt/stock/kline/get?cb={0}&fields1={1}&fields2={2}&klt={3}&fqt={4}&&secid={5}.{6}&beg={7}&end={8}";

    public static void RequestAllData()
    {
        var httpClient = new HttpClient();
        var response = httpClient.GetAsync(string.Format(ms_url, 
                                                         ms_jquery,
                                                         ms_field1, 
                                                         ms_field2,
                                                         ms_klt,
                                                         ms_fqt,
                                                         1,
                                                         600000,
                                                         ms_beg,
                                                         ms_end)).Result;

        var jsonString = response.Content.ReadAsStringAsync().Result;
        int jsonStringLength = jsonString.Length;
        int validStringLength = jsonStringLength - ms_jquery.Length - 3;
        jsonString = jsonString.Substring(ms_jquery.Length + 1,validStringLength);

        JObject jsonRoot = JObject.Parse(jsonString);
        JArray klines = jsonRoot["data"]["klines"].ToObject<JArray>();

        if (klines == null)
            return;

    }

    public static void RequestRealTimeQuotes()
    {
        string url = "https://push2.eastmoney.com/api/qt/clist/get?cb=jQuery1123008330414708828249_1669967900108&fid=f62&po=1&pz=50000&pn=1&np=1&fltt=2&invt=2&ut=b2884a393a59ad64002292a3e90d46a5&fs=m%3A0%2Bt%3A6%2Bf%3A!2%2Cm%3A0%2Bt%3A13%2Bf%3A!2%2Cm%3A0%2Bt%3A80%2Bf%3A!2%2Cm%3A1%2Bt%3A2%2Bf%3A!2%2Cm%3A1%2Bt%3A23%2Bf%3A!2%2Cm%3A0%2Bt%3A7%2Bf%3A!2%2Cm%3A1%2Bt%3A3%2Bf%3A!2&fields=f4%2Cf22%2Cf12%2Cf14%2Cf2%2Cf3%2Cf62%2Cf184%2Cf66%2Cf69%2Cf72%2Cf75%2Cf78%2Cf81%2Cf84%2Cf87%2Cf204%2Cf205%2Cf124%2Cf1%2Cf13%20%E2%80%94%E2%80%94%E2%80%94%E2%80%94%E2%80%94%E2%80%94%E2%80%94%E2%80%94%E2%80%94%E2%80%94%E2%80%94%E2%80%94%E2%80%94%E2%80%94%E2%80%94%E2%80%94%";

        var httpClient = new HttpClient();
        var response = httpClient.GetAsync(url).Result;

        var jsonString = response.Content.ReadAsStringAsync().Result;
        int jsonStringLength = jsonString.Length;
        int validStringLength = jsonStringLength - ms_jquery.Length - 3;
        jsonString = jsonString.Substring(ms_jquery.Length + 1, validStringLength);

        JObject jsonRoot = JObject.Parse(jsonString);
        JArray stockDataArray = jsonRoot["data"]["diff"].ToObject<JArray>();

        ((StockQuoteTableWindow)WidgetManagement.GetWidget<StockQuoteTableWindow>()).SetStockData(stockDataArray);
    }
}