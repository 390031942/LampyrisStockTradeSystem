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

}