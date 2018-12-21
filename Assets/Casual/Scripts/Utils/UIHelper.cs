using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class UIHelper
{
    public static void SetActive(this Transform trans, bool isActive)
    {
        if (trans)
        {
            SetActive(trans.gameObject, isActive);
        }
    }

    public static void SetActive(this Component comp, bool isActive)
    {
        if (comp)
        {
            SetActive(comp.gameObject, isActive);
        }
    }

    public static void SetActive(GameObject obj, bool isActive)
    {
        if (obj && obj.activeSelf != isActive)
        {
            obj.SetActive(isActive);
        }
    }

    static public GameObject GetChild(this GameObject parent, int index = 0)
    {
        return GetChild(parent.transform, index);
    }
    static public GameObject GetChild(this Transform parent, int index = 0)
    {
        return parent.GetChild(index).gameObject;
    }
    static public GameObject GetChild(this Component parent, string name)
    {
        return GetChild(parent.transform, name);
    }

    static public GameObject GetChild(this GameObject parent, string name)
    {
        return GetChild(parent.transform, name);
    }

    static public GameObject GetChild(this Transform parent, string name)
    {
        if (parent)
        {
            Transform child = parent.Find(name);
            if (child)
            {
                return child.gameObject;
            }
        }
        return null;
    }

    static public void GetChilds(GameObject parent, string[] names, GameObject[] objs)
    {
        if (names.Length != objs.Length)
        {
            Debug.LogErrorFormat("数量不匹配，请检查！");
            return;
        }

        for (int index = 0, len = names.Length; index < len; index++)
        {
            objs[index] = GetChild(parent, names[index]);
        }
    }

    public static T GetChild<T>(this Component parent, string name) where T : Component
    {
        return GetChild<T>(parent.transform, name);
    }

    public static T GetChild<T>(this GameObject parent, string name) where T : Component
    {
        return GetChild<T>(parent.transform, name);
    }

    public static T GetChild<T>(this Transform parent, string name) where T : Component
    {
        Transform child = parent.Find(name);
        if (child != null && parent != null)
        {
            return child.GetComponent<T>();
        }
        Debug.LogErrorFormat("找不到子物体:{0}, 或者找不到子物体组件:{1}！", name, typeof(T));

        return null;
    }

    static public void HideExtraChild(GameObject parent, int startIndex)
    {
        HideExtraChild(parent.transform, startIndex);
    }

    static public void HideExtraChild(Component parent, int startIndex)
    {
        HideExtraChild(parent.transform, startIndex);
    }

    static void HideExtraChild(Transform parent, int startIndex)
    {
        for (int len = parent.transform.childCount; startIndex < len; startIndex++)
        {
            GameObject extra = parent.GetChild(startIndex).gameObject;
            extra.SetActive(false);
        }
    }

    public static GameObject AddChildMtp(Component parent, int index, GameObject prefab, string goName = "")
    {
        return AddChildMtp(parent.transform, index, prefab, goName);
    }

    public static GameObject AddChildMtp(GameObject parent, int index, GameObject prefab, string goName = "")
    {
        return AddChildMtp(parent.transform, index, prefab, goName);
    }

    static GameObject AddChildMtp(Transform parent, int index, GameObject prefab, string goName = "")
    {
        GameObject ob = parent.transform.childCount > index ?
            parent.transform.GetChild(index).gameObject : AddChild(parent, prefab, goName);

        ob.SetActive(true);

        return ob;
    }

    static public GameObject AddChild(GameObject parent, GameObject pref, string name = "")
    {
        return AddChild<NullPool>(parent.transform, pref, name);
    }

    public static GameObject AddChild(Transform parent, GameObject prefab, string name = "")
    {
        return AddChild<NullPool>(parent, prefab, name);
    }

    public static GameObject AddChild<T>(GameObject parent, GameObject prefab, string name = "") where T : GameObjectPool
    {
        return AddChild<T>(parent.transform, prefab, name);
    }

    static GameObject AddChild<T>(Transform parent, GameObject pref, string name = "") where T : GameObjectPool
    {
        GameObject go = typeof(NullPool) == typeof(T) ? GameObject.Instantiate(pref, parent, false) : GameObjectPoolManager.Instance().FetchObject(pref, parent);

        if (go != null && !string.IsNullOrEmpty(name))
        {
            go.name = name;
        }
        return go;
    }

    public static void SetText(this Text text, object content)
    {
        text.text = content.ToString();
    }

    public static void SetText(this Text text, string format, params object[] args)
    {
        text.text = string.Format(format ?? "", args);
    }
}