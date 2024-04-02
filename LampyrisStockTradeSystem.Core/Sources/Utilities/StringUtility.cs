using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LampyrisStockTradeSystem;

public static class StringUtility
{
    public static string GetMoneyString(float money)
    {
        if(money < 10000.0f)
        {
            return Math.Round(money,2).ToString();
        }
        else if(money >= 10000.0f && money < 100000000.0f)
        {
            return Math.Floor(money / 10000.0f) + "万";
        }
        else
        {
            return Math.Round(money / 100000000.0f, 2) + "亿";
        }
    }
}
