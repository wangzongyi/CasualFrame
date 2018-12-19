using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventLib
{
    public EventLib(string type)
    {
        _eventType = type;
    }

    /// <summary>
    /// 不同观察者之间可能需要停止某个事件
    /// </summary>
    bool _stop;
    string _eventType;

    Dictionary<object/*observer*/, Delegate> _observers = new Dictionary<object, Delegate>();
    public int ObserverCount { get { return _observers.Count; } }
    public string EventType { get { return _eventType; } }

    public bool Register(object observer, Delegate method)
    {
        if (!_observers.ContainsKey(observer))
        {
            _observers[observer] = null;
        }
        
        Delegate listener = _observers[observer];
        if (listener == null || listener.GetType() == method.GetType())
        {
            _observers[observer] = Delegate.Combine(listener, method);
            return true;
        }

        Debug.LogErrorFormat("事件添加失败! 标志为: {0}的事件类型为：{1}，欲添加的事件类型{2}。", _eventType, listener.GetType(), method.GetType());
        
        return false;
    }

    /// <summary>
    /// 取消事件的注册，如果method为空，注销所有观察者
    /// </summary>
    /// <param name="observer"></param>
    /// <param name="method"></param>
    public void Disconnect(object observer, Delegate method = null)
    {
        Delegate listener = null;
        if (observer == null || !_observers.TryGetValue(observer, out listener))
            return;

        if (listener == null || method == null)
        {
            _observers.Remove(observer);
            return;
        }

        if (listener.GetType() != method.GetType())
        {
            Debug.LogErrorFormat("事件移除失败! 标志为: {0}的事件类型为：{1}，欲添加的事件类型{2}。", _eventType, listener.GetType(), method.GetType());
            return;
        }

        listener = Delegate.Remove(listener, method);

        if (listener == null)
        {
            _observers.Remove(observer);
        }
    }

    public void Invoke()
    {
        List<Delegate> methods = GetObserver<Action>();

        foreach (Action method in methods)
        {
            if (_stop)
                break;

            method();
        }

        _stop = false;
    }

    public void Invoke<T>(T arg)
    {
        List<Delegate> methods = GetObserver<Action<T>>();

        foreach (Action<T> method in methods)
        {
            if (_stop)
                break;

            method(arg);
        }

        _stop = false;
    }

    public void Invoke<T, T2>(T arg, T2 arg2)
    {
        List<Delegate> methods = GetObserver<Action<T, T2>>();

        foreach (Action<T, T2> method in methods)
        {
            if (_stop)
                break;

            method(arg, arg2);
        }

        _stop = false;
    }

    public void Invoke<T, T2, T3>(T arg, T2 arg2, T3 arg3)
    {
        List<Delegate> methods = GetObserver<Action<T, T2, T3>>();

        foreach (Action<T, T2, T3> method in methods)
        {
            if (_stop)
                break;

            method(arg, arg2, arg3);
        }

        _stop = false;
    }

    public void Invoke<T, T2, T3, T4>(T arg, T2 arg2, T3 arg3, T4 arg4)
    {
        List<Delegate> methods = GetObserver<Action<T, T2, T3, T4>>();

        foreach (Action<T, T2, T3, T4> method in methods)
        {
            if (_stop)
                break;

            method(arg, arg2, arg3, arg4);
        }

        _stop = false;
    }

    List<Delegate> GetObserver<T>()
    {
        List<Delegate> methods = new List<Delegate>();
        foreach (Delegate method in _observers.Values)
        {
            if (method is T)
            {
                methods.Add(method);
            }
        }

        return methods;
    }

    public void Stop()
    {
        _stop = true;
    }
}

public class EventManager : Singleton<EventManager>
{
    /* 
     * 因为新事件涉及一个EventLib实例生成，这里注销事件的时候不移除,类似做个缓存
     * List<string> _emptyEvents = new List<string>();
     */
    Dictionary<string/*EventType*/, EventLib> _eventDic = new Dictionary<string, EventLib>();

    public bool AddEvent(string type, object observer, Action method)
    {
        return AddEventExt(type, observer, method);
    }

    public bool AddEvent<T>(string type, object observer, Action<T> method)
    {
        return AddEventExt(type, observer, method);
    }

    public bool AddEvent<T, T2>(string type, object observer, Action<T, T2> method)
    {
        return AddEventExt(type, observer, method);
    }

    public bool AddEvent<T, T2, T3>(string type, object observer, Action<T, T2, T3> method)
    {
        return AddEventExt(type, observer, method);
    }

    public bool AddEvent<T, T2, T3, T4>(string type, object observer, Action<T, T2, T3, T4> method)
    {
        return AddEventExt(type, observer, method);
    }

    public bool AddEventExt(string type, object observer, Delegate method)
    {
        if (observer == null)
        {
            Debug.LogErrorFormat("Observer is null!");
            return false;
        }

        EventLib eventLib = null;
        if (!_eventDic.TryGetValue(type, out eventLib))
        {
            eventLib = new EventLib(type);
            _eventDic[type] = eventLib;
        }
        
        return eventLib.Register(observer, method);
    }

    /// <summary>
    /// 因为新事件涉及一个EventLib实例生成，这里注销事件的时候不移除,类似做个缓存
    /// </summary>
    /// <param name="type"></param>
    private void RemoveEvent(string type)
    {
        if (_eventDic.ContainsKey(type))
        {
            _eventDic.Remove(type);
            Debug.LogFormat("已移除事件：{0}!", type);
        }
    }

    public void RemoveEvent(string type, object observer, Action method)
    {
        RemoveEventExt(type, observer, method);
    }

    public void RemoveEvent<T>(string type, object observer, Action<T> method)
    {
        RemoveEventExt(type, observer, method);
    }

    public void RemoveEvent<T, T2>(string type, object observer, Action<T, T2> method)
    {
        RemoveEventExt(type, observer, method);
    }

    public void RemoveEvent<T, T2, T3>(string type, object observer, Action<T, T2, T3> method)
    {
        RemoveEventExt(type, observer, method);
    }

    public void RemoveEvent<T, T2, T3, T4>(string type, object observer, Action<T, T2, T3, T4> method)
    {
        RemoveEventExt(type, observer, method);
    }

    public void RemoveEventExt(string type, object observer, Delegate method)
    {
        EventLib eventLib = null;
        if (_eventDic.TryGetValue(type, out eventLib))
        {
            eventLib.Disconnect(observer, method);
            /* 
             * 因为新事件涉及一个EventLib实例生成，这里注销事件的时候不移除,类似做个缓存
            if (eventLib.ObserverCount == 0)
            {
                RemoveEvent(type);
            }
            */
        }
    }

    /// <summary>
    /// 移除某个观察者的所有事件
    /// </summary>
    /// <param name="observer"></param>
    public void RemoveObserverEvent(object observer)
    {
        if (observer == null)
            return;

        /* 
         * 因为新事件涉及一个EventLib实例生成，这里注销事件的时候不移除,类似做个缓存
         * _emptyEvents.Clear();
         */

        Dictionary<string, EventLib>.Enumerator enumerator = _eventDic.GetEnumerator();
        while(enumerator.MoveNext())
        {
            EventLib eventLib = enumerator.Current.Value;
            eventLib.Disconnect(observer);
            /*
            if (eventLib.ObserverCount == 0)
            {
                _emptyEvents.Add(eventLib.EventType);
            }
            */
        }
        /*
        foreach (string emptyType in _emptyEvents)
        {
            RemoveEvent(emptyType);
        }
        */
    }

    /// <summary>
    /// 清除所有事件，一般只在游戏重新初始化时调用
    /// </summary>
    public void Clear()
    {
        _eventDic.Clear();
    }

    /// <summary>
    /// 停止某个事件继续调用，标记会在此次调用后清除
    /// </summary>
    /// <param name="type"></param>
    public void StopEvent(string type)
    {
        EventLib eventLib = null;
        if (_eventDic.TryGetValue(type, out eventLib))
        {
            eventLib.Stop();
        }
    }

    public void Brocast(string type)
    {
        EventLib eventLib = null;
        if (_eventDic.TryGetValue(type, out eventLib))
        {
            eventLib.Invoke();
        }
    }

    public void Brocast<T>(string type, T arg)
    {
        EventLib eventLib = null;
        if (_eventDic.TryGetValue(type, out eventLib))
        {
            eventLib.Invoke(arg);
        }
    }
    public void Brocast<T, T2>(string type, T arg, T2 arg2)
    {
        EventLib eventLib = null;
        if (_eventDic.TryGetValue(type, out eventLib))
        {
            eventLib.Invoke(arg, arg2);
        }
    }
    public void Brocast<T, T2, T3>(string type, T arg, T2 arg2, T3 arg3)
    {
        EventLib eventLib = null;
        if (_eventDic.TryGetValue(type, out eventLib))
        {
            eventLib.Invoke(arg, arg2, arg3);
        }
    }
    public void Brocast<T, T2, T3, T4>(string type, T arg, T2 arg2, T3 arg3, T4 arg4)
    {
        EventLib eventLib = null;
        if (_eventDic.TryGetValue(type, out eventLib))
        {
            eventLib.Invoke(arg, arg2, arg3, arg4);
        }
    }
}
