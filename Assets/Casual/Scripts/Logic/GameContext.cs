using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameContext : MonoSingleton<GameContext>
{
    public static Action UpdateEvent;
    public static Action ApplicationQuitEvent;
    public static Action<bool> ApplicationPause;
    public static Action<bool> ApplicationFocus;

    [ContextMenu("start")]
    // Use this for initialization
    void Start()
    {
        UIManager.Instance().Open<UIBackground>("UIBackground");
        UIManager.Instance().Open<UIMain>("UIMain");

        TextAsset asset = Resources.Load<TextAsset>("ClientProto/GoodsInfo");
        Debug.Log(asset.bytes.Length);
        GoodsInfoMapManager.Instance().LoadConfig();

        foreach (var goodsInfo in GoodsInfoMapManager.Instance().map)
        {
            Debug.Log(goodsInfo.Key + "|" + goodsInfo.Value.Name);
        }
    }

    void Update()
    {
        UpdateEvent?.Invoke();
    }

    private void OnApplicationQuit()
    {
        ApplicationQuitEvent?.Invoke();
    }
    private void OnApplicationFocus(bool focus)
    {
        ApplicationFocus?.Invoke(focus);
    }
}
