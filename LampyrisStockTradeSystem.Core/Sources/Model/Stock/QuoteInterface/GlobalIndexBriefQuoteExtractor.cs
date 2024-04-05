namespace LampyrisStockTradeSystem;

public class GlobalIndexBriefQuoteExtractor: IStockQuoteInterface
{
    public override StockQuoteInterfaceType quetoType => StockQuoteInterfaceType.GlobalIndexBrief;

    protected override string url => "http://42.push2.eastmoney.com/api/qt/clist/get";

    protected override Dictionary<string, string> parameters => new Dictionary<string, string>()
    {
        { "cb", AppConfig.jQueryString },
        { "po", "1" },
        { "pz", "50" },
        { "pn", "1" },
        { "np", "1" },
        { "fltt", "2" },
        { "invt", "2" },
        { "ut", "b2884a393a59ad64002292a3e90d46a5" },
        { "wbp2u","2710255628112086|0|1|0|web" },
        { "fs","i:1.000001,i:0.399001,i:100.HSI,i:100.NDX"},
        { "fields", "f2,f3,f4,f14" },
        { "_","1711281402699"}
    };
}
