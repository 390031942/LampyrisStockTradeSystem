/*
** Author: wushuhong
** Contact: gameta@qq.com
** Description: 软件的功能注册类,分为两个类:
*  1) 快捷键功能,如按下F3打开上证指数,
*  2) 代码功能，比如在按键精灵输入61表示展示上证A股的涨幅排序
*/

namespace LampyrisStockTradeSystem;

public class AppFunctionSearchProvider : Singleton<AppFunctionSearchProvider>,ISearchableProvider
{
    private List<SearchResult> m_searchResult = new List<SearchResult>();

    public List<SearchResult> GetSearchResults(string code)
    { 
        m_searchResult.Clear();
        foreach (AppCodeFunctionInfo appCodeFunctionInfo in AppFunctionRegistry.codeFunctionInfos)
        {
            if (appCodeFunctionInfo.code.Contains(code))
            {
                SearchResult searchResult = new SearchResult
                {
                    type = SearchResultType.AppFunction,
                    code = appCodeFunctionInfo.code,
                    name = appCodeFunctionInfo.name,
                };
                m_searchResult.Add(searchResult);
            }
        }
        return m_searchResult;
    }
}
