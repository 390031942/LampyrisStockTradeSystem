/*
** Author: wushuhong
** Contact: gameta@qq.com
** Description: 枚举类型字段名称管理器
*/
using System.Reflection;

namespace LampyrisStockTradeSystem;

// 定义NamedValueAttribute，用于修饰枚举值
[AttributeUsage(AttributeTargets.Field)]
public class NamedValueAttribute : Attribute
{
    public string Name { get; private set; }

    public NamedValueAttribute(string name)
    {
        this.Name = name;
    }
}

public class EnumNameManager
{
    // 用于存储枚举值和名称的映射
    private static Dictionary<Enum, string> nameMap = new Dictionary<Enum, string>();

    static EnumNameManager()
    {
        // 在静态构造函数中扫描并记录所有枚举值的名称
        ScanAndRecordEnumNames();
    }

    // 扫描所有枚举类型，记录它们的NamedValueAttribute
    private static void ScanAndRecordEnumNames()
    {
        var enumTypes = Assembly.GetExecutingAssembly().GetTypes()
            .Where(t => t.IsEnum);

        foreach (var type in enumTypes)
        {
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static);
            foreach (var field in fields)
            {
                var attribute = field.GetCustomAttribute<NamedValueAttribute>();
                if (attribute != null)
                {
                    var enumValue = (Enum)field.GetValue(null);
                    nameMap[enumValue] = attribute.Name;
                }
            }
        }
    }

    // 获取枚举值的名称，如果没有找到则返回null
    public static string GetName(Enum enumValue)
    {
        if (nameMap.TryGetValue(enumValue, out string name))
        {
            return name;
        }

        return null;
    }
}
