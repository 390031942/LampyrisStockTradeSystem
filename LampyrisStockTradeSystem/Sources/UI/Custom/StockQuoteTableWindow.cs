/*
** Author: wushuhong
** Contact: gameta@qq.com
** Description: 股票实时行情表格窗口(占坑)
*  TODO: 加入更多的行情数据类型
*/
namespace LampyrisStockTradeSystem;

using ImGuiNET;
using Newtonsoft.Json.Linq;
using System.Text.Json;

[UniqueWidget]
public class StockQuoteTableWindow : Widget
{
    // 排序的列（0 = 第一列，1 = 第二列）
    private int m_sortByColumn = 0;

    // 排序的方向（true = 升序，false = 降序）
    private bool m_sortAscending = true;

    // 选中的股票数据
    private StockRealTimeQuoteData m_selectedData;

    // 股票数据列表
    private List<StockRealTimeQuoteData> m_stockKLineDatas = new List<StockRealTimeQuoteData>();

    // 股票数据列表中元素个数
    private int m_stockKLineDataCount = 0;

    private void DoTableColunmnData(StockRealTimeQuoteData data,string content,bool isSelect)
    {
        ImGui.TableNextColumn();
        if (ImGui.Selectable(content, isSelect))
        {
            m_selectedData = data;
            if (ImGui.IsMouseDoubleClicked(0))
            {
            }
        }
    }

    public override unsafe void OnGUI()
    {
        ImGui.BeginTable("Stocks",5, ImGuiTableFlags.Sortable);
        {
            ImGui.TableSetupColumn("代码",ImGuiTableColumnFlags.DefaultSort | ImGuiTableColumnFlags.PreferSortAscending | ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn("名称", ImGuiTableColumnFlags.WidthStretch|ImGuiTableColumnFlags.PreferSortDescending);
            ImGui.TableSetupColumn("价格", ImGuiTableColumnFlags.WidthStretch | ImGuiTableColumnFlags.PreferSortDescending);
            ImGui.TableSetupColumn("涨跌幅", ImGuiTableColumnFlags.WidthStretch | ImGuiTableColumnFlags.PreferSortDescending);
            ImGui.TableSetupColumn("涨跌额", ImGuiTableColumnFlags.WidthStretch | ImGuiTableColumnFlags.PreferSortDescending);

            // 获取当前排序设置
            ImGuiTableSortSpecsPtr sortSpecs = ImGui.TableGetSortSpecs();

            if (sortSpecs.NativePtr != null && sortSpecs.SpecsDirty)
            {
                // 这里记录 点击了一个列头后的 排序状态
                m_sortByColumn = sortSpecs.Specs.ColumnIndex;
                m_sortAscending = sortSpecs.Specs.SortDirection == ImGuiSortDirection.Ascending;
                sortSpecs.SpecsDirty = false;

                m_stockKLineDatas.Sort((a, b) => {
                    // 比较函数根据sortByColumn和sortAscending变量对股票数据进行排序
                    if (m_sortByColumn == 0)
                    {
                        return m_sortAscending ? a.code.CompareTo(b.code) : b.code.CompareTo(a.code);
                    }
                    else if (m_sortByColumn == 1)
                    {
                        return m_sortAscending ? a.name.CompareTo(b.name) : b.name.CompareTo(a.name);
                    }
                    else if (m_sortByColumn == 2)
                    {
                        int value = m_sortAscending ? a.closePrice.CompareTo(b.closePrice) : b.closePrice.CompareTo(a.closePrice);
                        return value != 0 ? value : m_sortAscending ? a.code.CompareTo(b.code) : b.code.CompareTo(a.code);
                    }
                    else if (m_sortByColumn == 3)
                    {
                        int value = m_sortAscending ? a.percentage.CompareTo(b.percentage) : b.percentage.CompareTo(a.percentage);
                        return value != 0 ? value : m_sortAscending ? a.code.CompareTo(b.code) : b.code.CompareTo(a.code);
                    }
                    else
                    {
                        int value = m_sortAscending ? a.priceChange.CompareTo(b.priceChange) : b.priceChange.CompareTo(a.priceChange);
                        return value != 0 ? value : m_sortAscending ? a.code.CompareTo(b.code) : b.code.CompareTo(a.code);
                    }
                });
            }

            ImGui.TableHeadersRow();

            lock(this.m_stockKLineDatas)
            {
                if (this.m_stockKLineDatas != null)
                {
                    for (int i = 0; i < m_stockKLineDataCount; i++)
                    {
                        ImGui.TableNextRow();
                        var data = m_stockKLineDatas[i];

                        bool selected = m_selectedData == data;

                        DoTableColunmnData(data, data.code, selected);
                        DoTableColunmnData(data, data.name, selected);
                        DoTableColunmnData(data, data.closePrice.ToString(), selected);
                        DoTableColunmnData(data, data.percentage.ToString() + "%", selected);
                        DoTableColunmnData(data, data.priceChange.ToString(), selected);
                    }
                }
            }
        }
        ImGui.EndTable();
    }

    public void SetStockData(JArray? stockDataArray)
    {
        m_stockKLineDataCount = 0;
        if (stockDataArray != null)
        {
            for (int i = 0; i < stockDataArray.Count; i++)
            {
                JObject stockObject = stockDataArray[i].ToObject<JObject>();

                if (stockObject != null)
                {
                    try
                    {
                        string name = stockObject["f14"].ToString();
                        float price = stockObject["f2"].ToObject<float>();
                        float percentage = stockObject["f3"].ToObject<float>();
                        float priceChange = stockObject["f4"].ToObject<float>();
                        string code = stockObject["f12"].ToString();

                        StockRealTimeQuoteData stockKLineData = null;

                        // 这里重复利用了List里面的StockRealTimeQuoteData，避免重新内存分配
                        if (i < m_stockKLineDatas.Count)
                        {
                            stockKLineData = m_stockKLineDatas[i];
                        }
                        else // 如果List里的数量比实际需要的要小，则进行内存分配
                        {
                            stockKLineData = new StockRealTimeQuoteData();
                            m_stockKLineDatas.Add(stockKLineData);
                        }

                        stockKLineData.code = code;
                        stockKLineData.name = name;
                        stockKLineData.closePrice = price;
                        stockKLineData.percentage = percentage;
                        stockKLineData.priceChange = priceChange;

                        // 这里记录股票的实际数量，以便遍历的时候用
                        m_stockKLineDataCount++;
                    }
                    catch (Exception e)
                    {

                    }
                }
            }

            m_stockKLineDatas.Sort((a, b) =>
            {
                return m_sortAscending ? a.code.CompareTo(b.code) : b.code.CompareTo(a.code);
            });
        }
    }
}
