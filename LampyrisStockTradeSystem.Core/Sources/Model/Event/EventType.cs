using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
