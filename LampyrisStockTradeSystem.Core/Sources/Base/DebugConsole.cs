namespace LampyrisStockTradeSystem;

public enum DebugConsoleLogLevel
{
    [NamedValue("[普通]")]
    Info = 1,

    [NamedValue("[警告]")]
    Warning = 2,

    [NamedValue("[错误]")]
    Error = 3
}

public class DebugConsoleLogData
{
    public DebugConsoleLogLevel level;
    public string time;
    public string message;

    private string m_string;

    public override string ToString()
    {
        if(m_string == null)
        {
            m_string ="[" + time + "]: " + EnumNameManager.GetName(level) + " " +message; 
        }
        return m_string;
    }
}

public class DebugConsole:Singleton<DebugConsole>
{
    private List<DebugConsoleLogData> m_debugConsoleLogDataList = new List<DebugConsoleLogData>();

    public List<DebugConsoleLogData> debugConsoleLogDatas => m_debugConsoleLogDataList;

    public void Log(string message, DebugConsoleLogLevel level = DebugConsoleLogLevel.Info)
    {
        m_debugConsoleLogDataList.Add(new DebugConsoleLogData()
        {
            message = message,
            level = level,
            time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss SSS")
        });
    }

    public void LogException(Exception ex)
    {
        m_debugConsoleLogDataList.Add(new DebugConsoleLogData()
        {
            message = ex.ToString(),
            level = DebugConsoleLogLevel.Error,
            time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss SSS")
        });
    }

    public void Clear()
    {
        m_debugConsoleLogDataList.Clear();
    }
}
