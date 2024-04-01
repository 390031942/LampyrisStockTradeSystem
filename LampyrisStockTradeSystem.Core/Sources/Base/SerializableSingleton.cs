/*
** Author: wushuhong
** Contact: gameta@qq.com
** Description: 可序列化的单例类，比如App设置等
*/

using System.Reflection;

namespace LampyrisStockTradeSystem;

public interface IPostSerializationHandler
{
    public void PostSerialization();
}

[Serializable]
public class SerializableSingleton<T> where T : class, new()
{
    private static T ms_instance;

    public static T Instance
    {
        get
        {
            if (ms_instance == null)
            {
                ms_instance = LifecycleManager.Instance.Get<SerializationManager>().TryDeserializeObjectFromFile<T>();
                if(ms_instance == null)
                {
                    ms_instance = new T();
                }
                else
                {
                    if (typeof(IPostSerializationHandler).IsAssignableFrom(typeof(T)))
                    {
                        MethodInfo method = typeof(T).GetMethod("PostSerialization");
                        if (method != null)
                        {
                            method.Invoke(ms_instance, null); // 通过反射调用func方法
                        }
                    }
                }

                LifecycleManager.Instance.Get<SerializationManager>().Register(ms_instance);
            }
            return ms_instance;
        }
    }

    public virtual void PostSerialization() { }
}