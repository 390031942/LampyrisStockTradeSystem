namespace LampyrisStockTradeSystem;

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
    public bool Satisfied(StockQuoteData stockData);
}

/// <summary>
/// 深圳主板
/// </summary>
public class SZMainBoardStockFilter : IStockTypeFilter
{
    public bool Satisfied(StockQuoteData stockData)
    {
        return stockData != null && stockData.code.StartsWith("00");
    }
}

/// <summary>
/// 上海主板
/// </summary>
public class SHMainBoardStockFilter : IStockTypeFilter
{
    public bool Satisfied(StockQuoteData stockData)
    {
        return stockData != null && stockData.code.StartsWith("60");
    }
}

/// <summary>
/// 沪深主板
/// </summary>
public class MainBoardStockFilter : IStockTypeFilter
{
    public bool Satisfied(StockQuoteData stockData)
    {
        return stockData != null && (stockData.code.StartsWith("60") || stockData.code.StartsWith("00"));
    }
}

/// <summary>
/// 创业板
/// </summary>
public class ChiNextStockFilter : IStockTypeFilter
{
    public bool Satisfied(StockQuoteData stockData)
    {
        return stockData != null && stockData.code.StartsWith("30");
    }
}