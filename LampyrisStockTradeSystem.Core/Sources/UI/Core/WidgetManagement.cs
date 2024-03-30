/*
** Author: wushuhong
** Contact: gameta@qq.com
** Description: 主界面子窗口管理类
*/

namespace LampyrisStockTradeSystem;

using ImGuiNET;
using System.Numerics;
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

    // 需要在下一帧调整大小
    private bool m_needResizeNextFrame;

    // 需要在下一帧调整调整
    private bool m_needSetPosNextFrame;

    public virtual ImGuiWindowFlags windowFlags { get; } = ImGuiWindowFlags.None;

    // 上一帧时的窗口大小
    public Vector2 previousFrameSize { get; private set; }

    // 上一帧时的窗口位置
    public Vector2 previousFramePosition { get; private set; }

    // 窗口大小
    private Vector2 m_size;
    public Vector2 size
    {
        get => m_size;
        set
        {
            if(m_size != value)
            {
                m_size = value;
                m_needResizeNextFrame = true;
            }
        }
    }

    // 窗口位置
    private Vector2 m_pos;
    public Vector2 pos
    {
        get => m_pos;
        set
        {
            if (m_pos != value)
            {
                m_pos = value;
                m_needSetPosNextFrame = true;
            }
        }
    }

    //  UI界面逻辑前调用
    public virtual void OnBeforeGUI()
    {
        if(m_needSetPosNextFrame)
        {
            ImGui.SetNextWindowPos(m_pos);
            m_needSetPosNextFrame = false;
        }

        if (m_needResizeNextFrame)
        {
            ImGui.SetNextWindowSize(m_size);
            m_needResizeNextFrame = false;
        }
    }

    //  UI界面逻辑结束后调用
    public virtual void OnAfterGUI()
    {
        previousFrameSize = ImGui.GetWindowSize();
        previousFramePosition = ImGui.GetWindowPos();
    }

    // UI界面逻辑
    public abstract void OnGUI();

    // 窗口被创建时的逻辑
    public virtual void OnAwake() { }

    // 窗口被销毁时的逻辑
    public virtual void OnDestroy() { }

    public virtual void OnMainWindowResize(int width, int height)
    {
        
    }
}

public static class WidgetManagement
{
    private static Dictionary<Type, List<Widget>> m_type2WidgetListDict = new Dictionary<Type, List<Widget>>();

    private static List<Widget> m_needAddWidgetList = new List<Widget>();

    private static List<Widget> m_needRemoveWidgetList = new List<Widget>();

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

        // Unique 检查
        if (!isUniqueWidget(widgetType))
        {
            widget = new T();
            widget.OnAwake();
            // m_type2WidgetListDict[widgetType].Add(widget);
            m_needAddWidgetList.Add(widget);
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
                    widget.OnAwake();
                    // m_type2WidgetListDict[widgetType].Add(widget);
                    m_needAddWidgetList.Add(widget);
                }
            } 
            else
            {
                widget = new T();
                widget.OnAwake();
                // m_type2WidgetListDict[widgetType].Add(widget);
                m_needAddWidgetList.Add(widget);
            }
        }
        return widget;
    }

    public static void HandleWindowResize(int width, int height)
    {
        foreach (var pair in m_type2WidgetListDict)
        {
            foreach (var widget in pair.Value)
            {
                widget.OnMainWindowResize(width, height);
            }
        }
    }

    public static void Update()
    {
        foreach (var pair in m_type2WidgetListDict)
        {
            m_needRemoveWidgetList.Clear();

            foreach (var widget in pair.Value)
            {
                try
                {
                    // ImGui.SetNextWindowSize(new Vector2(widget.size.X, widget.size.Y), ImGuiCond.Once);

                    bool value;

                    widget.OnBeforeGUI();

                    if (widget.widgetModel == WidgetModel.Normal)
                        value = ImGui.Begin(widget.Name, ref widget.isOpened,widget.windowFlags);
                    else
                    {
                        // ImGuiStylePtr style = ImGui.GetStyle();
                        // style.Colors[(int)ImGuiCol.ModalWindowDimBg] = new System.Numerics.Vector4(0.0f, 0.0f, 0.0f, 0.0f); 
                        ImGui.OpenPopup(pair.Key.Name);
                        value = ImGui.BeginPopupModal(pair.Key.Name, ref widget.isOpened, widget.windowFlags);
                    }

                    if (value)
                        widget.OnGUI();

                    widget.OnAfterGUI();

                    if (widget.widgetModel == WidgetModel.Normal)
                        ImGui.End();
                    else
                        ImGui.EndPopup();

                    if (!widget.isOpened)
                    {
                        m_needRemoveWidgetList.Add(widget);
                        continue;
                    }

                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            foreach (var widget in m_needRemoveWidgetList)
            {
                pair.Value.Remove(widget);
            }
        }

        // 处理新增加的Widget
        foreach(var widget in m_needAddWidgetList)
        {
            Type widgetType = widget.GetType();
            if (!m_type2WidgetListDict.ContainsKey(widgetType))
            {
                m_type2WidgetListDict.Add(widgetType, new List<Widget>());
            }
            m_type2WidgetListDict[widget.GetType()].Add(widget);
        }
        m_needAddWidgetList.Clear();
    }
}
