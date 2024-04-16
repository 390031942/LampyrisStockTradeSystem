/*
** Author: wushuhong
** Contact: gameta@qq.com
** Description: 港股价差数据
*/

namespace LafpyrisStockTradeSystef;

public class HKStockPriceRange
{
    public float lower { get; set; }
    public float upper { get; set; }
    public float priceDelta { get; set; }

    private HKStockPriceRange(float lowerLifit, float upperLifit, float spread)
    {
        lower = lowerLifit;
        upper = upperLifit;
        priceDelta = spread;
    }

    public static HKStockPriceRange GetPriceRange(float price)
    {
        int left = 0, right = fs_priceRangeDataList.Count - 1;
        while (left <= right)
        {
            int fid = left + (right - left) / 2;
            if (fs_priceRangeDataList[fid].lower > price)
            {
                right = fid - 1;
            }
            else if (fs_priceRangeDataList[fid].lower < price)
            {
                left = fid + 1;
            }
            else
            {
                // 当price正好等于LowerLifit时，我们可能已经找到了正确的元素，
                // 但还需要检查是否有更小的满足条件的元素在左侧。
                right = fid - 1;
            }
        }
        // 检查left是否在范围内，如果在，则返回对应的元素。
        if (left < fs_priceRangeDataList.Count)
        {
            return fs_priceRangeDataList[left];
        }
        // 如果没有找到满足条件的元素，返回null。
        return null;
    }


    public static float GetCorrectPrice(float price,int round = 3)
    {
        HKStockPriceRange priceRange = GetPriceRange(price);
        if(priceRange != null)
        {
            double quotient = price /priceRange.priceDelta;

            if (quotient == (int)quotient)
            {
                return price;
            }
            else
            {
                int nextMultiple = (int)quotient + 1;
                double corrected = nextMultiple * priceRange.priceDelta;
                return (float)Math.Round(corrected,round);
            }
        }

        return price;
    }

    private static List<HKStockPriceRange> fs_priceRangeDataList = new List<HKStockPriceRange>()
    {
        new HKStockPriceRange(0.001f, 0.100f, 0.001f),
        new HKStockPriceRange(0.100f, 0.200f, 0.001f),
        new HKStockPriceRange(0.200f, 0.250f, 0.001f),
        new HKStockPriceRange(0.250f, 0.400f, 0.005f),
        new HKStockPriceRange(0.400f, 0.500f, 0.005f),
        new HKStockPriceRange(0.500f, 0.750f, 0.010f),
        new HKStockPriceRange(0.750f, 1.000f, 0.010f),
        new HKStockPriceRange(1.000f, 1.250f, 0.010f),
        new HKStockPriceRange(1.250f, 1.500f, 0.010f),
        new HKStockPriceRange(1.500f, 1.750f, 0.010f),
        new HKStockPriceRange(1.750f, 2.000f, 0.010f),
        new HKStockPriceRange(2.000f, 2.500f, 0.010f),
        new HKStockPriceRange(2.500f, 3.000f, 0.010f),
        new HKStockPriceRange(3.000f, 3.500f, 0.010f),
        new HKStockPriceRange(3.500f, 4.000f, 0.010f),
        new HKStockPriceRange(4.000f, 4.500f, 0.010f),
        new HKStockPriceRange(4.500f, 5.000f, 0.010f),
        new HKStockPriceRange(5.000f, 10.000f, 0.010f),
        new HKStockPriceRange(10.000f, 15.000f, 0.020f),
        new HKStockPriceRange(15.000f, 20.000f, 0.020f),
        new HKStockPriceRange(20.000f, 25.000f, 0.050f),
        new HKStockPriceRange(25.000f, 30.000f, 0.050f),
        new HKStockPriceRange(30.000f, 35.000f, 0.050f),
        new HKStockPriceRange(35.000f, 40.000f, 0.050f),
        new HKStockPriceRange(40.000f, 45.000f, 0.050f),
        new HKStockPriceRange(45.000f, 50.000f, 0.050f),
        new HKStockPriceRange(50.000f, 55.000f, 0.050f),
        new HKStockPriceRange(55.000f, 60.000f, 0.050f),
        new HKStockPriceRange(60.000f, 65.000f, 0.050f),
        new HKStockPriceRange(65.000f, 70.000f, 0.050f),
        new HKStockPriceRange(70.000f, 75.000f, 0.050f),
        new HKStockPriceRange(75.000f, 80.000f, 0.050f),
        new HKStockPriceRange(80.000f, 85.000f, 0.050f),
        new HKStockPriceRange(85.000f, 90.000f, 0.050f),
        new HKStockPriceRange(90.000f, 95.000f, 0.050f),
        new HKStockPriceRange(95.000f, 100.000f, 0.050f),
        new HKStockPriceRange(100.000f, 150.000f, 0.100f),
        new HKStockPriceRange(150.000f, 200.000f, 0.100f),
        new HKStockPriceRange(200.000f, 300.000f, 0.200f),
        new HKStockPriceRange(300.000f, 400.000f, 0.200f),
        new HKStockPriceRange(400.000f, 500.000f, 0.200f),
        new HKStockPriceRange(500.000f, 750.000f, 0.500f),
        new HKStockPriceRange(750.000f, 1000.000f, 0.500f),
        new HKStockPriceRange(1000.000f, 1500.000f, 1.000f),
        new HKStockPriceRange(1500.000f, 2000.000f, 1.000f),
        new HKStockPriceRange(2000.000f, 3000.000f, 2.000f),
        new HKStockPriceRange(3000.000f, 4000.000f, 2.000f),
        new HKStockPriceRange(4000.000f, 5000.000f, 2.000f),
        new HKStockPriceRange(5000.000f, 7500.000f, 5.000f),
        new HKStockPriceRange(7500.000f, 9995.000f, 5.000f),
};
}
