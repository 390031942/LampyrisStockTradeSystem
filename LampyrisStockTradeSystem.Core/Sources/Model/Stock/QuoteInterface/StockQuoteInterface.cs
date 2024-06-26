﻿namespace LampyrisStockTradeSystem;

public class StockQuoteInterface:Singleton<StockQuoteInterface>
{
    private bool m_inited = false;

    private Dictionary<StockQuoteInterfaceType, IStockQuoteInterface> m_stockInterfaceDict = new Dictionary<StockQuoteInterfaceType, IStockQuoteInterface>();

    private void Init()
    {
        if(!m_inited)
        {
            m_stockInterfaceDict[StockQuoteInterfaceType.KLineData] = new StockKLineDataExtractor();
            m_stockInterfaceDict[StockQuoteInterfaceType.IntradayTradingDetail] = new IntradayTradingDetailExtractor();
            m_stockInterfaceDict[StockQuoteInterfaceType.CurrentQuotes] = new CurrentRealtimeQuoteExtractor();
            m_stockInterfaceDict[StockQuoteInterfaceType.HKLink] = new HKLinkQuoteExtractor();
            m_stockInterfaceDict[StockQuoteInterfaceType.GlobalIndexBrief] = new GlobalIndexBriefQuoteExtractor();

            m_inited = true;
        }
    }

    public string GetQuoteUrl(StockQuoteInterfaceType stockQuoteType,params string[] parameters)
    {
        Init();
        return m_stockInterfaceDict[stockQuoteType].MakeUrl(parameters);
    }
}
