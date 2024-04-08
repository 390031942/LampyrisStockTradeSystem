/*
** Author: wushuhong
** Contact: gameta@qq.com
** Description: 应用设置数据
*/

namespace LampyrisStockTradeSystem;

[Serializable]
public class AppSettings:SerializableSingleton<AppSettings>
{
    private string m_chromeProgramPath = "chrome-win64\\chrome.exe";

    private string m_chromeDriverProgramPath = "chrome-win64\\chromedriver.exe";

    public string chromeProgramPath => m_chromeProgramPath;

    public string chromeDriverProgramPath => m_chromeDriverProgramPath;


    // 仓位选择
    public int tradeOrderRatio = 1;

    // 成交剩余策略
    public int tradeOrderLeftStrategy = 1;

    // 异动筛选策略
    public int unusualStrategy = 1;

    // 卖出档位
    public int askLevel = 3;

    // 买入档位
    public int bidLevel = 3;
}
