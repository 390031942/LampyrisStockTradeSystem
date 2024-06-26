﻿namespace LampyrisStockTradeSystem;

public class HKLinkQuoteExtractor : IStockQuoteInterface
{
    public override StockQuoteInterfaceType quetoType => StockQuoteInterfaceType.HKLink;

    protected override string url => "http://45.push2.eastmoney.com/api/qt/clist/get";
    protected override Dictionary<string, string> parameters => new Dictionary<string, string>()
    {
        { "cb", AppConfig.jQueryString },
        { "po", "1" },
        { "pz", "50000" },
        { "pn", "1" },
        { "np", "1" },
        { "fltt", "2" },
        { "invt", "2" },
        { "ut", "b2884a393a59ad64002292a3e90d46a5" },
        { "wbp2u","2710255628112086|0|1|0|web" },
        { "fid","f3"},
        { "fs","b:MK0146,b:MK0144"},
        { "fields", "f1,f2,f3,f4,f5,f6,f7,f8,f9,f10,f12,f13,f14,f15,f16,f17,f18,f19,f20,f21,f23,f24,f25,f26,f22,f33,f11,f62,f128,f136,f115,f152" },
        { "_","1711281402699"}
    };
}