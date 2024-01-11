/*
** Author: wushuhong
** Contact: gameta@qq.com
** Description: 应用设置数据
*/

namespace LampyrisStockTradeSystem;

[Serializable]
public class AppSettings:SerializableSingleton<AppSettings>
{
    private string m_chromeProgramPath = "C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe";

    private string m_chromeDriverProgramPath = "C:\\Program Files\\Google\\Chrome\\Application\\chromedriver.exe";

    public string chromeProgramPath => m_chromeProgramPath;

    public string chromeDriverProgramPath => m_chromeDriverProgramPath;
}
