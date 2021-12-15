using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UObject = UnityEngine.Object;

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
    tga,
    asset,
    mp3,
    mp4,
    wav,
    ogg,
    bytes,
    controller,
    json,
    fbx,
#if UNITY_EDITOR
    preset,
#endif
}

public class ResourcesManager
{
    static private Dictionary<string, LoadedAssetInfo> _loadedAssetPool = new Dictionary<string, LoadedAssetInfo>();
    static private Dictionary<object/*<observer>*/, Dictionary<string, int>> _observeHash = new Dictionary<object, Dictionary<string, int>>();

    private const string ASSET_POOL_KEY = "{0}_{1}"/*Path_Type*/;

    /// <summary>
    /// 单资源加载：【异步】
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="assetPath"></param>
    /// <param name="callback"></param>
    /// <param name="rType"></param>
    /// <param name="observer"></param>
    /// <returns></returns>
    static public Coroutine LoadAsync<T>(string assetPath, Action<T> callback, ExtensionType rType = ExtensionType.prefab, object observer = null) where T : UObject
    {
        assetPath = string.Format("{0}/{1}.{2}", GameConfigs.AssetRoot, assetPath, rType);
        return LoadAsyncWithFullPath<T>(assetPath, callback, observer);
    }

    /// <summary>
    /// 多资源加载：【异步】
    /// </summary>
    /// <typeparam name="T">UnityEngine.Object</typeparam>
    /// <param name="assetPaths">相对路径</param>
    /// <param name="callback"></param>
    static public Coroutine LoadAsync<T>(ICollection<string> assetPaths, Action<T[]> callback, ExtensionType rType = ExtensionType.prefab, object observer = null) where T : UObject
    {
        return CoroutineAgent.StartCoroutine(_LoadAsync<T>(assetPaths, callback, rType, observer));
    }

    static private IEnumerator _LoadAsync<T>(ICollection<string> assetPaths, Action<T[]> callback, ExtensionType rType = ExtensionType.prefab, object observer = null) where T : UObject
    {
        List<T> assets = new List<T>();
        foreach (string path in assetPaths)
        {
            yield return LoadAsync<T>(path, (asset) =>
            {
                assets.Add(asset);
            }, rType, observer);
        }

        callback?.Invoke(assets.ToArray());
    }

    /// <summary>
    /// 单资源加载：【异步】
    /// </summary>
    /// <typeparam name="T">UnityEngine.Object</typeparam>
    /// <param name="fullAssetPath">绝对路径（包含拓展名）</param>
    /// <param name="callback"></param>
    static public Coroutine LoadAsyncWithFullPath<T>(string fullAssetPath, Action<T> callback, object observer = null) where T : UObject
    {
        T loadedAsset = PeekLoadedPool<T>(fullAssetPath);
        if (loadedAsset)
        {
            callback?.Invoke(loadedAsset);
        }
        else
        {
            callback += (obj) =>
            {
                EnqueueLoadedPool<T>(fullAssetPath, obj, observer);
            };

            if (GameConfigs.PublishMode == PublishMode.Release)
            {
                return AssetBundleLoader.Instance().LoadAssetByPath<T>(fullAssetPath, callback);
            }
            else
            {
                return LoadAsyncDebug(fullAssetPath, callback);
            }
        }

        return null;
    }

    /// <summary>
    /// 多资源加载：【异步】
    /// </summary>
    /// <param name="fullAssetPaths"></param>
    /// <param name="stepAction"></param>
    /// <param name="callback"></param>
    /// <param name="observer"></param>
    /// <returns></returns>
    static public Coroutine LoadAsyncWithExtensionPath(ICollection<string> fullAssetPaths, Action<UObject, int> stepAction, Action<UObject[]> callback, object observer = null)
    {
        return CoroutineAgent.StartCoroutine(_LoadAsyncWithExtensionPath(fullAssetPaths, stepAction, callback, observer));
    }

    static private IEnumerator _LoadAsyncWithExtensionPath(ICollection<string> fullAssetPaths, Action<UObject, int> stepAction, Action<UObject[]> callback, object observer = null)
    {
        List<UObject> assets = new List<UObject>();
        foreach (string path in fullAssetPaths)
        {
            string assetPath = string.Format("{0}/{1}",GameConfigs.AssetRoot, path);
            yield return LoadAsyncWithFullPath<UObject>(assetPath, (asset) =>
            {
                stepAction?.Invoke(asset, assets.Count);
                assets.Add(asset);
            }, observer);
        }

        yield return null;

        callback?.Invoke(assets.ToArray());
    }

    /// <summary>
    /// 单资源加载：【同步】
    /// </summary>
    /// <typeparam name="T">UnityEngine.Object</typeparam>
    /// <param name="assetPath">相对路径</param>
    /// <param name="rType"></param>
    /// <param name="observer"></param>
    /// <returns></returns>
    static public T LoadSync<T>(string assetPath, ExtensionType rType = ExtensionType.prefab, object observer = null) where T : UObject
    {
        assetPath = string.Format("{0}/{1}.{2}", GameConfigs.AssetRoot, assetPath, rType);
        return LoadSyncWithFullPath<T>(assetPath, observer);
    }

    /// <summary>
    /// 单资源加载:【同步】
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="fullAssetPath"></param>
    /// <param name="observer"></param>
    /// <returns></returns>
    static public T LoadSyncWithFullPath<T>(string fullAssetPath, object observer = null) where T : UObject
    {
        T obj = PeekLoadedPool<T>(fullAssetPath);

        if (!obj)
        {
            obj = LoadSyncWithFullPath<T>(fullAssetPath);
            EnqueueLoadedPool<T>(fullAssetPath, obj, observer);
        }

        return NullCheck.IsNotNull(obj, string.Format(LogMessage.ASSET_LOADED_FAILED, fullAssetPath)) ? obj : null;
    }

    /// <summary>
    /// 【同步】资源加载
    /// </summary>
    /// <typeparam name="T">UnityEngine.Object</typeparam>
    /// <param name="fullAssetPath">绝对路径（包含拓展名）</param>
    /// <returns></returns>
    static private T LoadSyncWithFullPath<T>(string fullAssetPath) where T : UObject
    {
        if (GameConfigs.PublishMode == PublishMode.Release)
        {
            return AssetBundleLoader.Instance().LoadAssetSyncByPath<T>(fullAssetPath);
        }
        else
        {
            return LoadDebug<T>(fullAssetPath);
        }
    }

    static private T PeekLoadedPool<T>(string fullAssetPath) where T : UObject
    {
        fullAssetPath = string.Format(ASSET_POOL_KEY, fullAssetPath, typeof(T).ToString());

        if (_loadedAssetPool.ContainsKey(fullAssetPath) && _loadedAssetPool[fullAssetPath].LoadedCount > 0)
        {
            return _loadedAssetPool[fullAssetPath].LoadedObject as T;
        }

        return default(T);
    }

    /// <summary>
    /// 记录已加载的资源
    /// </summary>
    /// <param name="fullAssetPath">绝对路径（包含拓展名）</param>
    /// <param name="obj"></param>
    /// <param name="observer"></param>
    static private void EnqueueLoadedPool<T>(string fullAssetPath, UObject obj, object observer)
    {
        fullAssetPath = string.Format(ASSET_POOL_KEY, fullAssetPath, typeof(T).ToString());

        if (!_loadedAssetPool.ContainsKey(fullAssetPath))
            _loadedAssetPool[fullAssetPath] = new LoadedAssetInfo(fullAssetPath);

        _loadedAssetPool[fullAssetPath].Loaded(obj);

        if (observer == null)
            return;

        if (!_observeHash.ContainsKey(observer))
            _observeHash[observer] = new Dictionary<string, int>();

        if (!_observeHash[observer].ContainsKey(fullAssetPath))
            _observeHash[observer][fullAssetPath] = 0;

        _observeHash[observer][fullAssetPath]++;
    }

    /// <summary>
    /// 卸载已加载的资源
    /// </summary>
    /// <param name="observer"></param>
    static public void UnloadAsset(object observer)
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

    static public void UnloadAsset<T>(object observer, string fullAssetPath)
    {
        fullAssetPath = string.Format(ASSET_POOL_KEY, fullAssetPath, typeof(T).ToString());

        if (_observeHash.ContainsKey(observer) && _observeHash[observer].ContainsKey(fullAssetPath))
        {
            UnloadAsset(fullAssetPath, _observeHash[observer][fullAssetPath]);
        }
    }

    /// <summary>
    /// 卸载已加载的资源
    /// </summary>
    /// <param name="fullAssetPath">绝对路径</param>
    /// <param name="dec">计数</param>
    static private void UnloadAsset(string fullAssetPath, int dec)
    {
        if (!string.IsNullOrEmpty(fullAssetPath) && _loadedAssetPool.ContainsKey(fullAssetPath))
        {
            _loadedAssetPool[fullAssetPath].Unload(AssetBundleLoader.Instance().UnloadAssetBundleByAssetPath, dec);
        }
    }

    static public void UnloadAsset<T>(string fullAssetPath, int dec)
    {
        UnloadAsset(string.Format(ASSET_POOL_KEY, fullAssetPath, typeof(T).ToString()), dec);
    }

    static private T LoadDebug<T>(string assetPath) where T : UObject
    {
#if UNITY_EDITOR
        return UnityEditor.AssetDatabase.LoadAssetAtPath<T>(assetPath);
#else
        assetPath = assetPath.Remove(assetPath.LastIndexOf('.'));
        assetPath = assetPath.Replace(GameConfigs.AssetRoot + "/", "");
        assetPath = assetPath.Replace("Resources/", "");
        return Resources.Load<T>(assetPath);
#endif
    }

    static private Coroutine LoadAsyncDebug<T>(string path, Action<T> callback) where T : UObject
    {
#if UNITY_EDITOR
        T obj = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path);
        if(NullCheck.IsNotNull(obj, string.Format(LogMessage.ASSET_LOADED_FAILED, path)))
        {
            callback(obj);
        }
        return null;
#else
        return CoroutineAgent.StartCoroutine(_ResourcesLoad(path, callback));
#endif
    }

    static private IEnumerator _ResourcesLoad<T>(string fullAssetPath, Action<T> callback) where T : UObject
    {
        fullAssetPath = fullAssetPath.Remove(fullAssetPath.LastIndexOf('.'));
        fullAssetPath = fullAssetPath.Replace(GameConfigs.AssetRoot + "/", "");
        fullAssetPath = fullAssetPath.Replace("Resources/", "");

        ResourceRequest request = Resources.LoadAsync<T>(fullAssetPath);
        yield return request;
        callback?.Invoke(request.asset as T);
    }

#if UNITY_EDITOR

    /// <summary>
    /// 仅在Editor下有效
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="assetPath"></param>
    /// <param name="rType"></param>
    /// <returns></returns>
    public static T LoadAssetAtPath<T>(string assetPath, ExtensionType rType) where T : UObject
    {
        assetPath = string.Format("{0}/{1}.{2}", GameConfigs.AssetRoot, assetPath, rType);
        return LoadAssetAtPath<T>(assetPath);
    }

    /// <summary>
    /// 仅在Editor下有效
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="fullAssetPath"></param>
    /// <returns></returns>
    public static T LoadAssetAtPath<T>(string fullAssetPath) where T : UObject
    {
        return UnityEditor.AssetDatabase.LoadAssetAtPath<T>(fullAssetPath);
    }
#endif
    public static void Dispose()
    {
        _loadedAssetPool.Clear();
        _observeHash.Clear();
    }
}
