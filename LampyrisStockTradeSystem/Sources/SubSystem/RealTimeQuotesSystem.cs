/*
** Author: wushuhong
** Contact: gameta@qq.com
** Description: 实时股票行情系统
*/
namespace LampyrisStockTradeSystem;

public class RealTimeQuotesSystem
{
    [PlannedTask(mode:PlannedTaskExecuteMode.ExecuteDuringTime, "09:15-15:00",3000)]
    [MenuItem("行情/实时行情刷新")]
    public static void Update()
    {
        WidgetManagement.GetWidget<StockQuoteTableWindow>();
    }

    // 获取股票 突破日新高次数 的数据(选股策略)
    [PlannedTask(mode: PlannedTaskExecuteMode.ExecuteOnlyOnTime, "09:45&10:00")]
    public static void FindBreakThrough()
    {
        // StockDataExtractor.GetBreakthroughStockData();
    }

    /// <summary>
    /// TODO:待删除，测试函数
    /// </summary>
    [PlannedTask(mode: PlannedTaskExecuteMode.ExecuteOnLaunch)]
    public static void OnLaunch()
    {
        MessageBox msgBox = (MessageBox)WidgetManagement.GetWidget<MessageBox>();
        msgBox.SetContent("Test Message","Test Content");
    }
}
