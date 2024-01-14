/*
** Author: wushuhong
** Contact: gameta@qq.com
** Description: HTTP请求结果json处理
*/

namespace LampyrisStockTradeSystem;

public static class JsonStripperUtil
{
    public static string GetEastMoneyStrippedJson(string jsonString)
    {
        int jsonStringLength = jsonString.Length;
        int validStringLength = jsonStringLength - AppConfig.jQueryString.Length - 3;

        return jsonString.Substring(AppConfig.jQueryString.Length + 1, validStringLength);
    }
}
