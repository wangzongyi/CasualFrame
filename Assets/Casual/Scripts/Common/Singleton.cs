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
    private static T m_instance = null;
    protected static bool IsApplicationQuit = false;

    public static T Instance()
    {
        if (m_instance == null)
        {
            m_instance = GameObject.FindObjectOfType(typeof(T)) as T;
            if (m_instance == null)
            {
                m_instance = new GameObject("Singleton of " + typeof(T).ToString(), typeof(T)).GetComponent<T>();
                m_instance.Init();
            }
            DontDestroyOnLoad(m_instance);
        }
        return m_instance;
    }

    private void Awake()
    {
        if (m_instance == null)
        {
            m_instance = this as T;
        }
    }

    public virtual void Init() { }

    private void OnApplicationQuit()
    {
        m_instance = null;
        IsApplicationQuit = true;
    }
}

