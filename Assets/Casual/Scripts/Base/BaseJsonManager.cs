using LitJson;
using UnityEngine;

public abstract class BaseJsonManager<T, K> : Singleton<T>, ILoadConfig where T : BaseJsonManager<T, K>, new()
{
    public K Config;
    protected abstract string assetPath { get; }
    public virtual void LoadConfig() {
        string value = ResourcesManager.LoadSync<TextAsset>(assetPath, ExtensionType.json).text;
        Config = JsonMapper.ToObject<K>(value);
    }

    public virtual Coroutine LoadConfigAsync()
    {
        return ResourcesManager.LoadAsync<TextAsset>(assetPath, (config) =>
        {
            Config = JsonMapper.ToObject<K>(config.text);
        }, ExtensionType.json);
    }
}
