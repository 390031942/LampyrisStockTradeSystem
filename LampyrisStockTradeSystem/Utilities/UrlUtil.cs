using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class UrlUtil
{
    public static string GetStockCodeParam(string stockCode)
    {
        if (string.IsNullOrEmpty(stockCode))
            return String.Empty;

        if (stockCode.StartsWith("60"))
            return "1." + stockCode;
        else
            return "0." + stockCode;
    }
}
