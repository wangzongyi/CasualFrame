using UnityEngine;
using System;
using Utils;

public class TimeManager : Singleton<TimeManager>
{
    const int TIME_PASS_INTERVAL = 1;
    const int TICKS_PER_SECOND = 10000000;

    private float m_lastPassTime;
    private long m_calibrateServerTime;
    private long m_calibrateNativeTime;

    private bool m_inited;

    public long ServerTime { get { return m_calibrateServerTime + NowTimeStamp() - m_calibrateNativeTime; } }
    public static DateTime UtcStart = new DateTime(1970, 1, 1, 0, 0, 0, 0);

    public DateTime ServerDateTime
    {
        get
        {
            return ToDateTime(ServerTime);
        }
    }

    protected override void Init()
    {
        m_lastPassTime = 0;
        m_calibrateNativeTime = NowTimeStamp();
        m_calibrateServerTime = m_calibrateNativeTime;

        EventManager.Instance().AddEvent<long>(EventEnum.CalibrateTime, this, OnCalibrateTime);

        GameContext.UpdateEvent += Update;
        GameContext.ApplicationPause += OnApplicationPause;
    }

    void Update()
    {
        if(Time.time - m_lastPassTime > TIME_PASS_INTERVAL)
        {
            m_lastPassTime = Time.time;
            EventManager.Instance().Brocast(EventEnum.TimePass, ServerTime);
        }
    }

    void OnApplicationPause(bool pauseStatus)
    {
        if (!pauseStatus)
        {
            CalibrateTime();
        }
    }

    void CalibrateTime()
    {
        //Network.RequestHeartBeat()
    }

    /// <summary>
    /// 时间校准
    /// </summary>
    /// <param name="time"></param>
    public void OnCalibrateTime(long time)
    {
        m_calibrateServerTime = time;
        m_calibrateNativeTime = NowTimeStamp();

        m_inited = true;
    }

    public TimeSpan TimeSpan(long targetTime)
    {
        return new TimeSpan((targetTime - ServerTime) * TICKS_PER_SECOND);
    }

    public long CompareTime(long targetTime)
    {
        return ServerTime - targetTime;
    }

    public static long NowTimeStamp()
    {
        return ToTimeStamp(DateTime.UtcNow);
    }

    public static long ToTimeStamp(DateTime time)
    {
        TimeSpan ts = time - UtcStart;
        return Convert.ToInt64(ts.TotalSeconds);
    }

    public static DateTime ToDateTime(long timeStamp)
    {
        DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(UtcStart);
        TimeSpan toNow = new TimeSpan(timeStamp * TICKS_PER_SECOND);
        return dtStart.Add(toNow);
    }
}
