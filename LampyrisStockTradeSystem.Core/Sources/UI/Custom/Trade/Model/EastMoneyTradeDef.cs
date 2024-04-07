/*
** Author: wushuhong
** Contact: gameta@qq.com
** Description: 东方财富通交易信息定义定义
*/

namespace LampyrisStockTradeSystem;

public class EastMoneyPositionStockInfo
{
    public string stockCode;
    public string stockName;
    public string count;
    public string useableCount;
    public string costPrice;
    public string currentPrice;
    public string money;
    public string profitLose;
    public string profitLoseRatio;
    public string todayProfitLost;
    public string todayProfitLostRatio;
}

// 持仓信息
public class EastMoneyPositionInfo
{
    // 总资产
    public string totalMoney;

    // 持仓资产
    public string positionMoney;

    // 持仓盈亏
    public string positionProfitLose;

    // 当日盈亏
    public string todayProfitLose;

    // 可用资金
    public string canUseMoney;

    public List<EastMoneyPositionStockInfo> stockInfos = new List<EastMoneyPositionStockInfo>();
}

// 撤单信息
public class EastMoneyRevokeInfo
{
    public List<EastMoneyRevokeStockInfo> stockInfos = new List<EastMoneyRevokeStockInfo>();
}

public class EastMoneyRevokeStockInfo
{
    // 时间
    public string timeString;

    // 是不是买入
    public bool isBuy;

    // 代码
    public string stockCode;

    // 名称
    public string stockName;

    // 委托数量
    public int orderCount;

    // 成交数量
    public int dealCount;

    // 委托价格
    public float orderPrice;

    // 成交价格
    public float dealPrice;

    // 成交金额
    public float dealMoney;

    // 状态
    public string status;

    // 委托编号
    public int id;
}


// 成交的信息
public class EastMoneyDealInfo
{

}

// 循环申报信息
public class EastMoneyCircularOrderInfo
{
    // 代码
    public string stockCode;

    // 名称
    public string stockName;

    // true:买入方向,false:卖出方向
    public bool isBuy;

    // 循环下单 重复次数
    public int retryCount;

    // 下单档位
    public int askBidLevel;

    // 总申报股数
    public int totalCount;

    // 剩余股数
    public int leftCount;

    // 上一次的委托ID
    public int previousOrderId;
}

// 仓位选择
public enum TradeOrderRatio
{
    [NamedValue("梭哈")]
    All = 1,

    [NamedValue("1/2")]
    Half = 2,

    [NamedValue("1/3")]
    Third = 3,

    [NamedValue("1/4")]
    Qurater = 4,

    Count = 4,
}

// 委托剩余成交策略
public enum TradeOrderLeftStrategy
{
    [NamedValue("即撤")]
    Cancel = 1,

    [NamedValue("循环")]
    Circular = 2,

    Count = 2,
}

// 买入卖出委托挡位选择
public enum TradeAskBidLevel
{
    [NamedValue("一档")]
    One = 1,

    [NamedValue("二档")]
    Two = 2,

    [NamedValue("三档")]
    Three = 3,

    [NamedValue("四档")]
    Four = 4,

    [NamedValue("五档")]
    Five = 5,

    Count = 5,
}