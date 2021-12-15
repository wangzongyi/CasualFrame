using System;
using UnityEngine;

public class BaseSceneController<T> : MonoSingleton<T> where T : BaseSceneController<T>
{
    protected virtual string ThisSceneName { get; }

    protected virtual void OnEnable()
    {
        AddEvent<string>(EventEnum.UNLOAD_SCENE, UnloadScene);
        RegisterEvents();
    }

    protected override void OnAwake()
    {
        InitComponent();
    }

    protected virtual void Start()
    {
        StartScene();
    }

    protected virtual void InitComponent() { }
    protected virtual void InitAssetPathList() { }

    protected virtual void StartScene() {}

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

    public void ReturnObjects(Transform node)
    {
        for (int index = node.childCount- 1; index >= 0; index--)
        {
            ReturnObject(node.GetChild(index).gameObject);
        }
    }

    public void ReturnObjects()
    {
        GameObjectAgent.ReturnObjects(this);
    }

    protected virtual void UnloadScene(string unloadScene)
    {
        if(unloadScene == ThisSceneName)
        {
            ReturnObjects();
        }
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
        RemoveEvents();
        StopAllCoroutines();
    }

    protected virtual void OnDestroy()
    {
        UnloadAsset();
    }
}