/*
** Author: wushuhong
** Contact: gameta@qq.com
** Description: 延时/延帧管理器
*/
using ImGuiNET;

namespace LampyrisStockTradeSystem;

public class CallTimer:Singleton<CallTimer>
{
    /* 自增的key */
    private int m_increaseKey = 0;

    private readonly Dictionary<int, DelayHandler> m_id2DelayHandlerDict = new Dictionary<int, DelayHandler>();

    private readonly List<int> m_shouldRemoveIDList = new List<int>();

    private enum DelayHandlerType
    {
        Interval = 0,
        FrameLoop = 1,
    }

    private class DelayHandler
    {
        public DelayHandlerType type;
        public Action action;
        public float delayMs;
        public int delayFrame;
        public int repeatTime;
        public float totalTime;
        public int totalFrame;
    }

    public int SetInterval(Action action,float delayMs,int repeatTime = -1)
    {
        if (action == null)
            return -1;

        lock (m_id2DelayHandlerDict)
        {
            int id = m_increaseKey++;
            m_id2DelayHandlerDict[id] = new DelayHandler()
            {
                type = DelayHandlerType.Interval,
                action = action,
                delayMs = delayMs,
                repeatTime = repeatTime,
            };

            return id;
        }
    }

    public int SetFrameLoop(Action action,int delayFrame,int repeatTime = -1)
    {
        if (action == null)
            return -1;

        lock (m_id2DelayHandlerDict)
        {
            int id = m_increaseKey++;
            m_id2DelayHandlerDict[id] = new DelayHandler()
            {
                type = DelayHandlerType.FrameLoop,
                action = action,
                delayFrame = delayFrame,
                repeatTime = repeatTime,
            };

            return id;
        }
    }

    public void ClearTimer(int id)
    {
        lock (m_id2DelayHandlerDict)
        {
            if (m_id2DelayHandlerDict.ContainsKey(id))
            {
                m_id2DelayHandlerDict.Remove(id);
            }
        }
    }

    public void Update()
    {
        lock (m_id2DelayHandlerDict)
        {
            foreach (var pair in m_id2DelayHandlerDict)
            {
                bool shouldDoAction = false;
                DelayHandler delayHandler = pair.Value;

                if (delayHandler.type == DelayHandlerType.Interval)
                {
                    delayHandler.totalTime += ImGui.GetIO().DeltaTime * 1000; // 转化为毫秒
                    if (delayHandler.totalTime >= delayHandler.delayMs)
                    {
                        shouldDoAction = true;
                        delayHandler.totalTime = 0.0f;
                    }
                }
                else
                {
                    delayHandler.totalFrame += 1;
                    if (delayHandler.totalFrame >= delayHandler.delayFrame)
                    {
                        shouldDoAction = true;
                        delayHandler.totalFrame = 0;
                    }
                }

                if (shouldDoAction)
                {
                    delayHandler.action();
                    if (delayHandler.repeatTime != -1)
                    {
                        if (--delayHandler.repeatTime <= 0)
                        {
                            m_shouldRemoveIDList.Add(pair.Key);
                        }
                    }
                }
            }

            foreach (int id in m_shouldRemoveIDList)
            {
                ClearTimer(id);
            }
            m_shouldRemoveIDList.Clear();
        }
    }
}
