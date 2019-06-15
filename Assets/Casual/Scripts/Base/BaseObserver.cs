using System;

public abstract class BaseObserver<T> : Singleton<T> where T : new()
{
    protected override void Init()
    {
        RegisterEvents();
    }

    public virtual void RegisterEvents(){ }

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

    protected void RemoveEvents()
    {
        EventManager.RemoveObserverEvent(this);
    }
}
