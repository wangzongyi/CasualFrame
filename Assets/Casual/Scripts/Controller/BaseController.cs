using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseController<T> : Singleton<T> where T : new()
{
    protected override void Init()
    {
        RegisterEvents();
    }

    protected virtual void RegisterEvents()
    {

    }

    protected void AddEvent(string eventType, Action method)
    {
        EventManager.Instance().AddEvent(eventType, this, method);
    }

    protected void AddEvent<T>(string eventType, Action<T> method)
    {
        EventManager.Instance().AddEvent(eventType, this, method);
    }

    protected void AddEvent<T1, T2>(string eventType, Action<T1, T2> method)
    {
        EventManager.Instance().AddEvent(eventType, this, method);
    }

    protected void AddEvent<T1, T2, T3>(string eventType, Action<T1, T2, T3> method)
    {
        EventManager.Instance().AddEvent(eventType, this, method);
    }
    protected void AddEvent<T1, T2, T3, T4>(string eventType, Action<T1, T2, T3, T4> method)
    {
        EventManager.Instance().AddEvent(eventType, this, method);
    }

    protected void RemoveEvents()
    {
        EventManager.Instance().RemoveObserverEvent(this);
    }
}
