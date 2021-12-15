using System;
using System.Collections.Generic;
using UObject = UnityEngine.Object;
using UnityEngine;

public class GameObjectAgent
{
    static private Dictionary<object/*Observer*/, HashSet<GameObject>> _objectPool = new Dictionary<object, HashSet<GameObject>>();

    /// <summary>
    /// 【同步方法】从GameObjectPool内取GameObject/实例化GameObject
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="prefab"></param>
    /// <param name="observer"></param>
    /// <param name="active"></param>
    /// <returns></returns>
    static public GameObject FetchObject(Transform parent, UObject prefab, object observer, bool active = true)
    {
        return FetchObject<GameObjectPool>(parent, prefab, observer, active);
    }

    static public GameObject FetchObject(GameObject parent, UObject prefab, object observer, bool active = true)
    {
        return FetchObject(parent.transform, prefab, observer, active);
    }

    /// <summary>
    /// 【同步方法】从泛型对象池内取GameObject/实例化GameObject
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="parent"></param>
    /// <param name="prefab"></param>
    /// <param name="observer"></param>
    /// <param name="active"></param>
    /// <returns></returns>
    static public GameObject FetchObject<T>(Transform parent, UObject prefab, object observer, bool active = true) where T : GameObjectPool
    {
        GameObject obj = GameObjectPoolManager.Instance().FetchObject<T>(parent, prefab, active);

        if (observer != null)
        {
            if (!_objectPool.ContainsKey(observer))
                _objectPool[observer] = new HashSet<GameObject>();

            _objectPool[observer].Add(obj);
        }

        return obj;
    }

    static public GameObject FetchObject<T>(GameObject parent, UObject prefab, object observer, bool active = true) where T : GameObjectPool
    {
        return FetchObject<T>(parent.transform, prefab, observer, active);
    }

    /// <summary>
    /// 【同步方法】从GameObjectPool内取GameObject/先加载资源后实例化GameObject
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="path"></param>
    /// <param name="observer"></param>
    /// <param name="active"></param>
    /// <returns></returns>
    static public GameObject FetchObject(Transform parent, string path, object observer, bool active = true)
    {
        return FetchObject(parent, ResourcesManager.LoadSync<GameObject>(path, ExtensionType.prefab, observer), observer, active);
    }

    static public GameObject FetchObject(GameObject parent, string path, object observer, bool active = true)
    {
        return FetchObject(parent.transform, path, observer, active);
    }
    /// <summary>
    /// 【同步方法】从指定对象池内取GameObject/先加载资源后实例化GameObject
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="parent"></param>
    /// <param name="path"></param>
    /// <param name="observer"></param>
    /// <param name="active"></param>
    /// <returns></returns>
    static public GameObject FetchObject<T>(Transform parent, string path, object observer, bool active = true) where T : GameObjectPool
    {
        return FetchObject<T>(parent, ResourcesManager.LoadSync<GameObject>(path, ExtensionType.prefab, observer), observer, active);
    }

    static public GameObject FetchObject<T>(GameObject parent, string path, object observer, bool active = true) where T : GameObjectPool
    {
        return FetchObject<T>(parent.transform, path, observer, active);
    }

    /// <summary>
    /// 【异步方法】从GameObjectPool内取GameObject/先加载资源后实例化GameObject
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="path"></param>
    /// <param name="callback"></param>
    /// <param name="observer"></param>
    /// <param name="active"></param>
    static public Coroutine FetchObjectAsync(Transform parent, string path, Action<GameObject> callback, object observer, bool active = true)
    {
        return FetchObjectAsync<GameObjectPool>(parent, path, callback, observer);
    }

    static public Coroutine FetchObjectAsync(GameObject parent, string path, Action<GameObject> callback, object observer, bool active = true)
    {
        return FetchObjectAsync(parent.transform, path, callback, observer, active);
    }

    /// <summary>
    /// 【异步方法】从GameObjectPool内取一组GameObject/先加载资源后实例化GameObject
    /// </summary>
    /// <returns></returns>
    static public Coroutine FetchObjectAsync(Transform parent, ICollection<string> paths, Action<GameObject[]> callback, object observer, bool active = true)
    {
        return FetchObjectAsync<GameObjectPool>(parent, paths, callback, observer, active);
    }

    /// <summary>
    /// 【异步方法】从GameObjectPool内取一组GameObject/先加载资源后实例化GameObject
    /// </summary>
    /// <returns></returns>
    static public Coroutine FetchObjectAsync<T>(Transform parent, ICollection<string> paths, Action<GameObject[]> callback, object observer, bool active = true) where T : GameObjectPool
    {
        return ResourcesManager.LoadAsync<GameObject>(paths, (prefabs) =>
        {
            List<GameObject> objs = new List<GameObject>();
            if (callback != null)
            {
                foreach (GameObject prefab in prefabs)
                {
                    GameObject instance = FetchObject<T>(parent, prefab, observer, active);
                    if (instance) objs.Add(instance);
                }

                callback(objs.ToArray());
            }

        }, ExtensionType.prefab, observer);
    }

    /// <summary>
    /// 【异步方法】从指定对象池内取GameObject/先加载资源后实例化GameObject
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="parent"></param>
    /// <param name="path"></param>
    /// <param name="callback"></param>
    /// <param name="observer"></param>
    /// <param name="active"></param>
    static public Coroutine FetchObjectAsync<T>(Transform parent, string path, Action<GameObject> callback, object observer, bool active = true) where T : GameObjectPool
    {
        return ResourcesManager.LoadAsync<GameObject>(path, (prefab) =>
        {
            GameObject obj = FetchObject<T>(parent, prefab, observer, active);
            callback?.Invoke(obj);
        }, ExtensionType.prefab, observer);
    }

    static public void ReturnObject(object observer, GameObject inst)
    {
        if (observer == null || inst == null || !_objectPool.ContainsKey(observer))
            return;

        _objectPool[observer].Remove(inst);
        GameObjectPoolManager.Instance().ReleaseObject(inst);
    }

    static public void ReturnObjects(object observer)
    {
        if (observer == null || !_objectPool.ContainsKey(observer))
            return;

        foreach (GameObject inst in _objectPool[observer])
        {
            GameObjectPoolManager.Instance().ReleaseObject(inst);
        }
        _objectPool.Remove(observer);
    }

    static public void Dispose()
    {
        _objectPool.Clear();
    }
}
