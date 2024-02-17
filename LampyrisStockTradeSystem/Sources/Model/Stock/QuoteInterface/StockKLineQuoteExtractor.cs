using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LampyrisStockTradeSystem;

public class StockKLineDataExtractor : IStockQuoteInterface
{
    public override StockQuoteInterfaceType quetoType => StockQuoteInterfaceType.KLineData;

    protected override string url => "https://push2his.eastmoney.com/api/qt/stock/kline/get";

    protected override Dictionary<string, string> parameters => new Dictionary<string, string>()
    {
       {"cb",AppConfig.jQueryString},
       {"fields1","f1%2Cf2%2Cf3%2Cf4%2Cf5%2Cf6" },
       {"fields2","f51%2Cf52%2Cf53%2Cf54%2Cf55%2Cf56%2Cf57%2Cf58%2Cf59%2Cf60%2Cf61" },
       {"klt","101" },
       {"fqt","1" },
       {"secid","%0%" }, // %0%表示占位符，表示MakeUrl函数参数specificParams的第0个元素
       {"beg","%1%19900101" },
       {"end","%2%20990101" },
       {"rtntype","6"},
    };
}