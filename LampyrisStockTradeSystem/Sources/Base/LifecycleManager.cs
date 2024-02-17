/*
** Author: wushuhong
** Contact: gameta@qq.com
** Description: 生命周期管理相关的实现，适用于每个子系统。
*/

namespace LampyrisStockTradeSystem;

using System.Reflection;

// 所有实现ILifecycle接口的类，都会被LifecycleManager创建一个唯一的实例
// 并对他们的生命周期进行管理，此外，你可以通过以下方法来访问一个实例，代码如下：
// LifecycleManager.Instance.Get<>
public interface ILifecycle
{
    public void OnStart();

    public void OnUpdate(float dTime);

    public void OnDestroy();
}

public class LifecycleManager:Singleton<LifecycleManager>
{
    private List<ILifecycle> m_lifecycleContexts = new List<ILifecycle>();

    public void StartUp()
    {
        // 获取当前程序集
        Assembly assembly = Assembly.GetExecutingAssembly();

        // 获取所有实现了 ILiftCycle 接口的类型
        var types = assembly.GetTypes()
            .Where(t => t.GetInterfaces().Contains(typeof(ILifecycle)))
            .ToList();

        // 创建实例
        foreach (var type in types)
        {
            if (type.IsClass && !type.IsAbstract)
            {
                // 注意：这假设每个类型都有一个无参数的构造函数
                ILifecycle instance = (ILifecycle)Activator.CreateInstance(type);

                if(instance != null)
                {
                    m_lifecycleContexts.Add(instance);
                    instance.OnStart();
                }
            }
        }
    }
    public void Tick()
    {
        foreach(ILifecycle lifecycle in m_lifecycleContexts)
        {
            lifecycle.OnUpdate(0.0f);
        }
    }

    public void ShutDown()
    {
        foreach (ILifecycle lifecycle in m_lifecycleContexts)
        {
            lifecycle.OnDestroy();
        }
    }
    
    public T? Get<T>()
    {
        Type requireType = typeof(T);
        foreach (ILifecycle lifecycle in m_lifecycleContexts)
        {
            if(lifecycle.GetType() == requireType)
            {
                return (T)lifecycle;
            }
        }

        return default(T);
    }
}
    
