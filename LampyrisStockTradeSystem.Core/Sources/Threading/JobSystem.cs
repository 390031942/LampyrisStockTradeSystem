using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LampyrisStockTradeSystem;

public struct JobInfo
{
    public object jobData;

    public Action<object> jobAction;
    public Action<object> jobFinishedCallback;
}

public class JobSystem:Singleton<JobSystem>
{
    private Thread m_thread;

    private volatile bool shouldStop = false;

    private volatile Queue<JobInfo> m_jobInfo = new Queue<JobInfo>();

    public void Init()
    {
        if(m_thread == null)
        {
            m_thread = new Thread(this.Update);
        }
    }

    public void ShutDown()
    {
        if(m_thread != null)
        {
            shouldStop = true;
        }
    }

    public void Update() 
    {
        while (!shouldStop)
        {
            if(m_jobInfo.Count > 0)
            {
                JobInfo jobInfo = m_jobInfo.Dequeue();
                jobInfo.jobAction(jobInfo.jobData);
            }
            else

            {
                Thread.Sleep(10);
            }
        }
    }

}
