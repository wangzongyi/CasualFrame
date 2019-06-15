using System;
using UnityEngine;

public class Singleton<T> where T : new()
{
    static T m_instance;
    static internal Type m_type = typeof(T);
    public static T Instance()
    {
        if (m_instance == null)
        {
            m_instance = Activator.CreateInstance<T>();
            (m_instance as Singleton<T>).Init();
        }
        return m_instance;
    }

    virtual protected void Init() { }

    public void Release()
    {
        m_instance = default(T);
    }
}

public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
{
    protected static T m_instance = null;
    private static readonly object _lock = new object();

    public static bool IsApplicationQuit = false;

    [SerializeField]
    protected bool m_dontDestroyOnLoad = true;

    public static T Instance()
    {
        lock(_lock)
        {
            if (m_instance == null && !IsApplicationQuit)
            {
                m_instance = FindObjectOfType(typeof(T)) as T;
                if (m_instance == null)
                {
                    m_instance = new GameObject("Singleton of " + typeof(T).Name, typeof(T)).GetComponent<T>();
                    m_instance.Init();
                }
            }
        }
        
        return m_instance;
    }

    private void Awake()
    {
        if (m_instance == null)
        {
            m_instance = this as T;
            m_instance.Init();
        }
        if (!m_instance.transform.parent && m_dontDestroyOnLoad) DontDestroyOnLoad(m_instance);
        OnAwake();
    }

    protected virtual void OnAwake() { }

    public virtual void Init() { }

    protected virtual void OnApplicationQuit()
    {
        IsApplicationQuit = true;
    }
}

