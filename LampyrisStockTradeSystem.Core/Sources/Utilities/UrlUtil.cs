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

        // 港股代码长度为5
        if(stockCode.Length == 5)
        {
            return "116." + stockCode;
        }
        else if(stockCode.Length == 6)
        {
            if (stockCode.StartsWith("60"))
                return "1." + stockCode;
            else
                return "0." + stockCode;
        }

        return "";
    }
}
