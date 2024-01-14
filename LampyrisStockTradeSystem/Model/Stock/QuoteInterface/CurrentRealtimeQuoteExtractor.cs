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
        { "fields", "f4%2Cf22%2Cf12%2Cf14%2Cf2%2Cf3%2Cf62%2Cf184%2Cf66%2Cf69%2Cf72%2Cf75%2Cf78%2Cf81%2Cf84%2Cf87%2Cf204%2Cf205%2Cf124%2Cf1%2Cf13%2Cf116%2Cf117%2Cf198%2Cf105%20%E2%80%94%E2%80%94%E2%80%94%E2%80%94%E2%80%94%E2%80%94%E2%80%94%E2%80%94%E2%80%94%E2%80%94%E2%80%94%E2%80%94%E2%80%94%E2%80%94%E2%80%94%E2%80%94%" }
    };

}
