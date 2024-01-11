/*
** Author:      wushuhong
** Contact:     gameta@qq.com
** Description: 股票数据的定义
*/

namespace LampyrisStockTradeSystem;

/* 股票指标数据接口 */
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

class StockData
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

class StockDatabase
{
    /// <summary>
    /// 股票代码->股票数据字典
    /// </summary>
    private static Dictionary<string, StockData> ms_stockCode2DataDict = new Dictionary<string, StockData>();

    private static void Read()
    {
        using (var reader = new StreamReader(@"C:\path\to\your\file.csv"))
        {
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var values = line.Split(',');

                
            }
        }
    }

    public static void Init()
    {
        
    }

    public static List<StockKLineData>? GetStockData(string stockCode)
    {
        if(ms_stockCode2DataDict.ContainsKey(stockCode))
        {
            return ms_stockCode2DataDict[stockCode].perDayKLineList;
        }
        return null;
    }

    [PlannedTask(mode: PlannedTaskExecuteMode.ExecuteOnlyOnTime | PlannedTaskExecuteMode.ExecuteOnlyOnTime,executeTime = "15:00")]
    public static void Refresh()
    {
        string date = DateUtil.GetCurrentDateString();
    }
}