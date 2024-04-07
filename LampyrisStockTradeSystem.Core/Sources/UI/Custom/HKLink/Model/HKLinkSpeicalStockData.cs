/*
** Author: wushuhong
** Contact: gameta@qq.com
** Description: 港股通精选股写死名单(暂时的)
*/
namespace LampyrisStockTradeSystem;

[Serializable]
public class HKLinkSpeicalStockData:SerializableSingleton<HKLinkSpeicalStockData>
{
    // 标黄精选股
    public HashSet<string> speicalStockDataSet = new HashSet<string>()
    {
        "京基金融国际",
        "微创医疗",
        "易联融科技-w",
        "信义光能",
        "华晨中国",
        "COSMOPL INT'L",
        "第四范式",
        "马可数字科技",
        "易点云",
        "TCL电子",
        "赛生药业",
        "丘钛科技",
        "柠萌影视",
        "药师帮",
        "李宁",
        "中铝国际",
        "创新奇智",
        "富力地产",
        "理想汽车",
        "高鑫零售",
        "希教国际控股",
        "阜博集团",
        "绿景中国地产",
        "时代天使",
        "融创服务",
        "龙光集团",
        "北森控股",
        "南京熊猫电子股份",
        "上美股份",
        "山东新华制药股份",
        "海伦司",
        "宜明昂科-B",
        "中国科培",
        "来凯医药-B",
        "再鼎医药",
        "东方海外国际",
        "FIT HON TENG",
        "药明合联",
        "北京北辰实业股份",
        "中国外运",
        "移卡",
        "中教控股",
        "3D MEDICINES",
        "龙湖集团",
        "明源云",
        "郑煤机",
        "平安好医生",
        "浙江世宝",
        "健世科技-B",
        "昊海生物科技",
        "JS环球生活",
        "圣诺医药-B",
        "同道猎聘",
        "中烟香港",
        "碧桂园服务",
        "猫眼娱乐",
        "君盛泰医药-B",
        "易鑫集团",
        "优必选",
        "华宝国际",
        "中旭未来",
        "云顶新耀-B",
        "宝龙地产",
        "珍酒李渡",
        "远洋集团",
        "泰格医药",
        "康希诺生物",
        "爱康医疗",
        "阿里健康",
        "KEEP",
        "东方甄选",
        "中国金茂",
        "友联国际教育租赁",
        "科伦博泰生物-B",
        "泉峰控股",
        "H&H国际控股"
    };

    // 标红精选股
    public HashSet<string> speicalExStockDataSet = new HashSet<string>()
    {
        "微创医疗",
        "华晨中国",
        "易点云",
        "中铝国际",
        "阜博集团",
        "南京熊猫电子股份",
        "宜明昂科-B",
        "来凯医药-B",
        "药明合联",
        "健世科技-B",
        "圣诺医药-B",
        "同道猎聘",
        "君盛泰医药-B",
        "优必选",
        "中旭未来",
        "远洋集团",
        "KEEP",
        "东方甄选"
    };
}
