using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public enum UILayerType
{
    [LabelText("背景层")]
    BACKGROUND = 0,
    [LabelText("普通层")]
    NORMAL = 1,
    [LabelText("固定层")]
    FIXED = 2,
    [LabelText("弹出层")]
    POPUP = 3,
    [LabelText("顶层")]
    TOP = 4,
    [LabelText("新手引导")]
    TUTORIAL = 5,
    [HideInInspector]
    MAX,
}

abstract public class UIBase<T> : UIBase
{
    protected T data;

    internal override void InitData(object data)
    {
        this.data = (T)data;
    }
}

[DisallowMultipleComponent]
abstract public class UIBase : BaseBehaviour
{
    [LabelText("界面层级")]
    public UILayerType LayerType;

    [LabelText("界面堆栈"), ShowIf("LayerType", UILayerType.NORMAL)]
    public bool NeedPush = false;

    protected string prefabName;
    protected Vector3 lastPosition;
    protected bool isVisible = true;

    protected override void OnAwake()
    {
        base.OnAwake();
        InitComponent();
    }

    /// <summary>
    /// 资源初始化，子类一般需要实现
    /// </summary>
    protected virtual void InitComponent() { }

    internal virtual void InitData(object data) { }

    internal void InitPrefabName(string prefabName)
    {
        this.prefabName = prefabName;
    }

    /// <summary>
    /// 打开此界面会回调此方法
    /// </summary>
    /// <param name="callback"></param>
    internal virtual void OnOpen(Action callback = null) { callback?.Invoke();}

    /// <summary>
    /// 关闭此界面
    /// </summary>
    /// <param name="callback"></param>
    protected virtual void Close(Action callback = null)
    {
        UIManager.Instance().Close(GetType(), callback);
    }

    /// <summary>
    /// 关闭此界面后调用
    /// </summary>
    /// <param name="callback"></param>
    internal virtual void CloseAction(Action callback = null)
    {
        GameObjectPoolManager.Instance().ReturnObject(gameObject);
        callback?.Invoke();
    }

    /// <summary>
    /// 如果之前隐藏了界面，用此方法重新打开
    /// </summary>
    internal virtual void Show()
    {
        if (isVisible)
            return;

        transform.localPosition = lastPosition;
        isVisible = true;
    }

    /// <summary>
    /// 隐藏当前界面，并将界面压入堆栈
    /// </summary>
    internal virtual void Hide()
    {
        if (!isVisible)
            return;

        lastPosition = transform.localPosition;
        transform.localPosition = GameConfigs.DISABLE_POSITION;
        isVisible = false;
    }

    /// <summary>
    /// 给按钮增加监听
    /// </summary>
    /// <param name="button"></param>
    /// <param name="method"></param>
    protected void AddClick(Button button, UnityAction method)
    {
        button.onClick.AddListener(method);
    }

    protected override void UnloadAsset()
    {
        base.UnloadAsset();
        ResourcesManager.UnloadAsset(prefabName, 1);
    }
}
