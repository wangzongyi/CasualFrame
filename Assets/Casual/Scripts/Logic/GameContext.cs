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
