using System;

public class BaseController<T> : Singleton<T> where T : new()
{
    protected override void Init()
    {
        RegisterEvents();
    }

    protected virtual void RegisterEvents() { }

    protected void AddEvent(string eventType, Action method)
    {
        EventManager.AddEvent(eventType, this, method);
    }

    protected void AddEvent<T1>(string eventType, Action<T1> method)
    {
        EventManager.AddEvent(eventType, this, method);
    }

    protected void AddEvent<T1, T2>(string eventType, Action<T1, T2> method)
    {
        EventManager.AddEvent(eventType, this, method);
    }

    protected void AddEvent<T1, T2, T3>(string eventType, Action<T1, T2, T3> method)
    {
        EventManager.AddEvent(eventType, this, method);
    }

    protected void AddEvent<T1, T2, T3, T4>(string eventType, Action<T1, T2, T3, T4> method)
    {
        EventManager.AddEvent(eventType, this, method);
    }

    protected void BrocastEvent(string eventType)
    {
        EventManager.Brocast(eventType);
    }

    protected void BrocastEvent<T1>(string eventType, T1 param)
    {
        EventManager.Brocast(eventType, param);
    }

    protected void BrocastEvent<T1, T2>(string eventType, T1 param, T2 param2)
    {
        EventManager.Brocast(eventType, param, param2);
    }

    protected void BrocastEvent<T1, T2, T3>(string eventType, T1 param, T2 param2, T3 param3)
    {
        EventManager.Brocast(eventType, param, param2, param3);
    }

    protected void BrocastEvent<T1, T2, T3, T4>(string eventType, T1 param, T2 param2, T3 param3, T4 param4)
    {
        EventManager.Brocast(eventType, param, param2, param3, param4);
    }

    protected void RemoveEvents()
    {
        EventManager.RemoveObserverEvent(this);
    }
}
