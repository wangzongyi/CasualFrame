using Google.Protobuf;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseConfigManager<T, K, V> : Singleton<T> where T : new() where V : IMessage
{
    protected string assetName;
    public Dictionary<K, V> map;

    public void LoadConfig()
    {
        TextAsset asset = Resources.Load<TextAsset>(assetName);
        map = Proto.GoodsInfoMap.Descriptor.Parser.ParseFrom(asset.bytes) as Dictionary<K, V>;
    }

    public V GetItem(K id)
    {
        return map != null && map.ContainsKey(id) ? map[id] : default(V);
    }
}
