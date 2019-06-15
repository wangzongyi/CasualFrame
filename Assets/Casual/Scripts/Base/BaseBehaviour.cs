using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

/// <summary>
/// 一般涉及资源加载的MonoBehaviour需要继承此类
/// 没有特殊情况推荐使用异步方法
/// </summary>
public abstract class BaseBehaviour : MonoBehaviour
{
    private void Awake()
    {
        OnAwake();
    }

    protected virtual void OnAwake()
    {
        RegisterEvents();
    }

    protected void AddEvent(string eventType, Action method)
    {
        EventManager.AddEvent(eventType, this, method);
    }

    protected void AddEvent<T>(string eventType, Action<T> method)
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

    protected void ExcuteEvent(string eventType)
    {
        EventManager.Brocast(eventType);
    }

    protected void ExcuteEvent<T>(string eventType, T param)
    {
        EventManager.Brocast<T>(eventType, param);
    }

    protected void ExcuteEvent<T, T1>(string eventType, T param, T1 param1)
    {
        EventManager.Brocast<T, T1>(eventType, param, param1);
    }

    protected void ExcuteEvent<T, T1, T2>(string eventType, T param, T1 param1, T2 param2)
    {
        EventManager.Brocast<T, T1, T2>(eventType, param, param1, param2);
    }

    protected void ExcuteEvent<T, T1, T2, T3>(string eventType, T param, T1 param1, T2 param2, T3 param3)
    {
        EventManager.Brocast<T, T1, T2, T3>(eventType, param, param1, param2, param3);
    }

    /// <summary>
    /// 注册事件的虚方法，如果子类有事件监听，需重写此方法
    /// </summary>
    protected virtual void RegisterEvents() { }

    /// <summary>
    /// 释放当前观察者事件，一般会自动在程序结束调用
    /// </summary>
    protected void RemoveEvents()
    {
        EventManager.RemoveObserverEvent(this);
    }

    protected void ReturnObject(GameObject inst)
    {
        GameObjectAgent.ReturnObject(this, inst);
    }

    protected void ReturnObjects()
    {
        GameObjectAgent.ReturnObjects(this);
    }

    /// <summary>
    /// 卸载已经加载的资源，一般会在程序结束后自动调用
    /// </summary>
    protected virtual void UnloadAsset()
    {
        ResourcesManager.UnloadAsset(this);
    }

    protected virtual void OnDisable()
    {
        ReturnObjects();
    }

    protected virtual void OnDestroy()
    {
        RemoveEvents();
        UnloadAsset();
    }
}
