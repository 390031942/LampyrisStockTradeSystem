namespace LampyrisStockTradeSystem;

public class IntradayTradingDetailExtractor : IStockQuoteInterface
{
    public override StockQuoteInterfaceType quetoType => StockQuoteInterfaceType.IntradayTradingDetail;

    protected override string url => throw new NotImplementedException();

    protected override Dictionary<string, string> parameters => throw new NotImplementedException();
}
