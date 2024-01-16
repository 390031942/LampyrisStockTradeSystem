/*
** Author: wushuhong
** Contact: gameta@qq.com
** Description: 主界面子窗口管理类
*/

namespace LampyrisStockTradeSystem;

using ImGuiNET;
using System.Reflection;

public class UniqueWidgetAttribute : Attribute
{
    public UniqueWidgetAttribute():base()
    {

    }
}

public enum WidgetModel
{
    Normal, // 正常模式
    PopupModal, // 模态对话框
}

public abstract class Widget
{
    // 窗口名称
    public virtual string Name => this.GetType().Name;

    // 窗口显示模式
    public virtual WidgetModel widgetModel => WidgetModel.Normal;

    // 窗口是否处于开启的状态，设置为false以后下一帧将销毁掉窗口
    public bool isOpened = true;

    // UI界面逻辑
    public abstract void OnGUI();

    // 窗口被创建时的逻辑
    public virtual void OnAwake() { }

    // 窗口被销毁时的逻辑
    public virtual void OnDestroy() { }
}

public static class WidgetManagement
{
    private static Dictionary<Type, List<Widget>> m_type2WidgetListDict = new Dictionary<Type, List<Widget>>();

    private static List<Widget> m_tempWidgetList = new List<Widget>();

    private static bool isUniqueWidget(Type type)
    {
        UniqueWidgetAttribute? unique = type.GetCustomAttribute<UniqueWidgetAttribute>();
        return unique != null;
    }

    public static T GetWidget<T>() where T: Widget, new()
    {
        // Widget 对象
        T widget;

        // Widget 类型
        Type widgetType = typeof(T);

        if (!m_type2WidgetListDict.ContainsKey(widgetType))
        {
            m_type2WidgetListDict[widgetType] = new List<Widget>();
        }

        // Unique 检查
        if (!isUniqueWidget(widgetType))
        {
            widget = new T();
            m_type2WidgetListDict[widgetType].Add(widget);
        }
        else
        {
            if(m_type2WidgetListDict.ContainsKey(widgetType))
            {
                List<Widget> widgetList =  m_type2WidgetListDict[widgetType];
                if(widgetList.Count > 0)
                {
                    widget = (T)widgetList[0];
                }
                else
                {
                    widget = new T();
                    m_type2WidgetListDict[widgetType].Add(widget);
                }
            } 
            else
            {
                widget = new T();
                m_type2WidgetListDict[widgetType].Add(widget);
            }
        }
        return widget;
    }


    public static void Update()
    {
        foreach(var pair in m_type2WidgetListDict)
        {
            m_tempWidgetList.Clear();

            foreach (var widget in pair.Value)
            {
                try
                {
                    bool value;
                    if (widget.widgetModel == WidgetModel.Normal)
                        value = ImGui.Begin(widget.Name, ref widget.isOpened);
                    else
                        value = ImGui.BeginPopupModal(pair.Key.Name, ref widget.isOpened);

                    if (value)
                        widget.OnGUI();

                    if (widget.widgetModel == WidgetModel.Normal)
                        ImGui.End();
                    else
                        ImGui.EndPopup();

                    if (!widget.isOpened)
                    {
                        widget.OnDestroy();
                        m_tempWidgetList.Add(widget);
                        continue;
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            foreach (var widget in m_tempWidgetList)
            {
                pair.Value.Remove(widget);
            }
        }
    }
}
