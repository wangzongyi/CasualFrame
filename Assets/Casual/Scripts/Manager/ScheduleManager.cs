using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class ScheduleEventComparer : IComparer<long>
{
    public int Compare(long x, long y)
    {
        return y.CompareTo(x);
    }
}

public class ScheduleManager : Singleton<ScheduleManager>
{
    PriorityQueue<long> m_schedule = new PriorityQueue<long>(new ScheduleEventComparer());
    Dictionary<long, Action> m_eventQueue = new Dictionary<long, Action>();

    protected override void Init()
    {
        EventManager.Instance().AddEvent<bool>(EventEnum.Online, this, Online);
        EventManager.Instance().AddEvent<long>(EventEnum.TimePass, this, TimePass);
    }

    void TimePass(long serverTime)
    {
        if (m_schedule.Count <= 0)
            return;

        long eventClock = m_schedule.Top();
        if (eventClock <= serverTime)
        {
            NotifyUpdate(eventClock);
            RemoveEvent(eventClock);
        }
    }

    /// <summary>
    /// 在指定时刻调用方法
    /// </summary>
    /// <param name="updateTime"></param>
    void NotifyUpdate(long updateTime)
    {
        if (m_eventQueue.ContainsKey(updateTime) && m_eventQueue[updateTime] != null)
            m_eventQueue[updateTime]();
    }

    /// <summary>
    /// 在时间线上增加事件
    /// </summary>
    /// <param name="updateTime"></param>
    /// <param name="method"></param>
    public void AddEvent(long updateTime, Action method)
    {
        if (!m_eventQueue.ContainsKey(updateTime))
        {
            m_schedule.Push(updateTime);
            m_eventQueue[updateTime] = method;
            return;
        }
        m_eventQueue[updateTime] += method;
    }

    /// <summary>
    /// 在时间线上移除事件
    /// </summary>
    /// <param name="updateTime"></param>
    void RemoveEvent(long updateTime)
    {
        if (m_eventQueue.ContainsKey(updateTime))
        {
            m_eventQueue[updateTime] = null;
            m_eventQueue.Remove(updateTime);
            m_schedule.Pop();
        }
    }

    public void RemoveEvent(long updateTime, Action method)
    {
        if (m_eventQueue.ContainsKey(updateTime))
        {
            m_eventQueue[updateTime] -= method;
            if (m_eventQueue[updateTime] == null)
            {
                RemoveEvent(updateTime);
            }
        }
    }

    // Release
    private void Online(bool online)
    {
        if (!online)
        {
            m_schedule.Clear();
            m_eventQueue.Clear();
        }
    }
}
