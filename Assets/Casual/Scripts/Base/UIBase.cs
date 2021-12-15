using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
using UnityEditor;
#endif

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
    [HideInInspector]
    TUTORIAL_MASK = 4,
    [LabelText("新手引导")]
    TUTORIAL = 5,
    [LabelText("顶层")]
    TOP = 6,
    [LabelText("闪屏")]
    SPLASH = 7,
    [HideInInspector]
    MAX,
}

abstract public class UIBase<T> : UIBase
{
    protected T data;

    internal override void InitData(object data)
    {
        this.data = data != null ? (T)data : default(T);
    }
}

abstract public class UIElement : BaseBehaviour
{
    /// <summary>
    /// 给按钮增加监听
    /// </summary>
    /// <param name="button"></param>
    /// <param name="method"></param>
    protected void AddClick(Button button, UnityAction method, bool once = false)
    {
        button.onClick.AddListener(method);
        if (once)
        {
            button.onClick.AddListener(() =>
            {
                button.onClick.RemoveListener(method);
            });
        }
    }

    protected void RemoveClick(Button button)
    {
        button.onClick.RemoveAllListeners();
    }
}

[DisallowMultipleComponent]
abstract public class UIBase : UIElement
{
    [LabelText("唯一")]
    public bool Unique = true;

    [LabelText("界面层级")]
    public UILayerType LayerType;

    [LabelText("界面堆栈"), ShowIf("LayerType", UILayerType.NORMAL)]
    public bool NeedPush = false;

    protected string prefabName;
    protected Vector3 lastPosition;
    protected bool isVisible = true;

    protected float uiStartTime;
    protected virtual string PageName { get; } = "";
    protected virtual string PageParam { get; set; } = "NONE";

#if UNITY_EDITOR
    private bool IsMapping
    {
        get
        {
            return UIPathMapping.Instance().IsMapping(GetType());
        }
    }

    [Button("映射资源路径", ButtonSizes.Medium), HideIf("IsMapping")]
    private void Mapping()
    {
        string path = AssetDatabase.GetAssetPath(PrefabUtility.GetCorrespondingObjectFromSource(this));
        if (!string.IsNullOrEmpty(path))
        {
            UIPathMapping.Instance().AppendMapping(GetType(), path);
        }
        else
        {
            Debug.LogErrorFormat("无法在文件夹内找到{0}的预制体!", GetType().ToString());
        }
    }
#endif

    internal virtual void InitData(object data) { }

    internal virtual void LoadAsset(Action callback)
    {
        InitAssetPathList();

        if (LoadAssetPathList != null && LoadAssetPathList.Count > 0)
        {
            ResourcesManager.LoadAsyncWithExtensionPath(LoadAssetPathList, (asset, index) =>
            {
                BrocastEvent(EventEnum.UPDATE_LOADING_PROGRESS, (index + 1.0f) / LoadAssetPathList.Count);
            }, (assets) =>
            {
                OnOpen(callback);
            }, this);
        }
        else
        {
            OnOpen(callback);
            BrocastEvent(EventEnum.UPDATE_LOADING_PROGRESS, 1f);
        }
    }

    internal void InitPrefabName(string prefabName)
    {
        this.prefabName = prefabName;
    }

    /// <summary>
    /// 打开此界面会回调此方法
    /// </summary>
    /// <param name="callback"></param>
    internal virtual void OnOpen(Action callback = null)
    {
        callback?.Invoke();
        uiStartTime = Time.realtimeSinceStartup;
    }

    /// <summary>
    /// 关闭此界面
    /// </summary>
    /// <param name="callback"></param>
    public void Close(Action callback = null)
    {
        switch (LayerType)
        {
            case UILayerType.NORMAL:
                UIManager.Instance().UIBackSequence(callback);
                break;
            default:
                UIManager.Instance().Close(this, callback);
                break;
        }
    }

    internal virtual void Clear(){ }

    /// <summary>
    /// 关闭此界面后调用
    /// </summary>
    /// <param name="callback"></param>
    internal virtual void BeforeClose(Action callback = null)
    {
        Clear();
        ReturnObjects();
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

    protected override void UnloadAsset()
    {
        base.UnloadAsset();
        ResourcesManager.UnloadAsset<GameObject>(prefabName, 1);
    }
}
