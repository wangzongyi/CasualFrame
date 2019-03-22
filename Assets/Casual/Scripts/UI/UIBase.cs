using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public enum UILayerType
{
    //背景层
    BACKGROUND = 0,
    //普通界面
    NORMAL = 1,
    // 固定窗口
    FIXED = 2,
    //弹窗
    POPUP = 3,
    //新手引导
    TUTORIAL = 4,
    //总数量
    MAX,
}

public class UIBase : BaseBehaviour
{
    public UILayerType LayerType;

    protected object data;
    protected string prefabName;
    protected Vector3 lastPosition;
    protected bool isVisible = true;

    protected override void VirtualAwake()
    {
        base.VirtualAwake();
        InitComponent();
    }

    // Use this for initialization
    void Start()
    {

    }

    protected virtual void InitComponent()
    {

    }

    internal virtual void Init(object data)
    {
        this.data = data;
    }

    internal virtual void Init(object data, string prefabName)
    {
        this.data = data;
        this.prefabName = prefabName;
    }

    internal virtual void Open(Action callback = null)
    {

    }

    internal virtual void Close(Action callback = null)
    {
        CloseAction(callback);
    }

    protected virtual void CloseAction(Action callback = null)
    {
        GameObjectPoolManager.Instance().ReturnObject(gameObject);
        UIManager.Instance().Remove(GetType());
        callback?.Invoke();
    }

    internal virtual void Show()
    {
        if (isVisible)
            return;

        transform.localPosition = lastPosition;
        isVisible = true;
    }

    internal virtual void Hide()
    {
        if (!isVisible)
            return;

        lastPosition = transform.localPosition;
        transform.localPosition = GameConfigs.DISABLE_POSITION;
        isVisible = false;
    }


    // Update is called once per frame
    void Update()
    {

    }

    protected void AddClick(Button button, UnityAction method)
    {
        button.onClick.AddListener(method);
    }

    protected override void UnloadAsset()
    {
        base.UnloadAsset();
        ResourcesManager.Instance().UnloadAsset(prefabName, 1);
    }
}
