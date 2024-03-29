﻿using System.Collections.Generic;
using UnityEngine;

public partial class GameObjectPoolManager : Singleton<GameObjectPoolManager>
{
    /// <summary>
    /// 所有对象池总量
    /// </summary>
    const int MAX_POOL_SIZE = 30;
    /// <summary>
    /// 每个对象池内容量
    /// </summary>
    const int MAX_POOL_OBJECT_SIZE = 30;
    /// <summary>
    /// 间隔帧删除检测
    /// </summary>
    const int DELETE_INTERVAL_FRAMES = 2;
    /// <summary>
    /// 每帧隐藏物体数量
    /// </summary>
    const int PERFRAME_DISABLE_COUNT = 10;
    /// <summary>
    /// Active时间
    /// </summary>
    const float STAY_ACTIVE_TIME = 0f;

    private Transform _poolRoot;

    /// <summary>
    /// 实例的对象池
    /// </summary>
    private readonly Dictionary<Object/*prefab*/, GameObjectPool> _objectPools = new Dictionary<Object, GameObjectPool>();

    /// <summary>
    /// GameObject和其Prefab的InstanceID映射关系
    /// </summary>
    private readonly Dictionary<GameObject/*instance*/, Object/*prefab*/> _instanceMap = new Dictionary<GameObject, Object>();

    private readonly List<GameObject> _tempDisablePool = new List<GameObject>();
    private readonly Dictionary<GameObject/*instance*/, float/*disableTime*/> _disablePool = new Dictionary<GameObject, float>();

    protected override void Init()
    {
        _poolRoot = new GameObject("GameObjectPoolManager").transform;
        Object.DontDestroyOnLoad(_poolRoot.gameObject);

        MonoContext.LaterUpdateEvent += LaterUpdate;
        Debug.Log("GameObjectPoolManager Init!");
    }

    private T GetPool<T>(Object prefab) where T : GameObjectPool
    {
        if (!_objectPools.ContainsKey(prefab))
        {
            _objectPools.Add(prefab, System.Activator.CreateInstance<T>());
        }

        return (T)_objectPools[prefab];
    }

    public void PreloadObjects(Object prefab, int count = 1)
    {
        PreloadObjects<GameObjectPool>(prefab, count);
    }

    /// <summary>
    /// 为对象池预生成对象
    /// </summary>
    /// <param name="prefab"></param>
    /// <param name="count"></param>
    /// <param name="type"></param>
    public void PreloadObjects<T>(Object prefab, int count = 1) where T : GameObjectPool
    {
        if (prefab == null)
        {
            return;
        }

        List<GameObject> list = new List<GameObject>();

        for (int i = 0; i < count; ++i)
        {
            list.Add(FetchObject<T>(prefab, false));
        }

        for (int i = 0; i < count; ++i)
        {
            ReleaseObject(list[i]);
        }
    }

    public GameObject FetchObject(Object prefab, bool active = true)
    {
        return FetchObject<GameObjectPool>(null, prefab, active);
    }

    public GameObject FetchObject(Transform parent, Object prefab, bool active = true)
    {
        return FetchObject<GameObjectPool>(parent, prefab, active);
    }

    public GameObject FetchObject<T>(Object prefab, bool active = true) where T : GameObjectPool
    {
        return FetchObject<T>(null, prefab, active);
    }

    /// <summary>
    /// param must be a prefab, please keep your prefab unique
    /// </summary>
    /// <param name="prefab"></param>
    /// <param name="active"></param>
    /// <returns></returns>
    public GameObject FetchObject<T>(Transform parent, Object prefab, bool active = true) where T : GameObjectPool
    {
        if (prefab == null)
            return null;

        T instPool = GetPool<T>(prefab);
        GameObject inst = instPool.Dequeue(prefab, parent);
        if (inst != null)
        {
            inst.transform.SetParent(parent, false);
            instPool.DequeueAction(inst, (GameObject)prefab, active);
            RemoveDisableObject(inst);
            if (typeof(T) != typeof(NullPool)) _instanceMap[inst] = prefab;
        }

        return inst;
    }

    /// <summary>
    /// param must be an instance
    /// </summary>
    /// <param name="inst"></param>
    public void ReleaseObject(GameObject inst)
    {
        if (inst == null || !_poolRoot)
        {
            return;
        }

        if (!_instanceMap.ContainsKey(inst))
        {
            Object.DestroyImmediate(inst);
            return;
        }

        Object prefab = _instanceMap[inst];
        GameObjectPool instPool = GetPool<GameObjectPool>(prefab);
        _instanceMap.Remove(inst);

        if (instPool.Count >= MAX_POOL_OBJECT_SIZE)//超出对象池最大数量，则直接销毁
        {
            Object.DestroyImmediate(inst);
            return;
        }

        Transform instRoot = TryGetInstRoot(prefab);
        instPool.Enqueue(inst, instRoot);
        AddDisableObject(inst);
    }

    Transform TryGetInstRoot(Object prefab)
    {
        string rootName = string.Format("[{0}|{1}]", prefab.name, prefab.GetInstanceID());
        Transform root = _poolRoot.Find(rootName);
        if (root == null)
        {
            GameObject newRoot = new GameObject(rootName);
            newRoot.transform.SetParent(_poolRoot, false);
            root = newRoot.transform;
        }

        return root;
    }

    void RemoveInstRoot(string rootName)
    {
        Transform root = _poolRoot.Find(rootName);
        if (root)
        {
            Object.DestroyImmediate(root.gameObject);
        }
    }

    public void ClearPools()
    {
        List<Object> tempPoolKeys = new List<Object>();
        foreach (var pool in _objectPools)
        {
            pool.Value.DequeueUnUsed((instance) =>
            {
                if (_instanceMap.ContainsKey(instance))
                {
                    _instanceMap.Remove(instance);
                }
            });
            if (pool.Value.ReferenceCount == 0)
                tempPoolKeys.Add(pool.Key);
        }
        foreach (var prefab in tempPoolKeys)
        {
            RemoveInstRoot(_objectPools[prefab].Name);
            _objectPools.Remove(prefab);
        }
    }

    private void LaterUpdate()
    {
        if (Time.frameCount % DELETE_INTERVAL_FRAMES != 0)
            return;

        int disableCount = 0;
        Dictionary<GameObject, float>.Enumerator enumerator = _disablePool.GetEnumerator();
        while (enumerator.MoveNext() && disableCount <= PERFRAME_DISABLE_COUNT)
        {
            if (Time.realtimeSinceStartup >= enumerator.Current.Value + STAY_ACTIVE_TIME)
            {
                _tempDisablePool.Add(enumerator.Current.Key);
                disableCount++;
            }
        }

        for (int index = 0; index < disableCount; index++)
        {
            GameObject disableObject = _tempDisablePool[index];
            if (disableObject != null && disableObject.activeInHierarchy)
            {
                disableObject.SetActive(false);
            }

            RemoveDisableObject(disableObject);
        }

        _tempDisablePool.Clear();
    }

    private void AddDisableObject(GameObject disableObject)
    {
        if (!_disablePool.ContainsKey(disableObject))
        {
            _disablePool.Add(disableObject, Time.realtimeSinceStartup);
        }
    }

    private void RemoveDisableObject(GameObject disableObject)
    {
        if (_disablePool.ContainsKey(disableObject))
        {
            _disablePool.Remove(disableObject);
        }
    }
}
