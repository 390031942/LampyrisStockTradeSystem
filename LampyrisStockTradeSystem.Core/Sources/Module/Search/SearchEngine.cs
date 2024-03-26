
namespace LampyrisStockTradeSystem;

public interface ISearchableProvider
{
    public List<SearchResult> GetSearchResults(string code);
}

public enum SearchResultType
{
    Unknown = -1,
    AppFunction = 0,
    SZ_A = 1,
    SH_A = 2,
    BZ_A = 3,
    ChiNext = 5,
    Index = 6,
}

public class SearchResult
{
    public SearchResultType type;
    public string name;
    public string code;
}

public class SearchEngine : Singleton<SearchEngine>
{
    private List<SearchResult> m_searchResults = new List<SearchResult>();
    public List<SearchResult> searchResults => m_searchResults;

    public void ExecuteSearch(string searchString)
    {
        m_searchResults.Clear();
        m_searchResults.AddRange(QuoteDatabase.Instance.GetSearchResults(searchString));
        m_searchResults.AddRange(AppFunctionSearchProvider.Instance.GetSearchResults(searchString));
    }

    public string ParseSearchResultTypeName(SearchResultType type )
    {
        switch(type)
        {
            case SearchResultType.SZ_A:
                return "深A";
            case SearchResultType.SH_A:
                return "沪A";
            case SearchResultType.Index:
                return "指数";
            case SearchResultType.ChiNext:
                return "科创";
            case SearchResultType.AppFunction:
                return "功能";
        }

        return "未知";
    }
}
