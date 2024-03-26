/*
** Author: wushuhong
** Contact: gameta@qq.com
** Description: 佳明选股指标策略的实现类,对于上市时间大于等于365天的个股，实现了:
* 1) 涨跌停家数曲线：
     统计每天涨停和跌停的家数，在同一个图内形成两条曲线。
* 2) 连板天梯图：
     统计2板及2板以上家数形成连板曲线图。
     统计最大连板高度形成曲线图。
* 3) 追涨抄底效应曲线:
     1. 涨幅榜前50（剔除一字板），1天、2天、5天后的平均涨幅，形成3条曲线。
    （昨天涨幅榜前50今天平均收益，前天涨幅榜前50今天平均收益，5天前涨幅榜前50今天平均收益（创业板涨跌幅/2再进行计算)
     2. 跌幅榜前50（剔除一字板），1天、2天、5天后的平均跌幅，形成3条曲线。
    （昨天跌幅榜前50今天平均收益，前天跌幅榜前50今天平均收益，5天前跌幅榜前50今天平均收益（创业板涨跌幅/2再进行计算)
*/
using LampyrisStockTradeSystem;

namespace LampyrisStockTradeSystem.GaaiMingStrategy;

public class GaimmingStrategy
{
    public List<Tuple<int,int>> GetRiseAndDownLimitCount()
    {
        QuoteDatabase.Instance.GetStockDatasConditiona();
    }
}

public static class PluginEntrance
{
    [MenuItem("选股工具")]
    public static void ShowWindow()
    {

    }
}