using Google.Protobuf;
using Google.Protobuf.Collections;
using UnityEngine;

public abstract class BaseConfigManager<T, K, V> : Singleton<T> where T : BaseConfigManager<T, K, V>, new() where V : IMessage
{
    public MapField<K, V> MapField;

    /// <summary>
    /// 配置加载同步方法
    /// </summary>
    public virtual void LoadConfig()
    {
        string assetName = string.Format("ClientProto/{0}", typeof(V).Name.ToLower());
        TextAsset asset = ResourcesManager.LoadSync<TextAsset>(assetName, ExtensionType.bytes);
        Deserialize(asset.bytes);
        AfterDeserialize();
    }

    /// <summary>
    /// 配置加载异步方法
    /// </summary>
    /// <returns></returns>
    public Coroutine LoadConfigAsync()
    {
        string assetName = string.Format("ClientProto/{0}", typeof(V).Name.ToLower());
        return ResourcesManager.LoadAsync<TextAsset>(assetName, (asset) =>
        {
            Deserialize(asset.bytes);
            AfterDeserialize();
        }, ExtensionType.bytes);
    }

    public abstract void Deserialize(byte[] bytes);

    public virtual void AfterDeserialize() { }

    public V GetItem(K id)
    {
        return MapField != null && MapField.ContainsKey(id) ? MapField[id] : default(V);
    }
}
