/*
** Author: wushuhong
** Contact: gameta@qq.com
** Description: 实时行情窗口,支持显示所有股票的实时行情数据
*  TODO: 加入更多的行情数据类型
*/
namespace LampyrisStockTradeSystem;

using ImGuiNET;

[UniqueWidget]
public class RealTimeQuoteWindow : Widget
{
    // 排序的列（0 = 第一列，1 = 第二列）
    private int m_sortByColumn = 0;

    // 排序的方向（true = 升序，false = 降序）
    private bool m_sortAscending = true;

    // 
    private QuoteDataView m_dataView;

    private List<QuoteDataView.QuoteDataViewField> m_quoteFields;

    private List<int> m_fieldIndexList = new List<int>();

    private void DoTableColunmnData(StockRealTimeQuoteData data,string content,bool isSelect)
    {
       
    }

    public override void OnAwake()
    {
        base.OnAwake();
        m_dataView = QuoteDatabase.Instance.CreateQuoteView();
        m_quoteFields = m_dataView.GetUseableQuoteField();

        m_fieldIndexList.Add(0);
        m_fieldIndexList.Add(1);
        m_fieldIndexList.Add(2);
        m_fieldIndexList.Add(3);
        m_fieldIndexList.Add(4);
        m_fieldIndexList.Add(5);

        m_dataView.SetRequiredFieldIndex(m_fieldIndexList);
    }

    public override unsafe void OnGUI()
    {
        ImGui.BeginTable("Stocks", m_fieldIndexList.Count + 1, ImGuiTableFlags.Sortable);
        {
            ImGui.TableSetupColumn("序号",ImGuiTableColumnFlags.DefaultSort | ImGuiTableColumnFlags.PreferSortAscending | ImGuiTableColumnFlags.WidthStretch);
            foreach(var fieldIndex in m_fieldIndexList)
            {
                ImGui.TableSetupColumn(m_quoteFields[fieldIndex].name, ImGuiTableColumnFlags.DefaultSort | ImGuiTableColumnFlags.PreferSortAscending);
            }

            // 获取当前排序设置
            ImGuiTableSortSpecsPtr sortSpecs = ImGui.TableGetSortSpecs();

            if (sortSpecs.NativePtr != null && sortSpecs.SpecsDirty)
            {
                // 这里记录 点击了一个列头后的 排序状态
                m_sortByColumn = sortSpecs.Specs.ColumnIndex;
                m_sortAscending = sortSpecs.Specs.SortDirection == ImGuiSortDirection.Ascending;
                sortSpecs.SpecsDirty = false;
            }

            ImGui.TableHeadersRow();

            for(int i = 0; i < m_dataView.rowCount; i++)
            {
                ImGui.TableNextRow();
                m_dataView.ProduceSingleRow(i);
                List<string> data = m_dataView.displayData;
                ImGui.TableNextColumn();
                ImGui.Text((i + 1).ToString());
                for (int j  = 0; j < data.Count;j++)
                {
                    ImGui.TableNextColumn();
                    ImGui.Text(data[j]);
                }
            }
        }
        ImGui.EndTable();
    }
}
