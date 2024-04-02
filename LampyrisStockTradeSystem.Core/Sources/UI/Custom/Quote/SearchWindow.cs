    /*
** Author: wushuhong
** Contact: gameta@qq.com
** Description: 主界面右下角的股票搜索窗口
*/
namespace LampyrisStockTradeSystem;

using ImGuiNET;
using System;
using System.Numerics;

[UniqueWidget]
public class SearchWindow : Widget
{
    // 搜索股票代码的字符串
    private string m_searchString = "123123";
    private string m_searchStringCached = null;

    // 表示延迟帧的数量，默认会被设置为4，等于0
    // 由于IMGUI的SetKeyboardFocusHere会使得文本处于选中状态，所以这里必须做一个特殊处理
    // 比如我想输入"600000",当输入"6"的时候，搜索窗口弹出，InputText的显示的文本是"6"，虽然InputText被设置了焦点，但是"6"被选中了
    // 此时在输入第二个字符"0"的时候，之前输入的"6"就被"0"覆盖了，所以这里做的处理的是：
    // 
    private int m_leftFrame = 0;

    // 选中的索引
    private int m_selectedIndex = -1;

    private bool m_needFocus = false;

    public override string Name => "Lampyris 键盘精灵";

    public override WidgetModel widgetModel => WidgetModel.Normal;

    // 填充表格的一行数据,参考同花顺键盘精灵的格式： 功能/证券/板块代码  |   名称   |  类型
    private string[] m_dataRowArray = new string[3];

    // 初次打开窗口时候，一定要进行搜索
    private bool m_hasSearch = false;

    // 这个窗口
    public override ImGuiWindowFlags windowFlags => ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse;

    public override void OnAwake()
    {
        pos = new Vector2(ImGui.GetIO().DisplaySize.X - 300, ImGui.GetIO().DisplaySize.Y - 350);
        size = new Vector2(300, 350);

        RuntimeContext.mainWindow.BeforeRenderFrame += this.OnBeforeRenderFrame;
    }

    private void OnBeforeRenderFrame()
    {
        if (m_leftFrame != -1)
        {
            m_leftFrame--;
            if(m_leftFrame == 0)
            {
                foreach (char c in m_searchStringCached)
                {
                    RuntimeContext.mainWindow.AddInputChar(c);
                }
                m_leftFrame = -1;
            }
        }
    }

    public override void OnGUI()                                               
    {
        if (m_needFocus)
        {
            ImGui.SetKeyboardFocusHere();
            m_needFocus = false;
        }
        string oldSearchString = m_searchString;
        bool changed = ImGui.InputText("##StockCodeSearch", ref m_searchString, 100);
        if (changed || !m_hasSearch)
        {
            SearchEngine.Instance.ExecuteSearch(m_searchString);
            m_hasSearch = true;
        }

        // 创建表格
        if (ImGui.BeginTable("##StockCodeSearchResultTable", 3, ImGuiTableFlags.NoSavedSettings | ImGuiTableFlags.RowBg | ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.ScrollY))
        {
            ImGui.TableSetupColumn("##StockCodeSearchResultTable_Col1", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn("##StockCodeSearchResultTable_Col2", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn("##StockCodeSearchResultTable_Col3", ImGuiTableColumnFlags.WidthStretch);

            for(int i = 0; i < SearchEngine.Instance.searchResults.Count;i++)
            {
                SearchResult searchResult = SearchEngine.Instance.searchResults[i];
                m_dataRowArray[0] = searchResult.code;
                m_dataRowArray[1] = searchResult.name;
                m_dataRowArray[2] = SearchEngine.Instance.ParseSearchResultTypeName(searchResult.type);

                ImGui.TableNextRow();
                for (int j = 0; j < m_dataRowArray.Length; j++)
                {
                    ImGui.TableSetColumnIndex(j);
                    if (m_selectedIndex == i)
                    {
                        ImGui.TextColored(new Vector4(1,0,0,1), m_dataRowArray[j]);
                        ImGui.SetScrollHereY(1.0f);
                    }
                    else
                    {
                        ImGui.Text(m_dataRowArray[j]);
                    }

                    // 如果点击了某个项，那么也要选中
                    if (ImGui.IsItemClicked())
                    {
                        m_selectedIndex = i;
                        // isOpened = false;
                    }
                }
            }

            ImGui.EndTable();
        }

        if (ImGui.IsKeyPressed(ImGui.GetKeyIndex(ImGuiKey.Enter)))
        {
            // Enter键被按下，就要关闭搜索窗口了
            isOpened = false;
            KLineWindow window = WidgetManagement.GetWidget<KLineWindow>();
            window.ShowQuoteByCode(m_dataRowArray[0]);
        }
        else if(ImGui.IsKeyPressed(ImGui.GetKeyIndex(ImGuiKey.UpArrow))) 
        {
            m_selectedIndex = m_selectedIndex - 1 >= 0 ? m_selectedIndex - 1 : 0;
        }
        else if (ImGui.IsKeyPressed(ImGui.GetKeyIndex(ImGuiKey.DownArrow)))
        {
            m_selectedIndex = m_selectedIndex + 1 < SearchEngine.Instance.searchResults.Count ? m_selectedIndex  + 1: m_selectedIndex;
        }
    }

    // 设置输入的文本并让焦点设置在搜索代码的输入框
    public void SetInputCodeAndFoucs(string inputString)
    {
        m_searchString = m_searchStringCached = inputString;
        m_needFocus = true;

        // 设置一个延迟帧
        m_leftFrame = 4;
    }

    // 主窗口大小改变时本窗口右下角位置 会恒等于 主窗口的右下角位置
    public override void OnMainWindowResize(int width, int height)
    {
        base.OnMainWindowResize(width, height);
        pos = new Vector2(width - 300, height - 350);
    }

    public override void OnAfterGUI()
    {
        base.OnAfterGUI();
        if (!ImGui.IsWindowFocused(ImGuiFocusedFlags.RootAndChildWindows))
        {
            isOpened = false;
        }
    }

    [MenuItem("窗口/搜索")]
    public static void ShowSearch()
    {
        WidgetManagement.GetWidget<SearchWindow>();
    }
}
