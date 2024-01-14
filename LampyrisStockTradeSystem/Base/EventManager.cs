using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LampyrisStockTradeSystem;

public struct EventExecutionInfo
{
    public Action<object[]> action;
    public object[]         parameters;
}

public class EventManager : Singleton<EventManager>,ILifecycle
{
    private Dictionary<EventType, HashSet<Action<object[]>>> m_event2HandlerListDict = new Dictionary<EventType, HashSet<Action<object[]>>>();

    private List<EventExecutionInfo> m_executetionInfo = new List<EventExecutionInfo>();
    private List<EventExecutionInfo> m_executetionInfoTemp = new List<EventExecutionInfo>();

    private HashSet<Action<object[]>> GetHandlerList(EventType eventType)
    {
        HashSet<Action<object[]>> handlerList;

        if (m_event2HandlerListDict.ContainsKey(eventType))
            handlerList = m_event2HandlerListDict[eventType];
        else
            handlerList = m_event2HandlerListDict[eventType] = new HashSet<Action<object[]>>();

        return handlerList;
    }

    public void AddEventHandler(EventType eventType, Action<object[]> action)
    {
        lock (m_event2HandlerListDict)
        {
            GetHandlerList(eventType).Add(action);
        }
    }

    public void RemoveEventHandler(EventType eventType, Action<object[]> action)
    {
        lock (m_event2HandlerListDict)
        {
            GetHandlerList(eventType).Remove(action);
        }
    }

    public void RaiseEvent(EventType eventType,params object[] args)
    {
        lock(m_event2HandlerListDict)
        {
            foreach (Action<object[]> action in GetHandlerList(eventType))
            {
                m_executetionInfo.Add(new EventExecutionInfo()
                {
                    action = action,
                    parameters = args,
                });
            }
        }
    }

    public void OnStart()
    {
        m_executetionInfo.Capacity = 1024;
    }

    public void OnUpdate(float dTime)
    {
        lock (m_executetionInfo)
        {
            if (m_executetionInfo.Count > 0)
            {
                EventExecutionInfo[] eventExecutionInfos = m_executetionInfo.ToArray();
                m_executetionInfo.Clear();
                
                foreach (EventExecutionInfo eventExecutionInfo in eventExecutionInfos)
                {
                    eventExecutionInfo.action?.Invoke(eventExecutionInfo.parameters);
                }
            }
        }
    }

    public void OnDestroy()
    {
    }
}