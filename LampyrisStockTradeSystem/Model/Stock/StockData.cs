/*
** Author: wushuhong
** Contact: gameta@qq.com
** Description: 股票数据的定义
*/

using System.Security.Cryptography.X509Certificates;

namespace LampyrisStockTradeSystem;

/* TODO:股票指标数据接口 */
public class IStockKLineIndicator
{

}

/* 股票MA指标 */
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

public enum StockType
{
    // 深证主板
    SZ_MainBoard = 1,
    // 上海主板(包含中小板)
    SH_MainBoard = 2,
    // 沪深主板
    MainBoard = SZ_MainBoard | SH_MainBoard,
    // 京市主板
    BJ_MainBoard = 4,
    // ST股
    ST = 8,
    // *ST股
    Star_ST = 16,
    // 创业板
    ChiNext = 32,
    // 科创板
    ScienceInnovation = 64,
    // 新股
    New = 128,
    // 上市交易后的第二个交易日至第五个交易日之间,无涨跌幅限制
    C_Prefix = 256,
}

public interface IStockTypeFilter
{
    public bool Satisfied(StockData stockData);
}

/// <summary>
/// 深圳主板
/// </summary>
public class SZMainBoardStockFilter : IStockTypeFilter
{
    public bool Satisfied(StockData stockData)
    {
        return (stockData == null && stockData.code.StartsWith("00"));
    }
}

/// <summary>
/// 上海主板
/// </summary>
public class SHMainBoardStockFilter : IStockTypeFilter
{
    public bool Satisfied(StockData stockData)
    {
        return (stockData != null && stockData.code.StartsWith("60"));
    }
}

/// <summary>
/// 沪深主板
/// </summary>
public class MainBoardStockFilter : IStockTypeFilter
{
    public bool Satisfied(StockData stockData)
    {
        return (stockData != null && (stockData.code.StartsWith("60") || stockData.code.StartsWith("00")));
    }
}

/// <summary>
/// 创业板
/// </summary>
public class ChiNextStockFilter : IStockTypeFilter
{
    public bool Satisfied(StockData stockData)
    {
        return (stockData != null && (stockData.code.StartsWith("30"));
    }
}

// 股票行情数据库，TODO：需要序列化保存 以便于实现 差异化请求数据
class StockDatabase:SerializableSingleton<StockDatabase>
{
    /// <summary>
    /// 股票代码->股票数据字典
    /// </summary>
    private static Dictionary<string, StockData> ms_stockCode2DataDict = new Dictionary<string, StockData>();


    /// <summary>
    /// 根据股票代码获取数据
    /// </summary>
    /// <param name="stockCode"></param>
    /// <returns></returns>
    public static List<StockKLineData>? GetStockData(string stockCode)
    {
        if(ms_stockCode2DataDict.ContainsKey(stockCode))
        {
            return ms_stockCode2DataDict[stockCode].perDayKLineList;
        }
        return null;
    }

    /// <summary>
    /// 每天收盘时刻后将数据保存到本地
    /// </summary>
    [PlannedTask(mode: PlannedTaskExecuteMode.ExecuteOnlyOnTime | PlannedTaskExecuteMode.ExecuteAfterTime,executeTime = "15:00")]
    public static void SaveCurrentDayStockData()
    {
    }
}