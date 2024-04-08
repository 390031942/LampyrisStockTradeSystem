/*
** Author: wushuhong
** Contact: gameta@qq.com
** Description: 应用程序颜色信息
*/
using System.Numerics;

namespace LampyrisStockTradeSystem;

[Serializable]
public class AppUIStyle:SerializableSingleton<AppUIStyle>
{
    // 行情中证券 "名字" 颜色
    public SerializableVector4 quoteNameColor = new SerializableVector4(0.8745f, 0.9843f, 0.6431f, 1f);

    // 行情 通用 颜色
    public SerializableVector4 quoteNormalColor = new SerializableVector4(0.78f, 0.78f, 0.78f, 1.0f);

    // 行情 上涨 颜色
    public SerializableVector4 quoteRiseColor = new SerializableVector4(1.0f, 0.36f, 0.36f, 1.0f);

    // 行情 下跌 颜色
    public SerializableVector4 quoteFallColor = new SerializableVector4(0.22f, 0.89f, 0.396f, 1.0f);

    // 行情 金额 颜色
    public SerializableVector4 quoteMoneyColor = new SerializableVector4(0.0f, 1.0f, 1.0f, 1.0f);

    // 普通的白色
    public SerializableVector4 normalWhiteColor = new SerializableVector4(1.0f, 1.0f, 1.0f, 1.0f);

    // 提示-黄色
    public SerializableVector4 tipYellowColor = new SerializableVector4(1.0f, 1.0f, 0.15f, 1.0f);

    // 提示-红色
    public SerializableVector4 tipRedColor = new SerializableVector4(1.0f, 0.36f, 0.36f, 1.0f);

    // 涨跌颜色反转
    public bool reverseRiseFallColor = false;

    public Vector4 GetRiseFallColor(float percentage)
    {
        if(percentage > 0.0f)
        {
            return !reverseRiseFallColor ? quoteRiseColor : quoteFallColor;
        }
        else if(percentage < 0.0f)
        {
            return reverseRiseFallColor ? quoteRiseColor : quoteFallColor;
        }

        return (Vector4)normalWhiteColor;
    }
}
