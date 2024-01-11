using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LampyrisStockTradeSystem;

public class SerializationManager : Singleton<SerializationManager>, ILifecycle
{
    public void OnDestroy()
    {
    }

    public void OnStart()
    {
    }

    public void OnUpdate(float dTime)
    {
    }

    public void Register<T>(T ms_instance) where T : class, new()
    {
        
    }
}
