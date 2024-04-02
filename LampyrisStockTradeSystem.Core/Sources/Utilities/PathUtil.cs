/*
** Author: wushuhong
** Contact: gameta@qq.com
** Description: 路径实用工具类
*/
namespace LampyrisStockTradeSystem;


public static class PathUtil
{
    public static string SerializedDataSavePath
    {
        get
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),AppConfig.AppDocFolderName);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return path;
        }
    }

    public static string CookieDataSavePath
    {
        get
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), AppConfig.CookieFolder);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return path;
        }
    }
}
