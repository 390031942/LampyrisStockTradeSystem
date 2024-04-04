/*
** Author: wushuhong
** Contact: gameta@qq.com
** Description: 异步操作基类
*/

namespace LampyrisStockTradeSystem;

public abstract class AsyncOperation
{
    protected float m_progress;

    protected bool m_finished;

    public float progress => m_progress;

    public bool finished => m_finished;

    public Action onCompletedCallback;

    public abstract object result { get; }

    public abstract void Execute();

    protected void ExecuteInternal()
    {
        // 在子线程上执行任务
        Task.Run(() =>
        {
            m_finished = false;
            Execute();
            m_finished = true;

            // 执行完后推迟一帧，在主线程上执行 onCompletedCallback
            if (onCompletedCallback != null)
            {
                CallTimer.Instance.SetFrameLoop(() =>
                {
                    onCompletedCallback.Invoke();
                }, 1, 1);
            }

        });
    }
}
