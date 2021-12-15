using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 对象池基类
/// </summary>
public class GameObjectPool
{
    public string Name { private set; get; }
    public int ReferenceCount = 0;
    Queue<GameObject> objQueue = new Queue<GameObject>();

    /// <summary>
    /// 存入实例
    /// </summary>
    /// <param name="inst"></param>
    public void Enqueue(GameObject inst, Transform poolRoot)
    {
        ReferenceCount -= 1;
        objQueue.Enqueue(inst);
        EnqueueAction(inst, poolRoot);
        Name = poolRoot.name;
    }

    /// <summary>
    /// 取出实例
    /// </summary>
    /// <param name="prefab"></param>
    /// <param name="parent"></param>
    /// <returns></returns>
    public GameObject Dequeue(Object prefab, Transform parent)
    {
        ReferenceCount += 1;
        return objQueue.Count > 0 ? objQueue.Dequeue() : (GameObject)Object.Instantiate(prefab, parent, false);
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
            Object.DestroyImmediate(objQueue.Dequeue());
        }
    }

    public void DequeueUnUsed(System.Action<GameObject> dequeueEvevryTime)
    {
        while (objQueue.Count > ReferenceCount)
        {
            GameObject instance = objQueue.Dequeue();
            dequeueEvevryTime(instance);
            Object.DestroyImmediate(instance);
        }
    }

    protected virtual void EnqueueAction(GameObject inst, Transform poolRoot)
    {
        inst.transform.SetParent(poolRoot, false);
        inst.transform.position = Vector3.one * 10000;
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
        inst.transform.localScale = prefab.transform.localScale;

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
    protected override void EnqueueAction(GameObject inst, Transform poolRoot)
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
