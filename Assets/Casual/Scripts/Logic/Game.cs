using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoSingleton<Game>
{
    public Action UpdateEvent;
    public Action ApplicationQuitEvent;
    public Action<bool> ApplicationPause;
    public Action<bool> ApplicationFocus;

    // Use this for initialization
    void Start()
    {

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
