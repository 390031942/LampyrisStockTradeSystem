/*
** Author: wushuhong
** Contact: gameta@qq.com
** Description: K线图辅助数据结构-线段树，解决了求解 给定区间内的最小最大价格及其对应的索引的问题
*/
namespace LampyrisStockTradeSystem;

public class QuotePriceSegmentTreeNode
{
    // 线段树左右端点
    public int startIndex;
    public int endIndex;

    // 最小/最大及其对应的索引
    public float maxValue;
    public int maxIndex;
    public float minValue;
    public int minIndex;

    public QuotePriceSegmentTreeNode left;
    public QuotePriceSegmentTreeNode right;

    public QuotePriceSegmentTreeNode(int startIndex, int endIndex)
    {
        this.startIndex = startIndex;
        this.endIndex = endIndex;
    }
}

public class QuotePriceSegmentTree
{
    private QuoteData m_quoteData;
    private QuotePriceSegmentTreeNode root;

    public QuotePriceSegmentTree(QuoteData quoteData)
    {
        if (quoteData == null)
            return;

        m_quoteData = quoteData;
        root = BuildSegmentTree(0, m_quoteData.perDayKLineList.Count - 1);
    }

    /// <summary>
    /// 递归构建线段树
    /// </summary>
    /// <param name="start">左端点索引</param>
    /// <param name="end">右端点索引</param>
    /// <returns></returns>
    private QuotePriceSegmentTreeNode BuildSegmentTree(int start, int end)
    {
        if (start > end)
            return null;

        // 递归边界条件时最大最小值分别取当天最高价和最低价 
        QuotePriceSegmentTreeNode node = new QuotePriceSegmentTreeNode(start, end);
        if (start == end)
        {
            node.maxValue = m_quoteData.perDayKLineList[start].highestPrice;
            node.maxIndex = start;
            node.minValue = m_quoteData.perDayKLineList[start].lowestPrice;
            node.minIndex = start;
            return node;
        }

        // 以中点为界限，构建左右侧区间的根节点
        int mid = (start + end) / 2;
        node.left = BuildSegmentTree(start, mid);
        node.right = BuildSegmentTree(mid + 1, end);

        node.maxValue = Math.Max(node.left.maxValue, node.right.maxValue);
        node.maxIndex = (node.left.maxValue >= node.right.maxValue) ? node.left.maxIndex : node.right.maxIndex;

        node.minValue = Math.Min(node.left.minValue, node.right.minValue);
        node.minIndex = (node.left.minValue <= node.right.minValue) ? node.left.minIndex : node.right.minIndex;

        return node;
    }

    public (float MaxValue, int MaxIndex) QueryMax(int left, int right)
    {
        return QueryHelperMax(root, left, right);
    }

    private (float MaxValue, int MaxIndex) QueryHelperMax(QuotePriceSegmentTreeNode node, int left, int right)
    {
        if (node.startIndex > right || node.endIndex < left)
            return (float.MaxValue, -1);

        if (node.startIndex >= left && node.endIndex <= right)
            return (node.maxValue, node.maxIndex);

        (float leftResult, int leftIndex) = QueryHelperMax(node.left, left, right);
        (float rightResult, int rightIndex) = QueryHelperMax(node.right, left, right);

        float maxValue = Math.Max(leftResult, rightResult);
        int maxIndex = (leftResult >= rightResult) ? leftIndex : rightIndex;

        return (maxValue, maxIndex);
    }

    public (float MinValue, int MinIndex) QueryMin(int left, int right)
    {
        return QueryHelperMin(root, left, right);
    }

    private (float MinValue, int MinIndex) QueryHelperMin(QuotePriceSegmentTreeNode node, int left, int right)
    {
        if (node.startIndex > right || node.endIndex < left)
            return (float.MinValue, -1);

        if (node.startIndex >= left && node.endIndex <= right)
            return (node.minValue, node.minIndex);

        (float leftResult, int leftIndex) = QueryHelperMin(node.left, left, right);
        (float rightResult, int rightIndex) = QueryHelperMin(node.right, left, right);

        float minValue = Math.Min(leftResult, rightResult);
        int minIndex = (leftResult <= rightResult) ? leftIndex : rightIndex;

        return (minValue, minIndex);
    }
}