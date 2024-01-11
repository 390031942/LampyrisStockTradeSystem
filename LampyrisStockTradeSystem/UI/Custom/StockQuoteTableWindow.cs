/*
** Author: wushuhong
** Contact: gameta@qq.com
** Description: 股票实时行情表格窗口(占坑)
*  TODO: 加入更多的行情数据类型
*/
namespace LampyrisStockTradeSystem;

using ImGuiNET;
using Newtonsoft.Json.Linq;

[UniqueWidget]
public class StockQuoteTableWindow : Widget
{
    // 排序的列（0 = 第一列，1 = 第二列）
    int sortByColumn = 0;

    // 排序的方向（true = 升序，false = 降序）
    bool sortAscending = true;

    // 选中的股票数据
    private StockRealTimeQuoteData selectedData;

    // 股票数据列表
    private List<StockRealTimeQuoteData> stockKLineDatas = new List<StockRealTimeQuoteData>();

    private void DoTableColunmnData(StockRealTimeQuoteData data,string content,bool isSelect)
    {
        ImGui.TableNextColumn();
        if (ImGui.Selectable(content, isSelect))
        {
            selectedData = data;
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
            ImGui.TableSetupColumn("名称", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn("价格", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn("涨跌幅", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn("涨跌额", ImGuiTableColumnFlags.WidthStretch);

            // 获取当前排序设置
            ImGuiTableSortSpecsPtr sortSpecs = ImGui.TableGetSortSpecs();

            if (sortSpecs.NativePtr != null && sortSpecs.SpecsDirty)
            {
                // 这里记录 点击了一个列头后的 排序状态
                sortByColumn = sortSpecs.Specs.ColumnIndex;
                sortAscending = sortSpecs.Specs.SortDirection == ImGuiSortDirection.Ascending;
                sortSpecs.SpecsDirty = false;

                stockKLineDatas.Sort((a, b) => {
                    // 比较函数根据sortByColumn和sortAscending变量对股票数据进行排序
                    if (sortByColumn == 0)
                    {
                        return sortAscending ? a.code.CompareTo(b.code) : b.code.CompareTo(a.code);
                    }
                    else if (sortByColumn == 1)
                    {
                        return sortAscending ? a.name.CompareTo(b.name) : b.name.CompareTo(a.name);
                    }
                    else if (sortByColumn == 2)
                    {
                        int value = sortAscending ? a.closePrice.CompareTo(b.closePrice) : b.closePrice.CompareTo(a.closePrice);
                        return value != 0 ? value : sortAscending ? a.code.CompareTo(b.code) : b.code.CompareTo(a.code);
                    }
                    else if (sortByColumn == 3)
                    {
                        int value = sortAscending ? a.percentage.CompareTo(b.percentage) : b.percentage.CompareTo(a.percentage);
                        return value != 0 ? value : sortAscending ? a.code.CompareTo(b.code) : b.code.CompareTo(a.code);
                    }
                    else
                    {
                        int value = sortAscending ? a.priceChange.CompareTo(b.priceChange) : b.priceChange.CompareTo(a.priceChange);
                        return value != 0 ? value : sortAscending ? a.code.CompareTo(b.code) : b.code.CompareTo(a.code);
                    }
                });
            }

            ImGui.TableHeadersRow();

            if(this.stockKLineDatas != null)
            {
                for(int i = 0;i < this.stockKLineDatas.Count;i++)
                {
                    ImGui.TableNextRow();
                    var data = stockKLineDatas[i];
                 
                    bool selected = selectedData == data;

                    DoTableColunmnData(data, data.code, selected);
                    DoTableColunmnData(data, data.name, selected);
                    DoTableColunmnData(data, data.closePrice.ToString(), selected);
                    DoTableColunmnData(data, data.percentage.ToString() + "%", selected);
                    DoTableColunmnData(data, data.priceChange.ToString(), selected);
                }
            }
           
        }
        ImGui.EndTable();
    }

    public void SetStockData(JArray? stockDataArray)
    {
        stockKLineDatas.Clear();

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

                        StockRealTimeQuoteData stockKLineData = new StockRealTimeQuoteData();
                        stockKLineData.code = code;
                        stockKLineData.name = name;
                        stockKLineData.closePrice = price;
                        stockKLineData.percentage = percentage;
                        stockKLineData.priceChange = priceChange;

                        stockKLineDatas.Add(stockKLineData);
                    }
                    catch (Exception e)
                    {

                    }
                }
            }

            stockKLineDatas.Sort((a, b) =>
            {
                return sortAscending ? a.code.CompareTo(b.code) : b.code.CompareTo(a.code);
            });
        }
    }
}
