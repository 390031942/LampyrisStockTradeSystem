/*
** Author: wushuhong
** Contact: gameta@qq.com
** Description: 股票数据的定义
*/

using Newtonsoft.Json.Linq;
using System.Security.Cryptography.X509Certificates;

namespace LampyrisStockTradeSystem;

/* TODO:股票指标数据接口 */
public class IStockKLineIndicator
{

}

/* 股票MA指标 */
[Serializable]
public struct MAIndicator
{
    public float MA5;
    public float MA10;
    public float MA20;
    public float MA30;
    public float MA60;
    public float MA120;
    public float MA250;
}

/* 股票K线数据，这里不一定指的是日K，只是泛指一根K线的数据 */
[Serializable]
public class StockKLineData
{
    /// <summary>
    /// 日期
    /// </summary>
    public DateTime date;

    /// <summary>
    /// 开盘价
    /// </summary>
    public float openPrice;

    /// <summary>
    /// 收盘价
    /// </summary>
    public float closePrice;

    /// <summary>
    /// 最高价
    /// </summary>
    public float highestPrice;

    /// <summary>
    /// 最低价
    /// </summary>
    public float lowestPrice;

    /// <summary>
    /// 成交量 
    /// </summary>
    public float volume;

    /// <summary>
    /// 成交额
    /// </summary>
    public float money;

    /// <summary>
    /// 振幅
    /// </summary>
    public float amplitude;

    /// <summary>
    /// 涨跌幅
    /// </summary>
    public float percentage;

    /// <summary>
    /// 涨跌额
    /// </summary>
    public float priceChange;

    /// <summary>
    /// 换手率
    /// </summary>
    public float turnOverRate;

    /// <summary>
    /// 在所有K线数据数据中的索引，方便找到前驱和后继
    /// </summary>
    public int index;

    /// <summary>
    /// 均值数据
    /// </summary>
    public MAIndicator maData;
}


/// <summary>
/// 股票数据 = 股票日K数据 + 基本面，TODO：以后会加入周线，分钟线等不同行情周期的数据
/// </summary>
[Serializable]
public class StockData
{
    /// <summary>
    /// 股票代码
    /// </summary>
    public string code;

    /// <summary>
    /// 股票名称
    /// </summary>
    public string name;

    /// <summary>
    /// 所属行业板块编号
    /// </summary>
    public string sectorCode;

    /// <summary>
    /// 所属行业板块名称
    /// </summary>
    public string sectorName;

    /// <summary>
    /// 每股收益
    /// </summary>
    public float EPS;

    /// <summary>
    /// 市盈率
    /// </summary>
    public float PERatio
    {
        get
        {
            if(perDayKLineList.Count > 0 && EPS != 0.0f)
            {
                StockKLineData stockKLineData = perDayKLineList.Last();
                return stockKLineData.closePrice/ EPS;
            }
            else
            {
                return 0.0f;
            }
        }
    }

    /// <summary>
    /// 日K线数据
    /// </summary>
    public List<StockKLineData> perDayKLineList = new List<StockKLineData>();

    /// <summary>
    /// 总股本
    /// </summary>
    public float totalShares;

    /// <summary>
    /// 流通股
    /// </summary>
    public float circulatingShares;

    /// <summary>
    /// 总市值(Total Market Capitalization,TMC)
    /// </summary>
    public float TMC;

    /// <summary>
    /// 流通市值(Circulating Market Capitalization,CMC)
    /// </summary>
    public float CMC;
}

/// <summary>
/// 实时行情，相当于当时的日K数据 + 股票代码和名称 
/// </summary>
public class StockRealTimeQuoteData:StockKLineData
{
    /// <summary>
    /// 股票代码
    /// </summary>
    public string code;

    /// <summary>
    /// 股票名称
    /// </summary>
    public string name;
}

// 股票行情数据库，TODO：需要序列化保存 以便于实现 差异化请求数据
[Serializable]
class StockDatabase:SerializableSingleton<StockDatabase>,IPostSerializationHandler
{
    /// <summary>
    /// 股票代码->股票数据字典
    /// </summary>
    private Dictionary<string, StockData> m_stockCode2DataDict = new Dictionary<string, StockData>();

    private List<KeyValuePair<string, StockData>> m_stockCode2DataList = new List<KeyValuePair<string, StockData>>();

    /// <summary>
    /// 根据股票代码获取数据
    /// </summary>
    /// <param name="stockCode"></param>
    /// <returns></returns>
    public List<StockKLineData>? GetStockData(string stockCode)
    {
        if(m_stockCode2DataDict.ContainsKey(stockCode))
        {
            return m_stockCode2DataDict[stockCode].perDayKLineList;
        }
        return null;
    }

    /// <summary>
    /// 获取所有股票的列表
    /// </summary>

    [PlannedTask(mode: PlannedTaskExecuteMode.ExecuteOnlyOnTime | PlannedTaskExecuteMode.ExecuteAfterTime | PlannedTaskExecuteMode.ExecuteOnLaunch, executeTime = "9:14")]
    public static void GetAllStockList()
    {
        HttpRequest.Get(StockQuoteInterface.Instance.GetQuoteUrl(StockQuoteInterfaceType.CurrentQuotes), (string json) => {
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
                            // 这里获取股票代码和名称，行业板块信息
                            string name = stockObject["f14"]?.ToString();
                            string code = stockObject["f12"]?.ToString();
                            string sectorCode = stockObject["f127"]?.ToString();

                            if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(code))
                            {
                                StockData stockData = null;
                                if (!StockDatabase.Instance.m_stockCode2DataDict.ContainsKey(code))
                                {
                                    stockData = StockDatabase.Instance.m_stockCode2DataDict[code] = new StockData()
                                    {
                                        code = code,
                                        name = name,
                                        sectorCode = sectorCode ?? "",
                                    };
                                }
                                else
                                {
                                    stockData = StockDatabase.Instance.m_stockCode2DataDict[code];
                                }
                                LifecycleManager.Instance.Get<EventManager>().RaiseEvent(EventType.UpdateHistoryQuotes, stockData);
                            }
                        }
                    }
                }
                // StockDatabase.Instance.UpdateHistoryQuotes();
            }
            catch (Exception ex)
            {
                WidgetManagement.GetWidget<MessageBox>().SetContent("StockQuoteInterfaceType.CurrentQuotes报错", ex.ToString());
            }
        });
    }

    /// <summary>
    /// 每天收盘时刻后将数据保存到本地
    /// </summary>
    [PlannedTask(mode: PlannedTaskExecuteMode.ExecuteOnlyOnTime | PlannedTaskExecuteMode.ExecuteAfterTime,executeTime = "15:00")]
    public static void SaveCurrentDayStockData()
    {

    }

    /*
        EASTMONEY_QUOTE_FIELDS = {
        'f12': '代码',
        'f14': '名称',
        'f3': '涨跌幅',
        'f2': '最新价',
        'f15': '最高',
        'f16': '最低',
        'f17': '今开',
        'f4': '涨跌额',
        'f8': '换手率',
        'f10': '量比',
        'f9': '动态市盈率',
        'f5': '成交量',
        'f6': '成交额',
        'f18': '昨日收盘',
        'f20': '总市值',
        'f21': '流通市值',
        'f13': '市场编号',
        'f124': '更新时间戳',
        'f297': '最新交易日',
    } 
    */
    private void UpdateHistoryQuotes()
    {

    }

    private static void UpdateHistoryQuotes(StockData stockData)
    {
        string url = StockQuoteInterface.Instance.GetQuoteUrl(StockQuoteInterfaceType.KLineData, UrlUtil.GetStockCodeParam("600000"), "20240112", "20240112");
        HttpRequest.GetSync(url, (string json) =>
        {
            string strippedJson = JsonStripperUtil.GetEastMoneyStrippedJson(json);
            try
            {
                JObject jsonRoot = JObject.Parse(strippedJson);
                JArray klinesArray = jsonRoot?["data"]?["klines"]?.ToObject<JArray>();
                if (klinesArray != null)
                {
                    for (int i = 0; i < klinesArray.Count; i++)
                    {
                        JToken klineToken = klinesArray[i];
                        string klineJson = klineToken.ToString();

                        // 2024-01-12,8.29,8.66,8.66,8.16,2718659,2311985347.48,6.35,10.04,0.79,21.33
                        // date,openPrice,price,highestPrice,lowestPrice,volumn，
                        string[] strings = klineJson.Split(',');
                        if (strings.Length > 0)
                        {
                            DateTime date = Convert.ToDateTime(strings[0]);
                            float openPrice = Convert.ToSingle(strings[1]);
                            float price = Convert.ToSingle(strings[2]);
                            float highestPrice = Convert.ToSingle(strings[3]);
                            float lowestPrice = Convert.ToSingle(strings[4]);
                            float volume = Convert.ToSingle(strings[5]);
                            float money = Convert.ToSingle(strings[6]);
                            float amplitude = Convert.ToSingle(strings[7]);
                            float percentage = Convert.ToSingle(strings[8]);
                            float priceChange = Convert.ToSingle(strings[9]);
                            float turnOverRate = Convert.ToSingle(strings[10]);

                            stockData.perDayKLineList.Add(new StockKLineData()
                            {
                                openPrice = openPrice,
                                closePrice = price,
                                highestPrice = highestPrice,
                                lowestPrice = lowestPrice,
                                volume = volume,
                                money = money,
                                amplitude = amplitude,
                                percentage = percentage,
                                priceChange = priceChange,
                                turnOverRate = turnOverRate
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                WidgetManagement.GetWidget<MessageBox>().SetContent("StockQuoteInterfaceType.CurrentQuotes报错", ex.ToString());
            }
        }
        );
    }

    public override void PostSerialization() 
    {
        foreach (KeyValuePair<string, StockData> kvp in m_stockCode2DataList)
        {
            m_stockCode2DataDict[kvp.Key] = kvp.Value;
        }
        LifecycleManager.Instance.Get<EventManager>().AddEventHandler(EventType.UpdateHistoryQuotes, new Action<object[]>((object[] param) => 
        {
            StockData stockData = (StockData)param[0];
            UpdateHistoryQuotes(stockData);
        })); 
    }
}