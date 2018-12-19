using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 对象池基类
/// </summary>
public class GameObjectPool
{
    Queue<GameObject> objQueue = new Queue<GameObject>();

    /// <summary>
    /// 如果池内已经有了该实例，直接返回
    /// </summary>
    /// <param name="go"></param>
    public void Enqueue(GameObject go)
    {
        if (objQueue.Contains(go))
            return;

        objQueue.Enqueue(go);
    }

    public GameObject Dequeue()
    {
        GameObject instObject = null;

        if (objQueue.Count > 0)
            instObject = objQueue.Dequeue();

        return instObject;
    }

    public bool Contains(GameObject go)
    {
        return objQueue.Contains(go);
    }

    public int Count { get { return objQueue.Count; } }

    public void ClearThisPool()
    {
        while (objQueue.Count > 0)
        {
            Object.Destroy(objQueue.Dequeue());
        }
    }

    public virtual void EnqueueAction(GameObject inst, Transform poolRoot)
    {
        inst.transform.position = Vector3.one * 10000;
        inst.transform.SetParent(poolRoot, false);
    }

    /// <summary>
    /// 取出实例时调用
    /// </summary>
    /// <param name="inst"></param>
    /// <param name="prefab"></param>
    /// <param name="active"></param>
    public virtual void DequeueAction(GameObject inst, GameObject prefab, bool active)
    {
        if (!active)
            inst.transform.position = GameConfigs.DISABLE_POSITION;
        else
            inst.transform.localPosition = prefab.transform.localPosition;

        inst.transform.rotation = prefab.transform.rotation;
        inst.transform.localScale = prefab.transform.lossyScale;

        if (active && !inst.activeSelf)
        {
            inst.SetActive(active);
        }
    }
}

public class NullPool : GameObjectPool
{

}

public class UIPool : GameObjectPool
{
    public override void EnqueueAction(GameObject inst, Transform poolRoot)
    {
        inst.transform.localPosition = GameConfigs.DISABLE_POSITION;
        inst.transform.SetParent(poolRoot, false);
    }

    public override void DequeueAction(GameObject inst, GameObject prefab, bool active)
    {
        inst.transform.localPosition = prefab.transform.localPosition;
        inst.transform.localRotation = prefab.transform.localRotation;
        inst.transform.localScale = prefab.transform.localScale;

        if (active && !inst.activeSelf)
        {
            inst.SetActive(active);
        }

        RectTransform prefabRect = prefab.GetComponent<RectTransform>();
        RectTransform instRect = inst.GetComponent<RectTransform>();
        if (prefabRect != null && instRect != null)
        {
            instRect.pivot = prefabRect.pivot;
            instRect.anchorMin = prefabRect.anchorMin;
            instRect.anchorMax = prefabRect.anchorMax;
            instRect.sizeDelta = prefabRect.sizeDelta;
            instRect.anchoredPosition = prefabRect.anchoredPosition;
        }
    }
}
