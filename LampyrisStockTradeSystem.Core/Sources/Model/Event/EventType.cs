/*
** Author: wushuhong
** Contact: gameta@qq.com
** Description: 程序全局事件类型定义
*/
namespace LampyrisStockTradeSystem;

public enum EventType
{
    // 股票数据库相关
    UpdateHistoryQuotes,
    UpdateRealTimeQuotes,

    // 自动化下单相关
    LoginButtonClicked,
    PositionUpdate, // 持仓更新
    RevokeUpdate, // 撤单更新
}
