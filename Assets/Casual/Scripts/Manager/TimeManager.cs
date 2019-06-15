using UnityEngine;
using System;

public class TimeManager : Singleton<TimeManager>
{
    const int TIME_PASS_INTERVAL = 1;
    const int TICKS_PER_SECOND = 10000000;

    public static long LOCAL_TIME_START = 62135625600; // 服务器所在时区的1970/01/01 00:00:00
    public static long NowTimeStamp { get { return ToUnixTimeStamp(DateTime.Now); } }

    private float m_lastPassTime;
    private long m_calibrateServerTime;
    private long m_calibrateNativeTime;

    public long ServerTime { get { return m_calibrateServerTime - m_calibrateNativeTime + NowTimeStamp; } }

    public DateTime ServerDateTime
    {
        get
        {
            return ToLocalDateTime(ServerTime);
        }
    }

    protected override void Init()
    {
        m_lastPassTime = 0;
        m_calibrateNativeTime = NowTimeStamp;
        m_calibrateServerTime = m_calibrateNativeTime;

        EventManager.AddEvent<long>(EventEnum.CalibrateTime, this, OnCalibrateTime);

        GameContext.UpdateEvent += Update;
        GameContext.ApplicationPause += OnApplicationPause;
    }

    void Update()
    {
        if (Time.time - m_lastPassTime > TIME_PASS_INTERVAL)
        {
            m_lastPassTime = Time.time;
            EventManager.Brocast(EventEnum.TimePass, ServerTime);
        }
    }

    void OnApplicationPause(bool pauseStatus)
    {
        if (!pauseStatus)
        {
            CalibrateTime();
        }
    }

    /// <summary>
    /// 可在心跳和从后台返回时校准时间戳
    /// </summary>
    void CalibrateTime()
    {
        //Network.RequestHeartBeat()
    }

    /// <summary>
    /// 时间校准
    /// </summary>
    /// <param name="serverTimeStamp"></param>
    public void OnCalibrateTime(long serverTimeStamp)
    {
        m_calibrateServerTime = serverTimeStamp;
        m_calibrateNativeTime = NowTimeStamp;
    }

    public TimeSpan TimeSpan(long targetTime)
    {
        return new TimeSpan((targetTime - ServerTime) * TICKS_PER_SECOND);
    }

    public long CompareTime(long targetTime)
    {
        return ServerTime - targetTime;
    }

    /// <summary>
    /// 将服务器所在时区的时间类型转化为时间戳
    /// </summary>
    /// <param name="localDateTime"></param>
    /// <returns></returns>
    public static long ToUnixTimeStamp(DateTime localDateTime)
    {
        return localDateTime.Ticks / TICKS_PER_SECOND - LOCAL_TIME_START;
    }

    /// <summary>
    /// 将服务器所在时区的时间戳转为时间类型
    /// </summary>
    /// <param name="localTimeStamp"></param>
    /// <returns></returns>
    public static DateTime ToLocalDateTime(long localTimeStamp)
    {
        return new DateTime((localTimeStamp + LOCAL_TIME_START) * TICKS_PER_SECOND);
    }
}
