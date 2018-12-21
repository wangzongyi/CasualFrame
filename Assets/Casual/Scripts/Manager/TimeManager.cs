using UnityEngine;
using System;
using Utils;

public class TimeManager : Singleton<TimeManager> {

	private long m_time;
	private long m_calibratingServerTime;
	private float m_calibratingGameTime;

	private bool m_inited;

    protected override void Init()
    {
		m_calibratingServerTime = TimeParse.DateTime2Long(System.DateTime.Now);
        GameContext.UpdateEvent += Update;
        GameContext.ApplicationPause += OnApplicationPause;
    }

	void Update ()
	{
		m_time = m_calibratingServerTime + (int) (Time.realtimeSinceStartup - m_calibratingGameTime);
        if (Time.frameCount % 10 == 0)
        {
            EventManager.Instance().Brocast("time_pass", m_time);
        }
	}

	void OnApplicationPause(bool pauseStatus)
	{
		if (!pauseStatus)
		{
		}
	}

	public System.DateTime ServerTime
	{
		get
		{
			return TimeParse.Long2DateTime(m_time);
		}
	}

	public long LongServerTime
	{
		get
		{
			return m_time;
		}
	}

	public void TimeCalibrate(long time)
	{
		m_time = m_calibratingServerTime = time;
		m_calibratingGameTime = Time.realtimeSinceStartup;

		m_inited = true;
	}

	public System.TimeSpan TimeSpan(long targetTime)
	{
		return TimeParse.TimeSpan(targetTime - m_time);
	}

	public long CompareTime(long targetTime)
	{
		return m_time - targetTime;
	}
}
