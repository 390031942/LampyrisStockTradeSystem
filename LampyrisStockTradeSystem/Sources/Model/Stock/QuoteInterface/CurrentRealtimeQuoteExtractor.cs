namespace LampyrisStockTradeSystem;

public class CurrentRealtimeQuoteExtractor : IStockQuoteInterface
{
    public override StockQuoteInterfaceType quetoType => StockQuoteInterfaceType.CurrentQuotes;

    protected override string url => "https://push2.eastmoney.com/api/qt/clist/get";
    protected override Dictionary<string, string> parameters => new Dictionary<string, string>()
    {
        { "cb", AppConfig.jQueryString },
        { "fid", "f62" },
        { "po", "1" },
        { "pz", "50000" },
        { "pn", "1" },
        { "np", "1" },
        { "fltt", "2" },
        { "invt", "2" },
        { "ut", "b2884a393a59ad64002292a3e90d46a5" },
        { "fs", "m%3A0%2Bt%3A6%2Bf%3A!2%2Cm%3A0%2Bt%3A13%2Bf%3A!2%2Cm%3A0%2Bt%3A80%2Bf%3A!2%2Cm%3A1%2Bt%3A2%2Bf%3A!2%2Cm%3A1%2Bt%3A23%2Bf%3A!2%2Cm%3A0%2Bt%3A7%2Bf%3A!2%2Cm%3A1%2Bt%3A3%2Bf%3A!2" },
        { "fields", "f2%2Cf3%2Cf4%2Cf8%2Cf12%2Cf14%2Cf22%2Cf31%2Cf32%2Cf33%2Cf34%2Cf35" }
    };

}
