/*
** Author: wushuhong
** Contact: gameta@qq.com
** Description: 港股通行情数据库
*/

using Newtonsoft.Json.Linq;

namespace LampyrisStockTradeSystem;
using LampyrisStockTradeSystemInternal;

// 港股通股票 股性肖像
[Serializable]
public class HKLinkStockPortrait
{
    // 近一年 最大涨幅
    public float maxPercentageRecentYear;

    // 近一年 涨幅总评分：
    // 计算公式：
    //
    //  ∞
    //  Σ i * i * Count(i),  其中Count(i)为近一年涨幅满足:  i * 10% <= 涨幅 < (i + 1) * 10% 的天数
    // i = 1
    // 比如某只股近一年涨幅在[10%,20%)的有3天，[20%,30%)的有1天，[30%,40%) 有2天
    // 则涨幅总评分: 为 1*1*3 + 2*2*1 + 3*3*2 = 3+4+18 = 25
    public float percentageScoreRecentYear;
}

public class HKLinkStockPortraitDataUpdateAsyncOperation : AsyncOperation
{
    private string m_dateTimeStartString;

    private string m_dateTimeEndString;

    private HttpRequestInternal m_httpRequestInternal = new HttpRequestInternal();

    private List<SerializableKeyValuePair<string, HKLinkStockPortrait>> m_dataList = new List<SerializableKeyValuePair<string, HKLinkStockPortrait>>();

    public HKLinkStockPortraitDataUpdateAsyncOperation(string dateTimeStartString, string dateTimeStartEnd)
    {
        m_dateTimeStartString = dateTimeStartString;
        m_dateTimeEndString = dateTimeStartEnd;

        ExecuteInternal();
    }

    public override object result
    {
        get
        {
            return m_dataList;
        }
    }

    public override void Execute()
    {
        m_progress = 0.0f;
        List<string> stockCodeList = HKLinkQuoteDatabase.Instance.GetAllStockCodeList();

        m_progress = 0.1f;
        for(int i = 0; i < stockCodeList.Count;i++)
        {
            string stockCode = stockCodeList[i];
            string url = StockQuoteInterface.Instance.GetQuoteUrl(StockQuoteInterfaceType.KLineData, UrlUtil.GetStockCodeParam(stockCode), m_dateTimeStartString, m_dateTimeEndString);
            m_httpRequestInternal.GetSync(url, (json) =>
            {
                HKLinkStockPortrait hKLinkStockPortrait = new HKLinkStockPortrait()
                {
                    maxPercentageRecentYear = -1000.0f,
                    percentageScoreRecentYear = 0
                };
                string strippedJson = JsonStripperUtil.GetEastMoneyStrippedJson(json);
                JObject jsonRoot = JObject.Parse(strippedJson);
                JArray klinesArray = jsonRoot?["data"]?["klines"]?.ToObject<JArray>();
                if (klinesArray != null)
                {
                    for (int i = 0; i < klinesArray.Count; i++)
                    {
                        JToken klineToken = klinesArray[i];
                        string klineJson = klineToken.ToString();
                        string[] strings = klineJson.Split(',');
                        if (strings.Length > 0)
                        {
                            float percentage = Convert.ToSingle(strings[8]);
                            if (hKLinkStockPortrait.maxPercentageRecentYear < percentage)
                            {
                                hKLinkStockPortrait.maxPercentageRecentYear = percentage;
                            }


                        }
                    }
                }

                m_dataList.Add(new SerializableKeyValuePair<string,HKLinkStockPortrait>(stockCode, hKLinkStockPortrait));
            });

            m_progress = 0.1f + 0.9f * (i / stockCodeList.Count);
        }

        m_progress = 1.0f;
    }
}

[Serializable]
public class HKLinkStockPortraitData:SerializableSingleton<HKLinkStockPortrait>
{
    private List<SerializableKeyValuePair<string, HKLinkStockPortrait>> m_dataList = new List<SerializableKeyValuePair<string, HKLinkStockPortrait>>();

    public Dictionary<string, HKLinkStockPortrait> m_dataDict = new Dictionary<string, HKLinkStockPortrait>();

    private DateTime m_recentEvaluateDate;

    public override void PostSerialization()
    {
        foreach(var kvp in m_dataList)
        {
            m_dataDict[kvp.Key] = kvp.Value;
        }
    }

    // 重新计算所有港股通的股性画像
    public AsyncOperation ReEvaluateIfNeed()
    {
        // 如果上一次画像计算的日期不是今天，那么就重新计算
        DateTime now = DateTime.Now;
        if (m_recentEvaluateDate.Year < now.Year || m_recentEvaluateDate.Month < now.Month || m_recentEvaluateDate.Day < now.Day)
        {
            // 计算的时间区间是【前一天减去365天，前一天】
            DateTime dateTimeStart = now.AddDays(-366);
            DateTime dateTimeEnd = now.AddDays(-1);

            string dateTimeStartString = dateTimeStart.ToString("yyyyMMdd");
            string dateTimeEndString = dateTimeEnd.ToString("yyyyMMdd");

            var operation =  new HKLinkStockPortraitDataUpdateAsyncOperation(dateTimeStartString, dateTimeEndString);
            operation.onCompletedCallback += () =>
            {
                m_dataList = (List<SerializableKeyValuePair<string, HKLinkStockPortrait>>)operation.result;
                m_recentEvaluateDate = now;
            };

            return operation;
        }

        return null;
    }
}

public class HKLinkQuoteDatabase:Singleton<HKLinkQuoteDatabase>
{
    public List<string> GetAllStockCodeList()
    {
        List<string> codeList = new List<string>();

        HttpRequest.GetSync(StockQuoteInterface.Instance.GetQuoteUrl(StockQuoteInterfaceType.HKLink), (string json) => {
            string strippedJson = JsonStripperUtil.GetEastMoneyStrippedJson(json);
            try
            {
                JObject jsonRoot = JObject.Parse(strippedJson);

                JArray stockDataArray = jsonRoot?["data"]?["diff"]?.ToObject<JArray>();
                if (stockDataArray != null)
                {
                    for (int i = 0; i < stockDataArray.Count; i++)
                    {
                        JObject stockObject = stockDataArray[i].ToObject<JObject>();

                        if (stockObject != null)
                        {
                            // 这里获取股票代码和名称
                            string code = stockObject["f12"]?.ToString();
                            codeList.Add(code);
                        }
                    }
                }
            }
            catch (Exception) { }
        });

        return codeList;
    }
}
