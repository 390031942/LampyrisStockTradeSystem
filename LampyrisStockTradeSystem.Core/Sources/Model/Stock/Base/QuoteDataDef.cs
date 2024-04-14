/*
** Author: wushuhong
** Contact: gameta@qq.com
** Description: 行情数据的定义
*/

using Newtonsoft.Json.Linq;
using System.Globalization;

namespace LampyrisStockTradeSystem;

/* TODO:指标数据接口 */
public class IStockKLineIndicator
{

}

/* MA指标 */
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

/* K线分析周期，跨度从 1分钟线 到 年线 */
public enum KLineDataCycle
{
   Minute1,
   Minute5,
   Minute15,
   Minute30,
   Minute60,
   Minute120,
   Day,
   Week,
   Month,
   Season,
   HalfYear,
   Year
}

/* K线数据，这里不一定指的是日K，只是泛指一根K线的数据，可以表示为 某只股票 或者是 指数 的行情数据 */
[Serializable]
public class KLineData
{
    /// <summary>
    /// 日期
    /// </summary>
    public DateTime date;

    /// <summary>
    /// K线分析周期
    /// </summary>
    public KLineDataCycle cycle = KLineDataCycle.Day;

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

    /// <summary>
    /// 昨日收盘
    /// </summary>
    public float lastClosePrice;
}

/// <summary>
/// 历史分时数据 的走势数据
/// </summary>
[Serializable]
public class HistoryTimeShareMinuteData
{
    public float price;
    public float volumn;
}

/// <summary>
/// 历史分时数据
/// </summary>
[Serializable]
public class HistoryTimeShareData
{
    /// <summary>
    /// 每分钟的数据
    /// </summary>
    public List<HistoryTimeShareMinuteData> minuteDataList = new List<HistoryTimeShareMinuteData>();
}

/// <summary>
/// 行情数据的基类
/// </summary>
[Serializable]
public class QuoteData
{
    /// <summary>
    /// 代码
    /// </summary>
    public string code;

    /// <summary>
    /// 名称
    /// </summary>
    public string name;

    /// <summary>
    /// 缩写，比如"深中华A" -> "SZHA"
    /// </summary>
    public string abbreviation;

    /// <summary>
    /// 日K线数据
    /// </summary>
    private List<KLineData> m_perDayKLineList;
    
    public List<KLineData> perDayKLineList
    {
        get
        {
            if (m_perDayKLineList == null || m_perDayKLineList.Count == 0)
            {
                m_perDayKLineList = new List<KLineData>();
                string url = StockQuoteInterface.Instance.GetQuoteUrl(StockQuoteInterfaceType.KLineData, UrlUtil.GetStockCodeParam(code), "20240102", "20991231");
                HttpRequest.GetSync(url, (string json) =>
                {
                    string strippedJson = JsonStripperUtil.GetEastMoneyStrippedJson(json);
                    try
                    {
                        JObject jsonRoot = JObject.Parse(strippedJson);

                        JArray stockDataArray = jsonRoot?["data"]?["klines"]?.ToObject<JArray>();
                        if (stockDataArray != null)
                        {
                            for (int i = 0; i < stockDataArray.Count; i++)
                            {
                                string kLineDataString = stockDataArray[i].ToObject<string>();
                                // 每一行的数据格式
                                // 日期,开盘价,收盘价,最高价,最低价,成交量,成交额,振幅,涨跌幅,涨跌额,换手率
                                string[] strings = kLineDataString.Split(',');

                                KLineData kLineData = new KLineData();
                                kLineData.date = DateTime.ParseExact(strings[0], "yyyy-MM-dd", CultureInfo.InvariantCulture);
                                kLineData.openPrice = float.Parse(strings[1]);
                                kLineData.closePrice = float.Parse(strings[2]);
                                kLineData.highestPrice = float.Parse(strings[3]);
                                kLineData.lowestPrice = float.Parse(strings[4]);
                                kLineData.volume = float.Parse(strings[5]);
                                kLineData.money = float.Parse(strings[6]);
                                kLineData.amplitude = float.Parse(strings[7]);
                                kLineData.percentage = float.Parse(strings[8]);
                                kLineData.priceChange = float.Parse(strings[9]);
                                kLineData.turnOverRate = float.Parse(strings[10]);

                                m_perDayKLineList.Add(kLineData);
                            }
                        }
                    }
                    catch (Exception ex) { }
                });
            }
            return m_perDayKLineList;
        }
    }

    /// <summary>
    /// 实时行情数据
    /// </summary>
    public RealTimeQuoteData realTimeQuoteData;
}

/// <summary>
/// 指数行情的数据
/// </summary>
[Serializable]
public class IndexQuoteData: QuoteData
{
    public IndexQuoteData()
    {
        realTimeQuoteData = new RealTimeQuoteData();
    }

    /// <summary>
    /// 成分股
    /// </summary>
    public List<StockQuoteData> stockQuoteDatas = new List<StockQuoteData>();

    /// <summary>
    /// 上涨加数
    /// </summary>
    public int riseCount;

    /// <summary>
    /// 平盘家数
    /// </summary>
    public int unchangedCount;

    /// <summary>
    /// 下跌家数
    /// </summary>
    public int fallCount;
}

/// <summary>
/// 指数行情的简明数据，只包含三个行情指标，方便在状态栏等地方展示
/// </summary>
[Serializable]
public class IndexBriefQuoteData
{
    // 指数代码
    public string code;

    // 指数名称
    public string name;

    // 指数现价
    public float currentPrice;

    // 指数涨跌额度
    public float priceChange;

    // 指数涨跌幅
    public float percentage;
}

/// <summary>
/// 股票数据 = 股票日K数据 + 基本面，TODO：以后会加入周线，分钟线等不同行情周期的数据
/// </summary>
[Serializable]
public class StockQuoteData:QuoteData
{
    public StockQuoteData()
    {
        realTimeQuoteData = new StockRealTimeQuoteData();
    }

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
    /// 市盈率(静态)
    /// </summary>
    public float PERatio;

    /// <summary>
    /// 市盈率(动态)
    /// </summary>
    public float PERatioDynamic
    {
        get
        {
            if (perDayKLineList.Count > 0 && EPS != 0.0f)
            {
                KLineData stockKLineData = perDayKLineList.Last();
                return stockKLineData.closePrice / EPS;
            }
            else
            {
                return 0.0f;
            }
        }
    }

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
/// 买/卖档 申报信息
/// </summary>
[Serializable]
public class AskBidData
{
    // 申报的价格和数量,如果价格为0表示这个档位不存在申报信息
    public float price;
    public float count;

    // 挂单队列，Level-2模式才有
    public List<float> queue;
}

/// <summary>
/// 买/卖五档申报信息(Level-1)
/// </summary>
[Serializable]
public abstract class AskBidQuoteData
{
    // 申报 买的
    public AskBidData[] bid;
    // 申报 卖的
    public AskBidData[] ask;

    public AskBidQuoteData()
    {
        // 数组长度为6 是为了方便取值1-5档，index = 0在这里没有意义
        // 申报 买的
        bid = new AskBidData[6];
        ask = new AskBidData[6];
    }

    // 档位数量(Level1 = 5, Level2 = 1000) 
    public abstract int count { get; }

    /// <summary>
    /// 委比
    /// </summary>
    public float theCommittee;
}

/// <summary>
/// 股票买/卖五档申报信息(Level-1)
/// </summary>
[Serializable]
public class AskBidQuoteDataLevelOne : AskBidQuoteData
{
    public override int count => 5;
}

/// <summary>
/// 股票买/卖五档申报信息(Level-2,占坑)
/// </summary>
[Serializable]
public class AskBidQuoteDataLevelTwo : AskBidQuoteData
{
    public override int count => 1000;
}

/// <summary>
/// 成交明细类型
/// </summary>
[Serializable]
public enum TransactionType
{
    Buy = 1, // 买
    Sell = 2, // 卖
}

/// <summary>
/// 成交明细
/// </summary>
[Serializable]
public class TransactionDetailData
{
    public TransactionType type;
    public float count;
}

/// <summary>
/// 交易的内外盘数量
/// </summary>
[Serializable]
public class TransactionSumData
{
    public float buyCount;
    public float sellCount;
}

/// <summary>
/// 实时行情基类，分为股票行情和指数/板块行情,以后还可以支持ETF行情,股指期货等
/// </summary>
[Serializable]
public class RealTimeQuoteData
{
    /// <summary>
    /// 实时K线数据，包含了价格，成交量，换手率等数据
    /// </summary>
    public KLineData kLineData = new KLineData();

    /// <summary>
    /// 分时数据，包括每分钟的数据
    /// </summary>
    public List<KLineData> minuteData = new List<KLineData>();

    /// <summary>
    /// 成交明细
    /// </summary>
    public List<TransactionDetailData> transactionDetailDataList = new List<TransactionDetailData>();

    /// <summary>
    /// 内外盘信息
    /// </summary>
    public TransactionSumData transactionSumData = new TransactionSumData();
}

/// <summary>
/// 股票的实时行情，在价格，成交量，换手率等数据的基础上加上了分时走势
/// </summary>
[Serializable]
public class StockRealTimeQuoteData : RealTimeQuoteData
{
    /// <summary>
    /// 股票买/卖五档申报信息(Level-1)
    /// </summary>
    public AskBidQuoteData bidAskData = new AskBidQuoteDataLevelOne();

    /// <summary>
    /// 量比
    /// </summary>
    public float volumnRate;

    /// <summary>
    /// 涨速
    /// </summary>
    public float riseSpeed;

    /// <summary>
    /// 买入价
    /// </summary>
    public float buyPrice;

    /// <summary>
    /// 卖入价
    /// </summary>
    public float sellPrice;
}

[Serializable]
public class IndexRealTimeQuoteData : RealTimeQuoteData
{
    /// <summary>
    /// 上涨/下跌/平盘数量
    /// </summary>
    public int riseCount;
    public int fallCount;
    public int unchangedCount;
}