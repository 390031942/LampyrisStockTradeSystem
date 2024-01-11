namespace LampyrisStockTradeSystem;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class RealTimeQuotesSystem
{
    [PlannedTask(mode:PlannedTaskExecuteMode.ExecuteDuringTime, "09:15-15:00",3000)]
    [MenuItem("行情/实时行情")]
    public static void Update()
    {
        WidgetManagement.GetWidget<StockQuoteTableWindow>();

        StockDataExtractor.RequestRealTimeQuotes();
    }

    // 获取股票 突破日新高次数 的数据
    [PlannedTask(mode: PlannedTaskExecuteMode.ExecuteOnlyOnTime, "09:45&10:00")]
    public static void FindBreakThrough()
    {
        // StockDataExtractor.GetBreakthroughStockData();
    }


    [PlannedTask(mode: PlannedTaskExecuteMode.ExecuteOnLaunch)]
    public static void OnLaunch()
    {
        MessageBox msgBox = (MessageBox)WidgetManagement.GetWidget<MessageBox>();
        msgBox.SetContent("Test Message","Test Content");
    }

    [PlannedTask(mode: PlannedTaskExecuteMode.ExecuteDuringTime, executeTime = "23:10-23:59",intervalMs = 3000)]
    public static void UpdateMessage()
    {
        WidgetManagement.GetWidget<MessageBox>().SetContent("1","2");
    }
}
