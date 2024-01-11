/*
** Author: wushuhong
** Contact: gameta@qq.com
** Description: 从东财数据接口请求并得到股票实时/历史行情，以及股票的基本面等信息
*  TODO:请求改造成多线程+异步回调模式，避免主线程卡死
*/
namespace LampyrisStockTradeSystem;

using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using System.Web;

public enum StockQuoteInterfaceType
{ 
    KLineData = 1, // 个股-K线数据
    IntradayTradingDetail = 2, // 个股-分时成交明细
    CurrentQuotes = 3, // 全体-实时行情列表
}

public abstract class StockDataInterface
{
    protected abstract string url { get; }

    protected abstract Dictionary<string, string> parameters { get; }

    public abstract StockQuoteInterfaceType quetoType { get; }

    public static string jQueryString = "jQuery1123008330414708828249_1669967900108";

    private bool TryParseSpecificParam(string rawValue, string[] specificParams, out string result)
    {
        result = "";

        Regex regex = new Regex(@"^%(\d+)%$");
        Match match = regex.Match(rawValue);

        if (match.Success)
        {
            if(int.TryParse(match.Groups[1].Value,out var number))
            {
                if(number >= 0 && number < specificParams.Length)
                {
                    result = specificParams[number];
                }
                return true;
            }

            return false;
        }
        else
        {
            return false;
        }
    }

    public string MakeUrl(params string[] specificParams)
    {
        var builder = new UriBuilder(this.url);
        var query = HttpUtility.ParseQueryString(builder.Query);

        foreach(KeyValuePair<string,string> kvp in parameters)
        {
            query[kvp.Key] = kvp.Value;
        }

        builder.Query = query.ToString();
        return builder.ToString();
    }
}

public class StockKLineDataExtractor : StockDataInterface
{
    public override StockQuoteInterfaceType quetoType => StockQuoteInterfaceType.KLineData;

    protected override string url => "https://push2his.eastmoney.com/api/qt/stock/kline/get";

    protected override Dictionary<string, string> parameters => new Dictionary<string, string>()
    {
       {"cb",jQueryString},
       {"fields1","f1%2Cf2%2Cf3%2Cf4%2Cf5%2Cf6" },
       {"fields2","f51%2Cf52%2Cf53%2Cf54%2Cf55%2Cf56%2Cf57%2Cf58%2Cf59%2Cf60%2Cf61" },
       {"klt","101" },
       {"fqt","1" },
       {"secid","%0%" }, // %0%表示占位符，表示MakeUrl函数参数specificParams的第0个元素，下同
       {"beg","19900101" },
       {"end","20990101" }
    };
}


public static class StockDataExtractor
{
    // 股票行情类型 -> 实现类
    private static Dictionary<StockQuoteInterfaceType, StockDataInterface> ms_stockQuoteType2ImplDict = new()
    {
        { StockQuoteInterfaceType.KLineData,new StockKLineDataExtractor() },
    };

    public static void Request(StockQuoteInterfaceType type)
    {
        StockDataInterface stockDataInterface = ms_stockQuoteType2ImplDict[type];

        var httpClient = new HttpClient();
        var response = httpClient.GetAsync(stockDataInterface.MakeUrl("1.600000")).Result;

        var jsonString = response.Content.ReadAsStringAsync().Result;
        int jsonStringLength = jsonString.Length;
        int validStringLength = jsonStringLength - StockDataInterface.jQueryString.Length - 3;
        jsonString = jsonString.Substring(StockDataInterface.jQueryString.Length + 1,validStringLength);

        JObject jsonRoot = JObject.Parse(jsonString);
        JArray klines = jsonRoot["data"]["klines"].ToObject<JArray>();

        if (klines == null)
            return;

    }

    /// <summary>
    /// 测试函数，不合规待删除
    /// </summary>
    public static void RequestRealTimeQuotes()
    {
        string url = "https://push2.eastmoney.com/api/qt/clist/get?cb=jQuery1123008330414708828249_1669967900108&fid=f62&po=1&pz=50000&pn=1&np=1&fltt=2&invt=2&ut=b2884a393a59ad64002292a3e90d46a5&fs=m%3A0%2Bt%3A6%2Bf%3A!2%2Cm%3A0%2Bt%3A13%2Bf%3A!2%2Cm%3A0%2Bt%3A80%2Bf%3A!2%2Cm%3A1%2Bt%3A2%2Bf%3A!2%2Cm%3A1%2Bt%3A23%2Bf%3A!2%2Cm%3A0%2Bt%3A7%2Bf%3A!2%2Cm%3A1%2Bt%3A3%2Bf%3A!2&fields=f4%2Cf22%2Cf12%2Cf14%2Cf2%2Cf3%2Cf62%2Cf184%2Cf66%2Cf69%2Cf72%2Cf75%2Cf78%2Cf81%2Cf84%2Cf87%2Cf204%2Cf205%2Cf124%2Cf1%2Cf13%20%E2%80%94%E2%80%94%E2%80%94%E2%80%94%E2%80%94%E2%80%94%E2%80%94%E2%80%94%E2%80%94%E2%80%94%E2%80%94%E2%80%94%E2%80%94%E2%80%94%E2%80%94%E2%80%94%";

        var httpClient = new HttpClient();
        var response = httpClient.GetAsync(url).Result;

        var jsonString = response.Content.ReadAsStringAsync().Result;
        int jsonStringLength = jsonString.Length;
        int validStringLength = jsonStringLength - StockDataInterface.jQueryString.Length - 3;
        jsonString = jsonString.Substring(StockDataInterface.jQueryString.Length + 1, validStringLength);

        JObject jsonRoot = JObject.Parse(jsonString);
        JArray stockDataArray = jsonRoot["data"]["diff"].ToObject<JArray>();

        ((StockQuoteTableWindow)WidgetManagement.GetWidget<StockQuoteTableWindow>()).SetStockData(stockDataArray);
    }
}