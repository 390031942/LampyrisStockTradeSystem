using ImGuiNET;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LampyrisStockTradeSystem;

public class UniqueWidgetAttribute : Attribute
{
    public UniqueWidgetAttribute():base()
    {

    }
}

public enum WidgetModel
{
    Normal,
    PopupModal,
}

public abstract class Widget
{
    public virtual string Name => this.GetType().Name;
    public virtual WidgetModel widgetModel => WidgetModel.Normal;


    private bool m_shouldClose = false;

    public bool isOpened = true;

    public bool shouldClose => m_shouldClose;

    public void MarkShouldClosed() { m_shouldClose = true; }

    public abstract void OnGUI();

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
                bool value;
                if(widget.widgetModel == WidgetModel.Normal)
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

            foreach (var widget in m_tempWidgetList)
            {
                pair.Value.Remove(widget);
            }
        }
    }
}
