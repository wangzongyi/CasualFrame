using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class ScheduleComparer : IComparer<Schedule>
{
    public int Compare(Schedule x, Schedule y)
    {
        return y.EventTime.CompareTo(x.EventTime);
    }
}

public class Schedule
{
    public string ID { get; private set; }
    public long EventTime { get; private set; }
    Delegate eventHandler;
    readonly object param;

    public Schedule(long eventTime, Delegate eventHandler, object param = null)
    {
        ID = Guid.NewGuid().ToString();
        EventTime = eventTime;
        this.eventHandler = eventHandler;
        this.param = param;
    }

    public void Invoke()
    {
        if (eventHandler.GetType() == typeof(Action<object>))
        {
            ((Action<object>)eventHandler)(param);
        }
        else
        {
            ((Action)eventHandler)();
        }
    }
}

public class ScheduleManager : Singleton<ScheduleManager>
{
    PriorityQueue<Schedule> m_scheduleQueue = new PriorityQueue<Schedule>(new ScheduleComparer());
    HashSet<string> m_scheduleHash = new HashSet<string>();

    protected override void Init()
    {
        EventManager.AddEvent<bool>(EventEnum.Online, this, Online);
        EventManager.AddEvent<long>(EventEnum.TimePass, this, TimePass);
    }

    void TimePass(long serverTime)
    {
        while (m_scheduleQueue.Count > 0)
        {
            Schedule schedule = m_scheduleQueue.Top();
            if (schedule.EventTime > serverTime)
                break;

            NotifySchedule(schedule);
            RemoveSchedule(schedule.ID);
            m_scheduleQueue.Pop();
        }
    }

    /// <summary>
    /// 在指定时刻调用方法
    /// </summary>
    /// <param name="updateTime"></param>
    void NotifySchedule(Schedule schedule)
    {
        if (IsExistSchedule(schedule.ID))
            schedule.Invoke();
    }

    /// <summary>
    /// 在时间线上增加事件
    /// </summary>
    /// <param name="scheduleTime"></param>
    /// <param name="method"></param>
    public Schedule PushSchedule(long scheduleTime, Action method)
    {
        Schedule schedule = new Schedule(scheduleTime, method);
        PushSchedule(schedule);
        return schedule;
    }

    public Schedule PushSchedule(long scheduleTime, Action<object> method, object param)
    {
        Schedule schedule = new Schedule(scheduleTime, method, param);
        PushSchedule(schedule);
        return schedule;
    }

    void PushSchedule(Schedule schedule)
    {
        if (!m_scheduleHash.Contains(schedule.ID))
        {
            m_scheduleQueue.Push(schedule);
            m_scheduleHash.Add(schedule.ID);

            Debug.Log(schedule.ID + ":执行时间：" + TimeManager.ToLocalDateTime(schedule.EventTime));
        }
    }

    public bool IsExistSchedule(string guid)
    {
        return m_scheduleHash.Contains(guid);
    }

    /// <summary>
    /// 在时间线上移除事件
    /// </summary>
    /// <param name="updateTime"></param>
    public void RemoveSchedule(string guid)
    {
        if (IsExistSchedule(guid))
        {
            m_scheduleHash.Remove(guid);
            Debug.Log("移除事件：" + guid);
        }
    }

    // Release
    private void Online(bool online)
    {
        if (!online)
        {
            m_scheduleQueue.Clear();
            m_scheduleHash.Clear();
        }
    }
}
