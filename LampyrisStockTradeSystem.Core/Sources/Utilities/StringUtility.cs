using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LampyrisStockTradeSystem;

public static class StringUtility
{
    public static string GetMoneyString(float money,int round1 = 2, int round2 = 0, int round3 = 2)
    {
        if(money < 10000.0f)
        {
            return Math.Round(money,round1).ToString();
        }
        else if(money >= 10000.0f && money < 100000000.0f)
        {
            return Math.Round(money / 10000.0f, round2) + "万";
        }
        else
        {
            return Math.Round(money / 100000000.0f, round3) + "亿";
        }
    }
}
