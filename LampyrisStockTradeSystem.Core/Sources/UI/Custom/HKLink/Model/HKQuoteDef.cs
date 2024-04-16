/*
** Author: wushuhong
** Contact: gameta@qq.com
** Description: 港股行情相关数据定义
*/

namespace LampyrisStockTradeSystem;

// 港股追高行情数据
public class HKChaseRiseQuoteData
{
    // 行情数据
    public QuoteData quoteData;

    // K线数据
    public Bitmap klineImage;
    public int klineTextureId;
    public bool isReloadKline = false;

    // 今日分时走势
    public Bitmap todayImage;
    public int todayImageTextureId;
    public bool isReloadToday = false;

    // 异步加载任务
    public Task<byte[]>? loadTodayImageTask;
    public Task<byte[]>? loadkLineImageTask;

    // 日内 分钟-分时图 破新高次数
    public int breakthroughTimes;

    // 成交额 排位 百分比(0.1表示当前成交额在所有股票中排 前10%)
    public double moneyRank;

    public bool displayingToday = true;

    public int lastUnusualTimestamp = -1;

    public string lastUnusualTime;

    public float recent2MinMaxMoney;
}

public enum HKStockUnusualStrategy
{
    [NamedValue("涨速≥1.5%(对于股价≤1.5元的涨速≥2%),且成交金额排位前50%")]
    RiseSpeedNormal = 1,

    [NamedValue("涨速≥5%(无视成交金额排位)")]
    RiseSpeedEx = 2,

    [NamedValue("跌速≥5%(无视成交金额排位)")]
    FallSpeed = 3,

    [NamedValue("日内分钟分时突破新高次数达到5的倍数(如5,10,15,20...)")]
    Breakthrough = 4,

    [NamedValue("涨幅≥10%")]
    RisePercentage = 5,

    [NamedValue("跌幅≥10%")]
    FallPercentage = 6,

    [NamedValue("(测试专用)涨速≥1.5%")]
    RiseSpeedTest = 7,

    Count = 7,
}