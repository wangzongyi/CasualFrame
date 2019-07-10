using System;

public class MonoContext : MonoSingleton<MonoContext>
{
    public static Action UpdateEvent;
    public static event Action LaterUpdateEvent;
    public static event Action FixedUpdateEvent;
    public static Action<bool> ApplicationPause;
    public static Action<bool> ApplicationFocus;
    public static Action ApplicationQuit;

    void Update()
    {
        UpdateEvent?.Invoke();
    }

    protected void LateUpdate()
    {
        LaterUpdateEvent?.Invoke();
    }

    protected void FixedUpdate()
    {
        FixedUpdateEvent?.Invoke();
    }

    protected override void OnApplicationQuit()
    {
        base.OnApplicationQuit();
        ApplicationQuit?.Invoke();
    }
    private void OnApplicationFocus(bool focus)
    {
        ApplicationFocus?.Invoke(focus);
    }
}
