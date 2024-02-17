/*
** Author: wushuhong
** Contact: gameta@qq.com
** Description: 负责计划时间的任务进行调度，如股票行情实时更新，盘中/收盘后 数据分析
*/

namespace LampyrisStockTradeSystem;

using System;
using System.Collections.Generic;
using System.Reflection;


public enum PlannedTaskExecuteMode
{
    // 只在规定的时间到了的时候执行
    ExecuteOnlyOnTime = 1,

    // 启动程序后如果在规定的时间点后，那么也执行该任务
    ExecuteAfterTime = 2,

    // 启动程序时自动执行一次
    ExecuteOnLaunch = 4,

    // 在时间范围内根据一定的间隔执行
    ExecuteDuringTime = 8,
}

public class PlannedTaskAttribute:Attribute 
{
    // 执行时刻
    public string executeTime;

    // 间隔时间(为0默认为1天一次)
    public int intervalMs;

    public PlannedTaskExecuteMode executeMode;

    public PlannedTaskAttribute(PlannedTaskExecuteMode mode = PlannedTaskExecuteMode.ExecuteOnlyOnTime,
                                string executeTime = "", 
                                int intervalMs = 0)
    {
        this.executeTime = executeTime;
        this.intervalMs = intervalMs;
        this.executeMode = mode;
    }
}

public class PlannedTaskExecutor : ILifecycle
{
    private struct Time
    {
        public int hour;
        public int minute;

        public bool Greater(DateTime other)
        {
            return (this.hour >= other.Hour || (this.hour == other.Hour && this.minute >= other.Minute));
        }

        public bool Greater(Time other)
        {
            return (this.hour >= other.hour || (this.hour == other.hour && this.minute >= other.minute));
        }

        public static Time FromDateTime(DateTime dateTime)
        {
            return new Time() { hour = dateTime.Hour, minute = dateTime.Minute };
        }
    }

    private class OnTimeTaskInfo
    {
        public List<Time> executeTimeList = new List<Time>();

        public Action action;
    }

    private class DuringTimeTaskInfo
    {
        public Time startTime;

        public Time endTime;

        public int intervalMs;

        public Action action;

        public float timeAccumulateMs;
    }

    private List<OnTimeTaskInfo> onTimeTaskInfoList = new List<OnTimeTaskInfo>();
    private List<DuringTimeTaskInfo> duringTimeTaskInfoList = new List<DuringTimeTaskInfo>();
    private List<Action> onLaunchActionList = new List<Action>();

    public void OnDestroy()
    {

    }

    public void OnStart()
    {
        Assembly asm = Assembly.GetExecutingAssembly();

        // 获取所有具有PlannedTask的属性的无参非泛型的静态方法
        var types = asm.GetTypes();

        DateTime nowDate = DateTime.Now;

        foreach (var type in types)
        {
            var methods = type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (var method in methods)
            {
                if (!method.IsGenericMethod && method.GetParameters().Length == 0)
                {
                    PlannedTaskAttribute plannedTask = method.GetCustomAttribute<PlannedTaskAttribute>();
                    if (plannedTask != null)
                    {
                        Action action = (Action)Delegate.CreateDelegate(typeof(Action), method);

                        bool isExecuteOnLaunch = ((int)plannedTask.executeMode & (int)PlannedTaskExecuteMode.ExecuteOnLaunch) != 0;
                        bool isExecuteOnTime = ((int)plannedTask.executeMode & (int)PlannedTaskExecuteMode.ExecuteOnlyOnTime) != 0;
                        bool isExecuteDuringTime = ((int)plannedTask.executeMode & (int)PlannedTaskExecuteMode.ExecuteDuringTime) != 0;
                        bool isExecuteAfterTime = ((int)plannedTask.executeMode & (int)PlannedTaskExecuteMode.ExecuteAfterTime) != 0;

                        // 如果是ExecuteOnLaunch模式则添加到列表，下一帧执行
                        if (isExecuteOnLaunch)
                        {
                            onLaunchActionList.Add(action);
                        }

                        if (isExecuteOnTime)
                        {
                            if (!string.IsNullOrEmpty(plannedTask.executeTime))
                            {
                                string[] dates = plannedTask.executeTime.Split("&");
                                if (dates.Length > 0)
                                {
                                    OnTimeTaskInfo onTimeTaskInfo = new OnTimeTaskInfo();
                                    foreach (string date in dates)
                                    {
                                        if (!string.IsNullOrEmpty(date))
                                        {
                                            if (DateUtil.ParseDateString(date, out var dateTime))
                                            {
                                                onTimeTaskInfo.executeTimeList.Add(Time.FromDateTime(dateTime));
                                            }
                                        }
                                    }
                                    onTimeTaskInfo.action = action;
                                    onTimeTaskInfoList.Add(onTimeTaskInfo);
                                }
                            }
                        }
                        if (isExecuteAfterTime)
                        {
                            if (!string.IsNullOrEmpty(plannedTask.executeTime))
                            {
                                if (DateUtil.ParseDateString(plannedTask.executeTime, out var dateTime))
                                {
                                    if(dateTime.Hour >= nowDate.Hour || (dateTime.Hour == nowDate.Hour && dateTime.Minute >= nowDate.Minute))
                                    {
                                        onLaunchActionList.Add(action);
                                    }
                                }
                            }
                        }
                        if (isExecuteDuringTime)
                        {
                            if (!string.IsNullOrEmpty(plannedTask.executeTime))
                            {
                                string[] strs = plannedTask.executeTime.Split("-");
                                if (strs.Length == 2)
                                {
                                    if (DateUtil.ParseDateString(strs[0], out var dateTimeBegin) &&
                                        DateUtil.ParseDateString(strs[1], out var dateTimeEnd))
                                    {
                                        DuringTimeTaskInfo duringTimeTaskInfo = new DuringTimeTaskInfo();
                                        duringTimeTaskInfo.startTime = Time.FromDateTime(dateTimeBegin);
                                        duringTimeTaskInfo.endTime = Time.FromDateTime(dateTimeEnd);
                                        duringTimeTaskInfo.action = action;
                                        duringTimeTaskInfo.intervalMs = plannedTask.intervalMs;
                                        duringTimeTaskInfoList.Add(duringTimeTaskInfo);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    public void OnUpdate(float dTime)
    {
        Time nowTime = Time.FromDateTime(DateTime.Now);
        float deltaTime = ImGuiNET.ImGui.GetIO().DeltaTime;

        // 处理onLaunchActionList
        foreach (Action action in onLaunchActionList)
        {
            action?.Invoke();
        }
        onLaunchActionList.Clear();

        // 处理onTimeTaskInfoList
        foreach(OnTimeTaskInfo onTimeTaskInfo in onTimeTaskInfoList)
        {
            foreach(Time time in onTimeTaskInfo.executeTimeList)
            {
                if(nowTime.Greater(time))
                {
                    onTimeTaskInfo.action?.Invoke();
                    break;
                }
            }
        }

        // 处理duringTimeTaskInfoList
        foreach (DuringTimeTaskInfo duringTimeTaskInfo in duringTimeTaskInfoList)
        {
            if(nowTime.Greater(duringTimeTaskInfo.startTime) && duringTimeTaskInfo.endTime.Greater(nowTime))
            {
                duringTimeTaskInfo.timeAccumulateMs += deltaTime * 1000;
                if (duringTimeTaskInfo.timeAccumulateMs > duringTimeTaskInfo.intervalMs)
                {
                    duringTimeTaskInfo.timeAccumulateMs = 0; //  duringTimeTaskInfo.timeAccumulateMs - deltaTime * 1000;
                    duringTimeTaskInfo.action?.Invoke();
                }
            }
        }
    }
}

