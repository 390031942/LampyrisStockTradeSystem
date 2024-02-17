/*
** Author: wushuhong
** Contact: gameta@qq.com
** Description: 软件的功能注册类,分为两个类:
*  1) 快捷键功能,如按下F3打开上证指数,
*  2) 代码功能，比如在按键精灵输入61表示展示上证A股的涨幅排序
*/

using ImGuiNET;

namespace LampyrisStockTradeSystem;

/// <summary>
/// 按键精灵输入代码的功能
/// </summary>
public class AppCodeFunctionInfo
{
    public string name;
    public string code;
    public Action action;
}

/// <summary>
/// 快捷键功能
/// </summary>
public class AppHotKeyFunctionInfo
{
    public string   name;
    public ImGuiKey hotKey;
    public ImGuiKey defaultHotkey;
    public Action   action;
}

/// <summary>
/// 所有App功能都在这里定义
/// </summary>
public static class AppFunctionRegistry
{
    public static List<AppCodeFunctionInfo> codeFunctionInfos = new List<AppCodeFunctionInfo>()
    {
        new AppCodeFunctionInfo()
        {
            name = "沪市A股排序",
            code = "S1",
            action = ()=> 
            { 
            
            },
        },

        new AppCodeFunctionInfo()
        {
            name = "深市A股排序",
            code = "S2",
            action = ()=>
            {

            },
        },
    };

    public static List<AppHotKeyFunctionInfo> hotKeyFunctionInfos = new List<AppHotKeyFunctionInfo>()
    {
        new AppHotKeyFunctionInfo()
        {
            name = "显示分时成交",
            defaultHotkey = ImGuiKey.F1,
            action = () =>
            {

            },
        },
        new AppHotKeyFunctionInfo()
        {
            name = "显示上证指数",
            defaultHotkey = ImGuiKey.F3,
            action = () => 
            { 
            
            },
        },
        new AppHotKeyFunctionInfo()
        {
            name = "显示深证成指",
            defaultHotkey = ImGuiKey.F4,
            action = () =>
            {

            },
        },
        new AppHotKeyFunctionInfo()
        {
            name = "切换K线/分时图",
            defaultHotkey = ImGuiKey.F5,
            action = () =>
            {

            },
        },
        new AppHotKeyFunctionInfo()
        {
            name = "返回",
            defaultHotkey = ImGuiKey.Escape,
            action = () =>
            {

            },
        },
    };
}