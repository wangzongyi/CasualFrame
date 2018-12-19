using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

public class BaseBehaviour : MonoBehaviour
{
    HashSet<GameObject> _objectPool = new HashSet<GameObject>();

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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="prefab"></param>
    /// <param name="active"></param>
    /// <returns></returns>
    protected GameObject FetchObject(Transform parent, Object prefab, bool active = true)
    {
        return FetchObject<GameObjectPool>(parent, prefab, active);
    }

    protected GameObject FetchObject<T>(Transform parent, Object prefab, bool active = true) where T : GameObjectPool
    {
        GameObject obj = GameObjectPoolManager.Instance().FetchObject<T>(parent, prefab, active);
        if (!_objectPool.Contains(obj))
            _objectPool.Add(obj);

        return obj;
    }

    protected GameObject FetchObject(Transform parent, string path, ExtensionType rType = ExtensionType.prefab, bool active = true)
    {
        return FetchObject(parent, ResourcesManager.Instance().LoadSync<GameObject>(path, rType), active);
    }

    protected GameObject FetchObject<T>(Transform parent, string path, ExtensionType rType = ExtensionType.prefab, bool active = true) where T : GameObjectPool
    {
        return FetchObject<T>(parent, ResourcesManager.Instance().LoadSync<GameObject>(path, rType), active);
    }

    protected void FetchObjectAsync(Transform parent, string path, Action<GameObject> callback, ExtensionType rType = ExtensionType.prefab, bool active = true)
    {
        ResourcesManager.Instance().LoadAsync<GameObject>(path, (prefab) =>
        {
            if (callback != null)
            {
                callback(FetchObject(parent, prefab, active));
            }

        }, rType, this);
    }

    /// <summary>
    /// 异步实例化对象
    /// </summary>
    /// <returns></returns>
    protected void FetchObjectAsync<T>(Transform parent, string path, Action<GameObject> callback, ExtensionType rType = ExtensionType.prefab, bool active = true) where T : GameObjectPool
    {
        ResourcesManager.Instance().LoadAsync<GameObject>(path, (prefab) =>
        {
            if (callback != null)
            {
                callback(FetchObject<T>(parent, prefab, active));
            }

        }, rType, this);
    }

    protected void ReturnObject(GameObject inst)
    {
        if (inst == null)
            return;

        _objectPool.Remove(inst);
        GameObjectPoolManager.Instance().ReturnObject(inst);
    }

    protected void ReturnObjects()
    {
        foreach(GameObject inst in _objectPool)
        {
            GameObjectPoolManager.Instance().ReturnObject(inst);
        }
        _objectPool.Clear();
    }

    /// <summary>
    /// 用此方法可自动卸载资源
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="path"></param>
    /// <param name="callback"></param>
    /// <param name="rType"></param>
    protected void LoadAsset<T>(string path, Action<T> callback, ExtensionType rType = ExtensionType.prefab) where T : Object
    {
        ResourcesManager.Instance().LoadAsync(path, callback, rType, this);
    }

    /// <summary>
    /// 用此方法可自动卸载资源
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="path"></param>
    /// <param name="rType"></param>
    /// <returns></returns>
    protected T LoadAssetSync<T>(string path, ExtensionType rType = ExtensionType.prefab) where T : Object
    {
        return ResourcesManager.Instance().LoadSync<T>(path, this, rType);
    }

    /// <summary>
    /// 之后再补齐代码
    /// </summary>
    protected virtual void UnloadAsset()
    {
        ResourcesManager.Instance().UnloadAsset(this);
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
