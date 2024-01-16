using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace LampyrisStockTradeSystem;

public enum StockQuoteInterfaceType
{
    KLineData = 1, // 个股-K线数据
    IntradayTradingDetail = 2, // 个股-分时成交明细
    CurrentQuotes = 3, // 全体-实时行情列表
}

public abstract class IStockQuoteInterface
{
    protected abstract string url { get; }

    protected abstract Dictionary<string, string> parameters { get; }

    public abstract StockQuoteInterfaceType quetoType { get; }

    private bool TryParseSpecificParam(string rawValue, string[] specificParams, out string result)
    {
        result = "";
        string pattern = @"^%(\d+)%(.*$)";
        Regex regex = new Regex(pattern);
        Match match = regex.Match(rawValue);

        if (match.Success)
        {
            result = match.Groups[2].Value;
            if (int.TryParse(match.Groups[1].Value, out var number))
            {
                if (number >= 0 && number < specificParams.Length)
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
        string resultUrl = url;

        if(parameters != null && parameters.Count > 0)
        {
            int count = parameters.Count;
            int index = 0;

            resultUrl = resultUrl + "?";

            foreach (KeyValuePair<string, string> kvp in parameters)
            {
                if (TryParseSpecificParam(kvp.Value, specificParams, out var result))
                    resultUrl = resultUrl + kvp.Key + "=" + result;
                else
                    resultUrl = resultUrl + kvp.Key + "=" + kvp.Value;

                if(index < count - 1)
                {
                    resultUrl = resultUrl + "&";
                }
                index++;
            }
        }

        return resultUrl;
    }
}