using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UObject = UnityEngine.Object;

public class AssetBundleInfo
{
    public int ReferencedCount;
    public AssetBundle AssetBundle;

    public AssetBundleInfo(AssetBundle assetBundle, string bundleName, int referencedCount = 1)
    {
        AssetBundle = assetBundle;
        ReferencedCount = referencedCount;
        //LogWriter.LuaPrint("Modify RefrencedCount init " + bundleName + " : " + ReferencedCount.ToString());
    }
}

public class AssetLoader : IDisposable
{
    public Type assetType;
    public string[] assetNames;
    public Action<UObject[]> sharpFunc;
    public Action<AssetBundle> sharpBundleFunc;

    public void LoadComplete(AssetBundle bundle)
    {
        if (sharpBundleFunc != null)
        {
            sharpBundleFunc(bundle);
        }

        if (sharpFunc != null)
        {
            List<UObject> loadAssets = new List<UObject>();
            for (int i = 0, len = assetNames.Length; i < len; i++)
            {
                UObject asset = bundle.LoadAsset(assetNames[i], assetType);
                loadAssets.Add(asset);
            }

            sharpFunc(loadAssets.ToArray());
        }
    }

    public void Dispose()
    {
        assetNames = null;
        sharpFunc = null;
        sharpBundleFunc = null;
    }
}

public class AssetBundleLoader : Singleton<AssetBundleLoader>
{
    AssetBundleManifest _assetBundleManifest = null;
    Dictionary<string, string[]> _dependencies = new Dictionary<string, string[]>();
    Dictionary<string, string> _assetPathInfoMap = new Dictionary<string, string>();
    Dictionary<string, AssetBundleInfo> _loadedAssetBundles = new Dictionary<string, AssetBundleInfo>();
    Dictionary<string, List<AssetLoader>> _loadRequests = new Dictionary<string, List<AssetLoader>>();

    // Load AssetBundleManifest.
    protected override void Init()
    {
        if (!GameConfigs.DebugMode)
        {
            LoadMainifest();
            LoadAssetPathInfos();
        }
    }

    public void LoadMainifest()
    {
        _assetBundleManifest = LoadAssetSync<AssetBundleManifest>(GameConfigs.AssetDir, "AssetBundleManifest");
    }

    public void UnloadMainfest()
    {
        UnloadAssetBundle(GameConfigs.AssetDir);
    }

    /// <summary>
    /// 加载资源的路径所对应的BundleName信息
    /// </summary>
    public void LoadAssetPathInfos()
    {
        string bundleName = GameConfigs.BundleFileName;
        TextAsset textAsset = LoadAssetSync<TextAsset>(bundleName, bundleName);

        string[] files = textAsset.text.Split('\r', '\n');
        for (int index = 0, len = files.Length; index < len; index++)
        {
            if (!string.IsNullOrEmpty(files[index]))
            {
                string[] fileInfos = files[index].Split('|');
                if (fileInfos.Length >= 2)
                {
                    _assetPathInfoMap[fileInfos[1]] = fileInfos[0];
                }
            }
        }
    }

    string GetBundleNameWithAssetPath(string assetPath)
    {
        return _assetPathInfoMap.ContainsKey(assetPath) ? _assetPathInfoMap[assetPath] : null;
    }

    public AssetBundle LoadAssetBundleSync(string abName)
    {
        return LoadAssetBundleSyncImp(abName);
    }

    private AssetBundle LoadAssetBundleSyncImp(string abName)
    {
        if (string.IsNullOrEmpty(abName))
            return null;

        abName = GetFinalABName(abName);

        AssetBundleInfo assetBundleInfo = null;
        if (_loadedAssetBundles.TryGetValue(abName, out assetBundleInfo))
        {
            assetBundleInfo.ReferencedCount++;
            return assetBundleInfo.AssetBundle;
        }
        else
        {
            string[] dependencies = GetDependencies(abName);
            for (int index = 0, len = dependencies.Length; index < len; index++)
            {
                LoadAssetBundleSyncImp(dependencies[index]);
            }
        }
        string url = UnityUtils.GetAssetURL(abName);
        AssetBundle bundle = AssetBundle.LoadFromFile(url);

        if (bundle != null)
        {
            _loadedAssetBundles.Add(abName, new AssetBundleInfo(bundle, abName));
        }

        return bundle;
    }

    public T LoadAssetSyncByPath<T>(string assetPath) where T : UObject
    {
        string abName = GetBundleNameWithAssetPath(assetPath);
        string assetName = Path.GetFileNameWithoutExtension(assetPath);

        return LoadAssetSync<T>(abName, assetName);
    }

    /// <summary>
    /// 资源加载同步方法
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="abName"></param>
    /// <param name="assetName"></param>
    /// <returns></returns>
    public T LoadAssetSync<T>(string abName, string assetName) where T : UObject
    {
        AssetBundle assetBundle = LoadAssetBundleSync(abName);
        return assetBundle != null ? assetBundle.LoadAsset<T>(assetName) : null;
    }

    string GetFinalABName(string abName)
    {
        if (!GameConfigs.AssetDir.Equals(abName))
        {
            abName = abName.ToLower();
            if (!abName.EndsWith(GameConfigs.ExtName))
            {
                abName = abName + GameConfigs.ExtName;
            }
            return abName;
        }

        return GameConfigs.AssetDir;
    }

    string[] GetDependencies(string abName)
    {
        if (!_dependencies.ContainsKey(abName))
            _dependencies[abName] = _assetBundleManifest.GetAllDependencies(abName);

        return _dependencies[abName];
    }

    public Coroutine LoadAssetBundle(string abName, Action<AssetBundle> action = null)
    {
        if (string.IsNullOrEmpty(abName))
            return null;

        abName = GetFinalABName(abName);

        AssetLoader request = new AssetLoader
        {
            sharpBundleFunc = action
        };

        return LoadAssetBundle(abName, request);
    }

    private Coroutine LoadAssetBundle(string abName, AssetLoader assetLoader)
    {
        AssetBundleInfo assetBundleInfo = null;

        if (_loadedAssetBundles.TryGetValue(abName, out assetBundleInfo) && assetLoader != null)
        {
            assetBundleInfo.ReferencedCount++;
            assetLoader.LoadComplete(assetBundleInfo.AssetBundle);
            assetLoader.Dispose();
            return null;
        }

        List<AssetLoader> requests = null;
        if (!_loadRequests.TryGetValue(abName, out requests))
        {
            requests = new List<AssetLoader>
            {
                assetLoader
            };
            _loadRequests.Add(abName, requests);
            return CoroutineAgent.StartCoroutine(OnLoadAssetBundle(abName, requests));
        }
        else
        {
            requests.Add(assetLoader);
        }
        return null;
    }

    IEnumerator OnLoadAssetBundle(string abName, List<AssetLoader> requests)
    {
        yield return CoroutineAgent.StartCoroutine(OnLoadAssetBundleImp(abName));

        AssetBundleInfo bundleInfo = GetLoadedAssetBundle(abName);
        if (bundleInfo == null)
        {
            _loadRequests.Remove(abName);
            Debug.LogError("LoadAsset Error--->>>" + abName);
            yield break;
        }

        for (int i = 0; i < requests.Count; i++)
        {
            requests[i].LoadComplete(bundleInfo.AssetBundle);
            requests[i].Dispose();
        }
    }

    IEnumerator OnLoadAssetBundleImp(string abName)
    {
        string url = UnityUtils.GetAssetURL(abName);

        AssetBundleCreateRequest download = null;
        if (GameConfigs.AssetDir.Equals(abName))
            download = AssetBundle.LoadFromFileAsync(url);
        else
        {
            string[] dependencies = GetDependencies(abName);
            if (dependencies.Length > 0)
            {
                for (int i = 0; i < dependencies.Length; i++)
                {
                    string depName = dependencies[i];
                    if (string.IsNullOrEmpty(depName))
                    {
                        Debug.LogError("加载依赖项为空：" + depName);
                        continue;
                    }

                    AssetBundleInfo bundleInfo = null;

                    if (_loadedAssetBundles.TryGetValue(depName, out bundleInfo))
                    {
                        bundleInfo.ReferencedCount++;
                        //this.Log("Modify RefrencedCount inc dep " + depName + " : " + bundleInfo.ReferencedCount.ToString());
                    }
                    else
                    {
                        if (!_loadRequests.ContainsKey(depName))
                        {
                            _loadRequests[depName] = new List<AssetLoader> {
                                new AssetLoader()
                            };
                            yield return CoroutineAgent.StartCoroutine(OnLoadAssetBundleImp(depName));
                        }
                        else
                        {
                            _loadRequests[depName].Add(new AssetLoader());
                        }
                    }
                    /* 原版本有错误
                    else if (!_loadRequests.ContainsKey(depName)) {
                        yield return StartCoroutine(OnLoadAssetBundle(depName, type));
                    }*/
                }
            }

            //this.Log("WWW.LoadFromCacheOrDownload " + url);
            download = AssetBundle.LoadFromFileAsync(url);
        }

        yield return download;

        _loadRequests.Remove(abName);

        AssetBundle assetObj = download.assetBundle;
        if (assetObj != null)
        {
            _loadedAssetBundles.Add(abName, new AssetBundleInfo(assetObj, abName));
        }
    }

    public Coroutine LoadAssetByPath<T>(string assetPath, Action<T> callback = null) where T : UObject
    {
        string abName = GetBundleNameWithAssetPath(assetPath);
        string assetName = Path.GetFileNameWithoutExtension(assetPath);

        return LoadAsset<T>(abName, assetName, callback);
    }

    /// <summary>
    /// 异步加载资源方法，推荐此方法
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="abName"></param>
    /// <param name="assetName"></param>
    /// <param name="callback"></param>
    public Coroutine LoadAsset<T>(string abName, string assetName, Action<T> callback = null) where T : UObject
    {
        return LoadAsset<T>(abName, new string[] { assetName }, (objs) =>
        {
            if (objs != null && objs.Length > 0 && callback != null)
            {
                callback(objs[0] as T);
            }
            else if (objs == null)
            {
                Debug.LogErrorFormat("加载AB:{0},assetName:{1}失败！", abName, assetName);
            }
        });
    }

    /// <summary>
    /// 载入素材
    /// </summary>
    public Coroutine LoadAsset<T>(string abName, string[] assetNames, Action<UObject[]> action = null) where T : UObject
    {
        if (string.IsNullOrEmpty(abName))
            return null;

        abName = GetFinalABName(abName);

        AssetLoader request = new AssetLoader
        {
            assetType = typeof(T),
            assetNames = assetNames,
            sharpFunc = action
        };

        return LoadAssetBundle(abName, request);
    }

    AssetBundleInfo GetLoadedAssetBundle(string abName)
    {
        AssetBundleInfo bundle = null;
        _loadedAssetBundles.TryGetValue(abName, out bundle);
        /* 加载的时候已经加载了依赖的话，可以不检测
        if (bundle == null) return null;
        
        // No dependencies are recorded, only the bundle itself is required.
        string[] dependencies = null;
        if (!_dependencies.TryGetValue(abName, out dependencies))
            return bundle;

        // Make sure all dependencies are loaded
        for (int i = 0; i < dependencies.Length; i++)
        {
            if (string.IsNullOrEmpty(dependencies[i]))
            {
                continue;
            }

            AssetBundleInfo dependentBundle;
            _loadedAssetBundles.TryGetValue(dependencies[i], out dependentBundle);
            if (dependentBundle == null)
            {
                return null;
            }
        }
        */
        return bundle;
    }

    public void UnloadAssetBundleByAssetPath(string assetPath, bool isThorough = false, int decCount = 1)
    {
        string abName = GetBundleNameWithAssetPath(assetPath);
        UnloadAssetBundle(abName, isThorough, decCount);
    }

    /// <summary>
    /// 此函数交给外部卸载专用，自己调整是否需要彻底清除AB
    /// </summary>
    /// <param name="abName"></param>
    /// <param name="isThorough"></param>
    public void UnloadAssetBundle(string abName, bool isThorough = false, int decCount = 1)
    {
        if (string.IsNullOrEmpty(abName))
            return;

        abName = GetFinalABName(abName);

        Debug.LogFormat("{0} assetbundle(s) in memory before unloading [{1}]", _loadedAssetBundles.Count, abName);

        if (UnloadAssetBundleInternal(abName, isThorough, decCount))
        {
            UnloadDependencies(abName, isThorough);
        }
        Debug.LogFormat("{0} assetbundle(s) in memory after unloading [{1}]", _loadedAssetBundles.Count, abName);
    }

    void UnloadDependencies(string abName, bool isThorough)
    {
        string[] dependencies = null;
        if (!_dependencies.TryGetValue(abName, out dependencies))
            return;

        // Loop dependencies.
        for (int i = 0; i < dependencies.Length; i++)
        {
            UnloadAssetBundleInternal(dependencies[i], isThorough);
        }
        _dependencies.Remove(abName);
    }

    bool UnloadAssetBundleInternal(string abName, bool isThorough, int decCount = 1)
    {
        AssetBundleInfo bundle = GetLoadedAssetBundle(abName);
        if (bundle == null) return false;

        bundle.ReferencedCount -= decCount;

        Debug.LogFormat("Modify RefrencedCount dec [{0}] decCount To : {1}", abName, bundle.ReferencedCount);

        if (bundle.ReferencedCount <= 0)
        {
            if (_loadRequests.ContainsKey(abName))
            {
                return false;     //如果当前AB处于Async Loading过程中，卸载会崩溃，只减去引用计数即可
            }
            bundle.AssetBundle.Unload(isThorough);
            _loadedAssetBundles.Remove(abName);

            Debug.LogFormat("[{0}] has been unloaded successfully", abName);

            return true;
        }

        return false;
    }

    public void UnloadAll()
    {
        var enumerator = this._loadedAssetBundles.GetEnumerator();

        while (enumerator.MoveNext())
        {
            if (null != enumerator.Current.Value.AssetBundle)
            {
                //to do if removed hot update or fix the hotupdate into scene
                enumerator.Current.Value.AssetBundle.Unload(true);
            }
        }

        this._dependencies.Clear();
        this._loadRequests.Clear();
        this._loadedAssetBundles.Clear();
    }
}