using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UObject = UnityEngine.Object;
using System.IO;

public class LoadedAssetInfo : IDisposable
{
    public LoadedAssetInfo(string path)
    {
        AssetPath = path;
        LoadedCount = 0;
    }

    public string AssetPath { get; private set; }
    public object LoadedObject { get; private set; }
    public int LoadedCount { get; private set; }

    public void Loaded(object obj)
    {
        LoadedCount++;
        LoadedObject = obj;
    }

    public void Unload(Action<string, bool, int> unload, int dec)
    {
        LoadedCount = Mathf.Max(LoadedCount - dec, 0);
        unload(AssetPath, true, dec);
    }

    public void Dispose()
    {
        LoadedObject = null;
        LoadedCount = 0;
    }
}

/// <summary>
/// 类型不足自行添加
/// </summary>
public enum ExtensionType
{
    prefab,
    mat,
    fontsetting,
    otf,
    ttf,
    png,
    asset,
    mp3,
    wav,
}

public class ResourcesManager : Singleton<ResourcesManager>
{
    Dictionary<string, LoadedAssetInfo> _loadedAssetPool = new Dictionary<string, LoadedAssetInfo>();
    Dictionary<object/*<observer>*/, Dictionary<string, int>> _observeHash = new Dictionary<object, Dictionary<string, int>>();

    void EnqueueLoadedPool(string path, UObject obj, object observer)
    {
        if (_loadedAssetPool.ContainsKey(path))
            _loadedAssetPool[path].Loaded(obj);

        if (observer == null)
            return;

        if (!_observeHash.ContainsKey(observer))
            _observeHash[observer] = new Dictionary<string, int>();

        if (!_observeHash[observer].ContainsKey(path))
            _observeHash[observer][path] = 0;

        _observeHash[observer][path]++;
    }

    /// <summary>
    /// 资源加载同步方法
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="path">现有路径即可</param>
    /// <param name="rType"></param>
    /// <returns></returns>
    public T LoadSync<T>(string path, ExtensionType rType = ExtensionType.prefab) where T : UObject
    {
        path = string.Format("{0}/{1}.{2}", GameConfigs.AssetRoot, path, rType);
        return LoadSyncWithFullPath<T>(path);
    }

    public T LoadSync<T>(string path, object observer, ExtensionType rType = ExtensionType.prefab) where T : UObject
    {
        path = string.Format("{0}/{1}.{2}", GameConfigs.AssetRoot, path, rType);
        return LoadSyncWithFullPath<T>(path, observer);
    }

    public T LoadSyncWithFullPath<T>(string path, object observer = null) where T : UObject
    {
        if (_loadedAssetPool.ContainsKey(path) && _loadedAssetPool[path].LoadedCount > 0)
            return _loadedAssetPool[path].LoadedObject as T;

        T obj = LoadSyncWithFullPath<T>(path);
        if (!_loadedAssetPool.ContainsKey(path))
            _loadedAssetPool[path] = new LoadedAssetInfo(path);

        EnqueueLoadedPool(path, obj, observer);

        return obj;
    }

    T LoadSyncWithFullPath<T>(string path) where T : UObject
    {
        if (GameConfigs.PublishMode == PublishMode.Release)
        {
            return AssetBundleLoader.Instance().LoadAssetSyncByPath<T>(path);
        }
        else
        {
            return LoadDebug<T>(path);
        }
    }

    /// <summary>
    /// 资源加载异步方法--推荐此方法
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="path">现有路径即可</param>
    /// <param name="callback"></param>
    public void LoadAsync<T>(string path, Action<T> callback, ExtensionType rType = ExtensionType.prefab, object observer = null) where T : UObject
    {
        path = string.Format("{0}/{1}.{2}", GameConfigs.AssetRoot, path, rType);
        LoadAsyncWithFullPath<T>(path, callback);
    }

    /// <summary>
    /// 资源加载异步方法2--
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="path">完整的资源路径，包括拓展名</param>
    /// <param name="callback"></param>
    public void LoadAsyncWithFullPath<T>(string path, Action<T> callback, object observer = null) where T : UObject
    {
        if (!_loadedAssetPool.ContainsKey(path))
        {
            _loadedAssetPool[path] = new LoadedAssetInfo(path);
        }

        if (_loadedAssetPool[path].LoadedCount > 0)
        {
            if (callback != null)
                callback(_loadedAssetPool[path].LoadedObject as T);
        }
        else
        {
            callback += (obj) =>
            {
                EnqueueLoadedPool(path, obj, observer);
            };

            if (GameConfigs.PublishMode == PublishMode.Release)
            {
                AssetBundleLoader.Instance().LoadAssetByPath<T>(path, callback);
            }
            else
            {
                LoadAsyncDebug<T>(path, callback);
            }
        }
    }

    public void UnloadAsset(object observer)
    {
        if (_observeHash.ContainsKey(observer))
        {
            foreach (KeyValuePair<string, int> loadedInfo in _observeHash[observer])
            {
                UnloadAsset(loadedInfo.Key, loadedInfo.Value);
            }
            _observeHash.Remove(observer);
        }
    }

    public void UnloadAsset(string path, int dec)
    {
        if (_loadedAssetPool.ContainsKey(path))
        {
            _loadedAssetPool[path].Unload(AssetBundleLoader.Instance().UnloadAssetBundleByAssetPath, dec);
        }
    }

    /// <summary>
    /// 资源加载异步方法---多资源加载
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="paths"></param>
    /// <param name="callback"></param>
    public void LoadAsync<T>(string[] paths, Action<UObject[]> callback) where T : UObject
    {

    }

    T LoadDebug<T>(string path) where T : UObject
    {
#if UNITY_EDITOR
        return UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path);
#else
        path = path.Remove(path.LastIndexOf('.'));
        path = path.Replace(GameConfigs.AssetRoot + "/", "");
        path = path.Replace("Resources/", "");
        return Resources.Load<T>(path);
#endif
    }

    void LoadAsyncDebug<T>(string path, Action<T> callback) where T : UObject
    {
#if UNITY_EDITOR
        T obj = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path);
        callback(obj);
#else
        //T obj = LoadDebug<T>(path);
        //if (callback != null) callback(obj);

        CoroutineAgent.StartCoroutine(_LoadAsyncDebug(path, callback));
#endif
    }

    IEnumerator _LoadAsyncDebug<T>(string path, Action<T> callback) where T : UObject
    {
        path = path.Remove(path.LastIndexOf('.'));
        path = path.Replace(GameConfigs.AssetRoot + "/", "");
        path = path.Replace("Resources/", "");

        ResourceRequest request = Resources.LoadAsync<T>(path);
        yield return request;
        if (callback != null)
        {
            callback(request.asset as T);
        }
    }
}
