using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace LampyrisStockTradeSystem;

[Serializable]
public class AppUIStyle:SerializableSingleton<AppUIStyle>
{
    // 行情中证券 "名字" 颜色
    public Vector4 quoteNameColor = new Vector4(0.8745f, 0.9843f, 0.6431f, 1f);

    // 行情 通用 颜色
    public Vector4 quoteNormalColor = new Vector4(0.78f, 0.78f, 0.78f, 1.0f);

    // 行情 上涨 颜色
    public Vector4 quoteRiseColor = new Vector4(1.0f, 0.36f, 0.36f, 1.0f);

    // 行情 下跌 颜色
    public Vector4 quoteFallColor = new Vector4(0.22f, 0.89f, 0.396f, 1.0f);

    // 行情 金额 颜色
    public Vector4 quoteMoneyColor = new Vector4(0.0f, 1.0f, 1.0f, 1.0f);
}
