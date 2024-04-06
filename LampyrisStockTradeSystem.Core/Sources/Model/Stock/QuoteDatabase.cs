/*
** Author: wushuhong
** Contact: gameta@qq.com
** Description: 行情数据库，内部实现了实时行情的获取与持久化，提供了外部访问股票数据的接口
*/

using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace LampyrisStockTradeSystem;

public class QuoteFieldInfo
{
    public int index;   
}

/// <summary>
/// 行情视图类，对于展示了一个行情列表的UI，其行情数据需通过"行情视图"来获取。
/// 其功能在于 负责维护一个行情列表，支持对其数据进行排序，并支持按需获取相应的行情字段。
/// 如 展示"沪深京排行"行情时，只需要创建一个行情视图，点击行情列表头部表格时，可以对某一列字段进行排序。
/// 此外，展示"沪深京排行"行情时，往往需要比较多的字段，并且用户可能会希望 增加/减少 这些行情的字段，行情视图提供了实现功能的接口。
/// 再比如，行情软件中的全景图视角,可能涉及"沪A5分钟涨幅","深A5分钟跌幅"等行情视角，创建多个行情视图可以很方便的展示这些数据。
/// </summary>
public abstract class QuoteDataView
{
    public enum QuoteDataViewFieldColorStyle
    {
        // 固定某种颜色
        Fixed,

        // 大于/小于/等于 0 时有不同的颜色
        Signed,

        // 价格类型，要和昨日收盘价进行对比
        Price,

        // 金额类型
        Money,
    }

    public class QuoteDataViewField
    {
        // 字段名称,比如"换手率","每股收益(元)"
        public string name;
        
        // 字段的路径，比如"基本行情/换手率","财务数据/每股收益(元)",如果为空，则意味着这个字段常驻
        public string path = "";

        // 是否可以点击表头排序
        public bool isSortable = true;

        // 字段颜色显示风格，比如股票名称永远显示白色，而涨幅是 涨红，跌绿，平白
        public QuoteDataViewFieldColorStyle colorStyle = QuoteDataViewFieldColorStyle.Fixed;

        public Func<QuoteData,string> getterFunc;
    }

    protected List<QuoteData> m_quoteData = new List<QuoteData>();

    protected List<string> m_displayData = new List<string>();

    public List<string> displayData => m_displayData;

    public int rowCount => m_quoteData.Count;

    protected List<int> m_requiredFieldIndex = new List<int>();

    public void Activate(List<StockQuoteData> values)
    {
        foreach (var item in values)
        {
            m_quoteData.Add(item);
        }
    }

    public void Deactivate()
    {

    }

    public void SetRequiredFieldIndex(List<int> requiredFieldIndex)
    {
        m_requiredFieldIndex = requiredFieldIndex;
    }

    public void SetSortingIndex(int index, bool ascending = true)
    {

    }

    public abstract List<QuoteDataViewField> GetUseableQuoteField();

    public void ProduceSingleRow(int row)
    {
        m_displayData.Clear();
        QuoteData quoteData = m_quoteData[row];
        List<QuoteDataViewField> quoteDataViewFields = GetUseableQuoteField();

        foreach (int fieldInddex in m_requiredFieldIndex)
        {
            QuoteDataViewField quoteDataViewField = quoteDataViewFields[fieldInddex];
            m_displayData.Add(quoteDataViewField.getterFunc(quoteData));
        }
    }
}

public class StockDataView : QuoteDataView
{
    private List<QuoteDataViewField> m_useableQuoteField;

    public override List<QuoteDataViewField> GetUseableQuoteField()
    {
        if (m_useableQuoteField == null)
        {
            m_useableQuoteField = new List<QuoteDataViewField>()
        {
            // 0
            new QuoteDataViewField()
            {
                name = "代码",
                getterFunc = (QuoteData quote) =>
                {
                    return quote.code;
                }
            },
             // 1
            new QuoteDataViewField()
            {
                name ="名称",
                getterFunc = (QuoteData quote) =>
                {
                    return quote.name;
                }
            },
            // 2
            new QuoteDataViewField()
            {
                name ="最新",
                colorStyle = QuoteDataViewFieldColorStyle.Price,
                getterFunc = (QuoteData quote) =>
                {
                    return quote.realTimeQuoteData.kLineData.closePrice.ToString();
                }
            },
            // 3
            new QuoteDataViewField()
            {
                name ="涨幅%",
                colorStyle = QuoteDataViewFieldColorStyle.Signed,
                getterFunc =(QuoteData quote) =>
                {
                    return quote.realTimeQuoteData.kLineData.percentage.ToString() + "%";
                }
            },
            // 4
            new QuoteDataViewField()
            {
                name ="涨跌",
                colorStyle = QuoteDataViewFieldColorStyle.Signed,
                getterFunc =(QuoteData quote) =>
                {
                    return quote.realTimeQuoteData.kLineData.priceChange.ToString() + "%";
                }
            },
            // 5 
            new QuoteDataViewField()
            {
                name ="总量",
                colorStyle = QuoteDataViewFieldColorStyle.Signed,
                getterFunc =(QuoteData quote) =>
                {
                    return quote.realTimeQuoteData.kLineData.volume.ToString();
                }
            },
            // 6
            new QuoteDataViewField()
            {
                name ="现量",
                getterFunc =(QuoteData quote) =>
                {
                    var transactionDetailDataList = quote.realTimeQuoteData.transactionDetailDataList;
                    if(transactionDetailDataList.Count > 0)
                    {
                        TransactionDetailData transactionDetailData = transactionDetailDataList[transactionDetailDataList.Count - 1];
                        if(transactionDetailData.type == TransactionType.Buy)
                            return transactionDetailData.count + "B";
                        else if(transactionDetailData.type == TransactionType.Sell)
                            return transactionDetailData.count + "S";
                    }

                    return "-";
                }
            },
            // 7
            new QuoteDataViewField()
            {
                name ="买入价" ,
                colorStyle = QuoteDataViewFieldColorStyle.Price,
                getterFunc = HandleAskBidField
            },
            // 8
            new QuoteDataViewField()
            {
                name ="卖出价" ,
                colorStyle = QuoteDataViewFieldColorStyle.Price,
                getterFunc = HandleAskBidField
            },
            // 9
            new QuoteDataViewField()
            {
                name ="涨速%",
                colorStyle = QuoteDataViewFieldColorStyle.Signed,
                getterFunc =(QuoteData quote) =>
                {
                    //@TODO:
                    return "0.0%";
                }
            },
            // 10
            new QuoteDataViewField()
            {
                name ="换手%",
                getterFunc =(QuoteData quote) =>
                {
                    return quote.realTimeQuoteData.kLineData.turnOverRate + "%";
                }
            },
            // 11
            new QuoteDataViewField()
            {
                name ="金额",
                getterFunc =(QuoteData quote) =>
                {
                     return quote.realTimeQuoteData.kLineData.money.ToString();
                }
            },
            // 12
            new QuoteDataViewField()
            {
                name ="市盈率(动)" ,
                getterFunc =(QuoteData quote) =>
                {
                    return ((StockQuoteData)quote).PERatioDynamic.ToString();
                }
            },
            // 13
            new QuoteDataViewField()
            {
                name ="所属行业",
                getterFunc =(QuoteData quote) =>
                {
                    return ((StockQuoteData)quote).sectorName;
                }
            },
            // 14
            new QuoteDataViewField()
            {
                name ="最高" ,
                colorStyle = QuoteDataViewFieldColorStyle.Price,
                getterFunc =(QuoteData quote) =>
                {
                    return quote.realTimeQuoteData.kLineData.highestPrice.ToString();
                }
            },
            // 15
            new QuoteDataViewField()
            {
                name ="最低" ,
                colorStyle = QuoteDataViewFieldColorStyle.Price,
                getterFunc =(QuoteData quote) =>
                {
                    return quote.realTimeQuoteData.kLineData.lowestPrice.ToString();
                }
            },
            // 16
            new QuoteDataViewField()
            {
                name ="开盘",
                colorStyle = QuoteDataViewFieldColorStyle.Price,
                getterFunc =(QuoteData quote) =>
                {
                    return quote.realTimeQuoteData.kLineData.openPrice.ToString();
                }
            },
            // 17
            new QuoteDataViewField()
            {
                name ="昨收",
                getterFunc =(QuoteData quote) =>
                {
                    var perDayKLineList = quote.perDayKLineList;
                    if(perDayKLineList.Count > 0)
                        return perDayKLineList[perDayKLineList.Count - 1].closePrice.ToString();
                    else
                        return "-";
                }
            },
            // 18
            new QuoteDataViewField()
            {
                name ="振幅%" ,
                getterFunc =(QuoteData quote) =>
                {
                    return quote.realTimeQuoteData.kLineData.amplitude +  "%";
                }
            },
            // 19
            new QuoteDataViewField()
            {
                name ="量比",
                getterFunc =(QuoteData quote) =>
                {
                    StockQuoteData stockQuote = ((StockQuoteData)quote);
                    StockRealTimeQuoteData realTimeQuoteData = (StockRealTimeQuoteData)stockQuote.realTimeQuoteData;
                    return realTimeQuoteData.volumnRate.ToString();
                }
            },
            // 20
            new QuoteDataViewField()
            {
                name ="委比%",
                colorStyle = QuoteDataViewFieldColorStyle.Signed,
                getterFunc =(QuoteData quote) =>
                {
                    StockQuoteData stockQuote = ((StockQuoteData)quote);
                    StockRealTimeQuoteData realTimeQuoteData = (StockRealTimeQuoteData)stockQuote.realTimeQuoteData;
                    return realTimeQuoteData.bidAskData.theCommittee + "%";
                }
            },
            // 21
            new QuoteDataViewField()
            {
                name ="内盘",
                getterFunc =(QuoteData quote) =>
                {
                    StockQuoteData stockQuote = ((StockQuoteData)quote);
                    StockRealTimeQuoteData realTimeQuoteData = (StockRealTimeQuoteData)stockQuote.realTimeQuoteData;
                    return realTimeQuoteData.transactionSumData.buyCount.ToString();
                }
            }, 
            // 22
            new QuoteDataViewField()
            {
                name ="外盘",
                getterFunc =(QuoteData quote) =>
                {
                    StockQuoteData stockQuote = ((StockQuoteData)quote);
                    StockRealTimeQuoteData realTimeQuoteData = (StockRealTimeQuoteData)stockQuote.realTimeQuoteData;
                    return realTimeQuoteData.transactionSumData.sellCount.ToString();
                }
            },
            // 23
            new QuoteDataViewField()
            {
                name = "内外比",
                getterFunc =(QuoteData quote) =>
                {
                    StockQuoteData stockQuote = ((StockQuoteData)quote);
                    StockRealTimeQuoteData realTimeQuoteData = (StockRealTimeQuoteData)stockQuote.realTimeQuoteData;

                    float buyCount = realTimeQuoteData.transactionSumData.buyCount;
                    float sellCount = realTimeQuoteData.transactionSumData.sellCount;

                    return buyCount + ":" + sellCount;
                }
            }
        };
        }
        return m_useableQuoteField;
    }

    protected static string HandleAskBidField(QuoteData quote)
    {
        StockQuoteData stockQuote = ((StockQuoteData)quote);
        StockRealTimeQuoteData realTimeQuoteData = (StockRealTimeQuoteData)stockQuote.realTimeQuoteData;
        AskBidData askBidData = realTimeQuoteData.bidAskData.bid[1];
        if (askBidData.price > 0.0f)
            return askBidData.price.ToString();
        else
            return "-";
    }
}

[Serializable]
public class QuoteDatabase : SerializableSingleton<QuoteDatabase>, IPostSerializationHandler, ISearchableProvider
{
    // 股票代码->股票数据字典
    private Dictionary<string, StockQuoteData> m_stockCode2DataDict = new Dictionary<string, StockQuoteData>();

    // 指数简明行情的数据字典
    private Dictionary<string, IndexBriefQuoteData> m_code2BriefQuoteDataMap = new Dictionary<string, IndexBriefQuoteData>();

    private List<SerializableKeyValuePair<string, StockQuoteData>> m_stockCode2DataList = new List<SerializableKeyValuePair<string, StockQuoteData>>();

    /// <summary>
    /// 根据股票代码获取数据
    /// </summary>
    /// <param name="stockCode"></param>
    /// <returns></returns>
    public List<KLineData>? GetStockData(string stockCode)
    {
        if (m_stockCode2DataDict.ContainsKey(stockCode))
        {
            return m_stockCode2DataDict[stockCode].perDayKLineList;
        }
        return null;
    }

    /// <summary>
    /// 获取所有股票的实时行情列表
    /// </summary>
    // [PlannedTask(mode: PlannedTaskExecuteMode.ExecuteOnlyOnTime | PlannedTaskExecuteMode.ExecuteAfterTime | PlannedTaskExecuteMode.ExecuteOnLaunch, executeTime = "9:10")]
    public static void GetAllStockData()
    {
        // URL请求返回的Json格式
        //
        // { "f1":2, // 固有字段，无实际意义
        //   "f2":3.17, // 现价
        //   "f3":10.07, // 涨幅
        //   "f10": // 量比
        //   "f12":"002467", // 名称
        //   "f13":0, // 市场编号,0表示深证,1表示上证
        //   "f14":"二六三", // 证券名称
        //   "f22":0.0, // 涨速
        //   "f31":0.0, // 买入价
        //   "f32":0.0, // 卖出价
        //   "f33",委比
        //   f34外盘，f35内盘
        //   "f62":29691216.0, // 主力净流入净额
        //   "f66":30835868.0, // 超大单流入净额
        //   "f69":56.63, // 超大单净流入净占比
        //   "f72":-1144652.0, // 大单净流入净额
        //   "f75":-2.1,  // 大单净流入净占比
        //   "f78":-8464367.0, // 中单净流入净额
        //   "f81":-15.54, // 中单净流入净占比
        //   "f84":-21226848.0, // 小单净流入净额
        //   "f87":-38.98,  // 小单净流入净占比
        //   "f105":"-", // 固有字段，无实际意义
        //   "f116":"-", // 固有字段，无实际意义
        //   "f117":"-", // 固有字段，无实际意义
        //   "f124":1707377655, // 更新时间戳
        //   "f127":10.84, // 3日涨幅
        //   "f184":54.52,// 增仓占比
        //   "f204":"-",
        //   "f205":"-",
        //   "f206":"-"
        //   f211,买一量 ,f212 卖一量
        //   }

        HttpRequest.Get(StockQuoteInterface.Instance.GetQuoteUrl(StockQuoteInterfaceType.CurrentQuotes), (json) =>
        {
            string strippedJson = JsonStripperUtil.GetEastMoneyStrippedJson(json);
            try
            {
                JObject jsonRoot = JObject.Parse(strippedJson);

                JArray stockDataArray = jsonRoot?["data"]?["diff"]?.ToObject<JArray>();
                if (stockDataArray != null)
                {
                    for (int i = 0; i < stockDataArray.Count; i++)
                    {
                        JObject stockObject = stockDataArray[i].ToObject<JObject>();

                        if (stockObject != null)
                        {
                            // 这里获取股票代码和名称
                            string name = stockObject["f14"]?.ToString();
                            string code = stockObject["f12"]?.ToString();

                            if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(code))
                            {
                                StockQuoteData stockData = null;
                                if (!Instance.m_stockCode2DataDict.ContainsKey(code))
                                {
                                    stockData = Instance.m_stockCode2DataDict[code] = new StockQuoteData()
                                    {
                                        code = code,
                                        name = name,
                                        abbreviation = ChineseUtil.ToPinYin(name,true),
                                    };
                                }
                                else
                                {
                                    stockData = Instance.m_stockCode2DataDict[code];
                                }

                                // 更新股票的实时行情
                                StockRealTimeQuoteData realTimeQuoteData = (StockRealTimeQuoteData)stockData.realTimeQuoteData;
                                realTimeQuoteData.kLineData.closePrice = stockObject["f2"].SafeToObject<float>(); // 现价
                                realTimeQuoteData.kLineData.percentage = stockObject["f3"].SafeToObject<float>(); // 涨幅
                                realTimeQuoteData.kLineData.priceChange = stockObject["f4"].SafeToObject<float>(); // 涨跌额
                                realTimeQuoteData.kLineData.volume = stockObject["f5"].SafeToObject<float>(); // 成交量
                                realTimeQuoteData.kLineData.money = stockObject["f6"].SafeToObject<float>(); // 成交额
                                realTimeQuoteData.kLineData.turnOverRate = stockObject["f8"].SafeToObject<float>(); // 换手率
                                realTimeQuoteData.kLineData.highestPrice = stockObject["f15"].SafeToObject<float>(); // 最高
                                realTimeQuoteData.kLineData.lowestPrice = stockObject["f16"].SafeToObject<float>(); // 最低
                                realTimeQuoteData.kLineData.openPrice = stockObject["f17"].SafeToObject<float>(); // 今开
                                realTimeQuoteData.riseSpeed = stockObject["f22"].SafeToObject<float>(); // 涨速
                                realTimeQuoteData.bidAskData.theCommittee = stockObject["f31"].SafeToObject<float>(); // 买一价,可能是"-"
                                realTimeQuoteData.buyPrice = stockObject["f32"].SafeToObject<float>(); // 卖一价,可能是"-"
                                realTimeQuoteData.sellPrice = stockObject["f33"].SafeToObject<float>(); // 委比
                                realTimeQuoteData.transactionSumData.buyCount = stockObject["f34"].SafeToObject<float>(); // 外盘
                                realTimeQuoteData.transactionSumData.sellCount = stockObject["f35"].SafeToObject<float>(); // 内盘
                            }
                        }
                    }

                    LifecycleManager.Instance.Get<EventManager>().RaiseEvent(EventType.UpdateRealTimeQuotes);
                }
            }
            catch (Exception ex)
            {
                // WidgetManagement.GetWidget<MessageBox>().SetContent("StockQuoteInterfaceType.CurrentQuotes报错", ex.ToString());
            }
        });
    }

    /// <summary>
    /// 每天收盘时刻后将数据保存到本地
    /// </summary>
    [PlannedTask(mode: PlannedTaskExecuteMode.ExecuteOnlyOnTime | PlannedTaskExecuteMode.ExecuteAfterTime, executeTime = "15:00")]
    public static void SaveCurrentDayStockData()
    {

    }

    /*
        EASTMONEY_QUOTE_FIELDS = {
        'f12': '代码',
        'f14': '名称',
        'f3': '涨跌幅',
        'f2': '最新价',
        'f15': '最高',
        'f16': '最低',
        'f17': '今开',
        'f4': '涨跌额',
        'f8': '换手率',
        'f10': '量比',
        'f9': '动态市盈率',
        'f5': '成交量',
        'f6': '成交额',
        'f18': '昨日收盘',
        'f20': '总市值',
        'f21': '流通市值',
        'f13': '市场编号',
        'f124': '更新时间戳',
        'f297': '最新交易日',
    } 
    */
    private void UpdateHistoryQuotes()
    {

    }

    private static void UpdateHistoryQuotes(StockQuoteData stockData)
    {
        string url = StockQuoteInterface.Instance.GetQuoteUrl(StockQuoteInterfaceType.KLineData, UrlUtil.GetStockCodeParam("600000"), "20240112", "20240112");
        HttpRequest.GetSync(url, (json) =>
        {
            string strippedJson = JsonStripperUtil.GetEastMoneyStrippedJson(json);
            try
            {
                JObject jsonRoot = JObject.Parse(strippedJson);
                JArray klinesArray = jsonRoot?["data"]?["klines"]?.ToObject<JArray>();
                if (klinesArray != null)
                {
                    for (int i = 0; i < klinesArray.Count; i++)
                    {
                        JToken klineToken = klinesArray[i];
                        string klineJson = klineToken.ToString();

                        // 2024-01-12,8.29,8.66,8.66,8.16,2718659,2311985347.48,6.35,10.04,0.79,21.33
                        // date,openPrice,price,highestPrice,lowestPrice,volumn，
                        string[] strings = klineJson.Split(',');
                        if (strings.Length > 0)
                        {
                            DateTime date = Convert.ToDateTime(strings[0]);
                            float openPrice = Convert.ToSingle(strings[1]);
                            float price = Convert.ToSingle(strings[2]);
                            float highestPrice = Convert.ToSingle(strings[3]);
                            float lowestPrice = Convert.ToSingle(strings[4]);
                            float volume = Convert.ToSingle(strings[5]);
                            float money = Convert.ToSingle(strings[6]);
                            float amplitude = Convert.ToSingle(strings[7]);
                            float percentage = Convert.ToSingle(strings[8]);
                            float priceChange = Convert.ToSingle(strings[9]);
                            float turnOverRate = Convert.ToSingle(strings[10]);

                            stockData.perDayKLineList.Add(new KLineData()
                            {
                                openPrice = openPrice,
                                closePrice = price,
                                highestPrice = highestPrice,
                                lowestPrice = lowestPrice,
                                volume = volume,
                                money = money,
                                amplitude = amplitude,
                                percentage = percentage,
                                priceChange = priceChange,
                                turnOverRate = turnOverRate
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                WidgetManagement.GetWidget<MessageBox>().SetContent("StockQuoteInterfaceType.CurrentQuotes报错", ex.ToString());
            }
        }
        );
    }

    public override void PostSerialization()
    {
        // foreach (KeyValuePair<string, StockQuoteData> kvp in m_stockCode2DataList)
        // {
        //     m_stockCode2DataDict[kvp.Key] = kvp.Value;
        // }
        // LifecycleManager.Instance.Get<EventManager>().AddEventHandler(EventType.UpdateHistoryQuotes, new Action<object[]>((param) =>
        // {
        //     StockData stockData = (StockData)param[0];
        //     UpdateHistoryQuotes(stockData);
        // }));
    }

    public List<SearchResult> GetSearchResults(string code)
    {
        string upperCode = code.ToUpper();
        List<SearchResult> result = new List<SearchResult>();
        foreach (var kvp in Instance.m_stockCode2DataDict)
        {
            if (kvp.Value.code.Contains(code) || kvp.Value.abbreviation.StartsWith(upperCode))
            {
                SearchResultType searchResultType = SearchResultType.Unknown;
                if (kvp.Value.code.StartsWith("60"))
                    searchResultType = SearchResultType.SH_A;
                else if (kvp.Value.code.StartsWith("00") || kvp.Value.code.StartsWith("30"))
                    searchResultType = SearchResultType.SZ_A;
                else if (kvp.Value.code.StartsWith("68"))
                    searchResultType = SearchResultType.ChiNext;

                SearchResult searchResult = new SearchResult()
                {
                    type = searchResultType,
                    code = kvp.Value.code,
                    name = kvp.Value.name
                };
                result.Add(searchResult);
            }
        }
        return result;
    }

    public QuoteDataView CreateQuoteView(bool isStock = true)
    {
        QuoteDataView dataView = new StockDataView();
        dataView.Activate(m_stockCode2DataDict.Values.ToList());
        return dataView;
    }

    public QuoteData QueryQuoteData(string code)
    {
        return m_stockCode2DataDict[code];
    }

    [PlannedTask(PlannedTaskExecuteMode.ExecuteDuringTime, executeTime = "*", intervalMs = 1000)]
    private static void RefreshIndexBriefQuoteData()
    {
        HttpRequest.Get(StockQuoteInterface.Instance.GetQuoteUrl(StockQuoteInterfaceType.GlobalIndexBrief), (json) =>
        {
            string strippedJson = JsonStripperUtil.GetEastMoneyStrippedJson(json);
            JObject jsonRoot = JObject.Parse(strippedJson);
            JArray indexDataArray = jsonRoot?["data"]?["diff"]?.ToObject<JArray>();
            if (indexDataArray != null)
            {
                for (int i = 0; i < indexDataArray.Count; i++)
                {
                    JObject dataObject = indexDataArray[i].ToObject<JObject>();
                    if (dataObject != null)
                    {
                        string code = dataObject["f12"].SafeToObject<string>();
                        string name = dataObject["f14"].SafeToObject<string>();

                        float currentPrice = dataObject["f2"].SafeToObject<float>();
                        float percentage = dataObject["f3"].SafeToObject<float>();
                        float priceChange = dataObject["f4"].SafeToObject<float>();

                        IndexBriefQuoteData indexBriefQuoteData = null;
                        if (!Instance.m_code2BriefQuoteDataMap.ContainsKey(code))
                        {
                            indexBriefQuoteData = Instance.m_code2BriefQuoteDataMap[code] = new IndexBriefQuoteData();
                            indexBriefQuoteData.code = code;
                            indexBriefQuoteData.name = name;
                        }
                        else
                        {
                            indexBriefQuoteData = Instance.m_code2BriefQuoteDataMap[code];
                        }
                        indexBriefQuoteData.currentPrice = currentPrice;
                        indexBriefQuoteData.percentage = percentage;
                        indexBriefQuoteData.priceChange = priceChange;
                    }
                }
            }
        });
    }

    public IndexBriefQuoteData QueryIndexBriefQuoteData(string code)
    {
        if (m_code2BriefQuoteDataMap.ContainsKey(code))
        {
            return m_code2BriefQuoteDataMap[code];
        }

        return null;
    }

    public List<QuoteData> GetStockDataListConditional(Func<bool,QuoteData> filterFunc)
    {
        List<QuoteData> result = new List<QuoteData>();
        foreach (var kvp in m_stockCode2DataList)
        {
            StockQuoteData stockQuoteData = kvp.Value;
            result.Add(stockQuoteData);
        }

        return result;
    }
}