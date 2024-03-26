using NPinyin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LampyrisStockTradeSystem;

public static class ChineseUtil
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="s">原始字符串对象</param>
    /// <param name="abbreviation">是否转化为拼音缩写，"深中华A" -> "SZHA" </param>
    /// <returns></returns>
    public static string ToPinYin(this string s,bool abbreviation)
    {
        string result = Pinyin.GetPinyin(s);
        if(abbreviation)
        {
            result = string.Concat(result.Split(' ')
                                         .Where(word => !string.IsNullOrEmpty(word))
                                         .Select(word => word.Substring(0, 1).ToUpper()));

        }
        return result;
    }
}
