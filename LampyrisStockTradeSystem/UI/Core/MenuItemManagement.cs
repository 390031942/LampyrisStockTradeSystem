namespace LampyrisStockTradeSystem;

using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;


public class MenuItemAttribute : Attribute
{
    public string path;

    public MenuItemAttribute(string path)
    {
        this.path = path;
    }
}

public class MenuItemNode
{
    public string name;

    public Action? action;

    public MenuItemNode? parent;

    public List<MenuItemNode> children = new List<MenuItemNode>();

    public MenuItemNode(string name)
    {
        this.name   = name;
    }
}

public class MenuItemManagement
{
    private MenuItemNode m_dummyRoot = new MenuItemNode("");

    private Dictionary<string, MenuItemNode> m_name2nodeDict = new Dictionary<string, MenuItemNode>();

    public void ConstructSinglePath(string path,MethodInfo methodInfo)
    {
        if (string.IsNullOrEmpty(path) || methodInfo == null)
            return;

        Action action = (Action)Delegate.CreateDelegate(typeof(Action), methodInfo);

        // 处理路径分隔符
        path = path.Replace("\\","/");

        // 根据路径分隔符分隔
        string[] strs = path.Split('/');

        MenuItemNode currentNode = m_dummyRoot;

        string fullPath = "";

        foreach (string str in strs)
        {
            if (m_name2nodeDict.ContainsKey(str))
            {
                currentNode = m_name2nodeDict[str];
            } 
            else
            {
                fullPath += ("/" + str);

                MenuItemNode newNode = new MenuItemNode(str);
                m_name2nodeDict[fullPath] = newNode;
                currentNode.children.Add(newNode);
                newNode.parent = currentNode;
                currentNode = newNode;

            }
        }

        currentNode.action = action;
    }

    public void Scan()
    {
        Assembly asm = Assembly.GetExecutingAssembly();

        // 获取所有具有MenuItem属性的无参非泛型的静态方法
        var types = asm.GetTypes();

        m_name2nodeDict.Clear();
        m_dummyRoot.children.Clear();

        foreach (var type in types)
        {
            var methods = type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (var method in methods)
            {
                if (!method.IsGenericMethod && method.GetParameters().Length == 0)
                {
                    MenuItemAttribute menuItem = method.GetCustomAttribute<MenuItemAttribute>();
                    if (menuItem != null)
                    {
                        ConstructSinglePath(menuItem.path,method);
                    }
                }
            }
        }
    }

    private void TraverseNode(MenuItemNode node)
    {
        if(node.children.Count > 0)
        {
            if (ImGui.BeginMenu(node.name))
            {
                foreach(var child in node.children)
                {
                    TraverseNode(child);
                }
            }
            ImGui.EndMenu();
        }
        else
        {
            if (ImGui.MenuItem(node.name))
            {
                node.action?.Invoke();
            }
        }
    }

    public void PerformMenuItem()
    {
        ImGui.BeginMainMenuBar();

        foreach (var children in m_dummyRoot.children)
        {
            TraverseNode(children);
        }
    }
}
