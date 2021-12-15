using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseScriptObjectManager<T, V> : Singleton<T>, ILoadConfig where T : BaseScriptObjectManager<T, V>, new() where V : class, new()
{
    public Dictionary<string, V> MapField = new Dictionary<string, V>();
    protected abstract string assetPath { get; }
    public virtual void LoadConfig() { }

    public V GetItem(string key)
    {
        if (!MapField.ContainsKey(key))
        {
            string path = assetPath + "/" + key;
            BaseScriptObject<V> scriptObject = ResourcesManager.LoadSync<BaseScriptObject<V>>(path, ExtensionType.asset);
            MapField[key] = scriptObject.Value;
        }

        return MapField[key];
    }

    public virtual Coroutine LoadConfigAsync()
    {
        return CoroutineAgent.StartCoroutine(CoLoadConfigAsyc());
    }

    private IEnumerator CoLoadConfigAsyc()
    {
        List<string> assets = null;

        yield return ResourcesManager.LoadAsync<TextAsset>(assetPath + "s", (valueList) =>
        {
            assets = LitJson.JsonMapper.ToObject<List<string>>(valueList.text);
        }, ExtensionType.json);

        foreach (string asset in assets)
        {
            yield return ResourcesManager.LoadAsync<BaseScriptObject<V>>(assetPath + "/" + asset, (scriptObject) =>
            {
                MapField[asset] = scriptObject.Value;
            }, ExtensionType.asset);
        }
    }
}