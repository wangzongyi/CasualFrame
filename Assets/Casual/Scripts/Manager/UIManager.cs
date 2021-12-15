using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class UISession : IDisposable
{
    public string PrefabName;
    public Action Callback;
    public object Data;
    public Type PageType;

    public void Init(Type pageType, object data, string prefabName, Action callback)
    {
        PageType = pageType;
        Data = data;
        PrefabName = prefabName;
        Callback = callback;
    }

    public void Dispose()
    {
        PageType = null;
        PrefabName = null;
        Callback = null;
        Data = null;
    }
}


public class UIManager : MonoSingleton<UIManager>
{
    public enum LoadState
    {
        loading,
        breakup,
        done
    }

    [SerializeField, LabelText("UICamera")]
    private Camera uiCamera;
    public static Camera UICamera { get { return Instance().uiCamera; } }

    [SerializeField, LabelText("LayerRoot")]
    private Canvas[] layerRoot = new Canvas[(int)UILayerType.MAX];

    //private Queue<UIBase>
    private UISession currentSession;
    private Stack<UISession> backSequence = new Stack<UISession>();
    private Dictionary<Type, LoadState> loadState = new Dictionary<Type, LoadState>();
    /// <summary>
    /// 存储有唯一标志的界面
    /// </summary>
    private Dictionary<Type, UIBase> existUnique = new Dictionary<Type, UIBase>();
    private Dictionary<int/*instanceID*/, UIBase> existExtra = new Dictionary<int, UIBase>();

    private const string UI_PREFAB_PATH = "{0}/Prefab/UI/{1}.prefab";

    public override void Init()
    {
        RegisterObjectPool();
        CalculateScreenSize();
    }

    private void RegisterObjectPool()
    {
        ObjectPoolManager.Instance().RegistCreater<UISession>(() =>
        {
            return new UISession();
        });
    }

    private void CalculateScreenSize()
    {
        UnityUtils.CalculateScreenSize();
        Debug.LogFormat("屏幕分辨率为：{0}。", GameConfigs.ScreenSizeLogic);
    }

    public Coroutine Open<T>(Action callback = null) where T : UIBase
    {
        return Open<T>(null, callback);
    }

    public Coroutine Open<T>(object data, Action callback = null) where T : UIBase
    {
        return Open(typeof(T), data, callback);
    }

    private Coroutine Open(Type pageType, object data, Action callback = null)
    {
        string assetPath = UIPathMapping.Instance().GetAssetPath(pageType);
        if (NullCheck.IsNotNull(assetPath, string.Format(LogMessage.ASSET_LOADED_FAILED, pageType.ToString())))
        {
            return OpenByFullPath(pageType, assetPath, data, callback, true);
        }

        return null;
    }

    private Coroutine OpenByFullPath(Type pageType, string fullPrefabName, object data, Action callback = null, bool pushCurrentToBack = true, Action closeAction = null)
    {
        if (IsExistUnique(pageType))
        {
            existUnique[pageType].InitData(data);
            existUnique[pageType].LoadAsset(callback);
        }
        else if (!loadState.ContainsKey(pageType) || loadState[pageType] != LoadState.loading)
        {
            loadState[pageType] = LoadState.loading;
            return ResourcesManager.LoadAsyncWithFullPath<GameObject>(fullPrefabName, (prefab) =>
            {
                LoadDone(pageType, prefab, fullPrefabName, data, callback, pushCurrentToBack);
                closeAction?.Invoke();
            });
        }

        return null;
    }

    private void LoadDone(Type pageType, GameObject prefab, string fullPrefabName, object data, Action callback, bool pushCurrentToBack)
    {
        //ui加载状态检测
        if (loadState.ContainsKey(pageType))
        {
            if (loadState[pageType] == LoadState.breakup)
                return;

            GameObject uiInstance = GameObjectPoolManager.Instance().FetchObject<UIPool>(prefab);
            UIBase uiPage = (UIBase)(uiInstance.GetComponent(pageType) ?? uiInstance.AddComponent(pageType));
            uiInstance.name = prefab.name;

            uiInstance.transform.SetParent(layerRoot[(int)uiPage.LayerType].transform, false);

            uiPage.InitData(data);
            uiPage.InitPrefabName(fullPrefabName);

            if (uiPage.Unique)
            {
                existUnique[pageType] = uiPage;
            }
            else
            {
                existExtra[uiPage.GetInstanceID()] = uiPage;
            }

            uiPage.LoadAsset(() =>
            {
                if (uiPage.LayerType == UILayerType.NORMAL)
                {
                    if (currentSession != null && IsExistUnique(currentSession.PageType))
                    {
                        Type currentPageType = currentSession.PageType;

                        if (GetPage(currentPageType).NeedPush && pushCurrentToBack)
                        {
                            backSequence.Push(currentSession);
                        }
                        else
                        {
                            ObjectPoolManager.Instance().ReturnObject(currentSession);
                        }
                        CloseUnique(currentPageType);
                    }

                    UISession uisession = ObjectPoolManager.Instance().FetchObject<UISession>();
                    uisession.Init(pageType, data, fullPrefabName, callback);

                    currentSession = uisession;
                }
                loadState[pageType] = LoadState.done;
                callback?.Invoke();
            });
        }
        else
        {
            Debug.LogErrorFormat("UI [{0}] 加载出错 !", fullPrefabName);
        }
    }

    /// <summary>
    /// 弹出队列中界面
    /// </summary>
    public void UIBackSequence(Action callback = null)
    {
        if (backSequence.Count > 0)
        {
            UISession session = backSequence.Pop();
            OpenByFullPath(session.PageType, session.PrefabName, session.Data, session.Callback, false, callback);
        }
    }

    private void CollectUI(UIBase uibase)
    {
        if (!uibase) return;
        GameObjectPoolManager.Instance().ReleaseObject(uibase.gameObject);
    }

    public void Close<T>(Action callback = null) where T : UIBase
    {
        if (!CloseUnique(typeof(T), callback))
        {
            CloseExtra(typeof(T), callback);
        }
    }

    public void Close(UIBase uibase, Action callback)
    {
        if (uibase.Unique)
        {
            CloseUnique(uibase.GetType(), callback);
        }
        else
        {
            CloseExtra(uibase, callback);
        }
    }

    private bool CloseUnique(Type pageType, Action callback = null)
    {
        if (loadState.ContainsKey(pageType))
        {
            loadState[pageType] = LoadState.breakup;

            UIBase uiInstance = GetPage(pageType);

            if (uiInstance)
            {
                existUnique.Remove(pageType);
                uiInstance.BeforeClose(callback);
                CollectUI(uiInstance);

                return true;
            }
        }
        return false;
    }

    private void CloseExtra(UIBase uibase, Action callback = null)
    {
        Type pageType = uibase.GetType();
        if (loadState.ContainsKey(pageType))
        {
            loadState[pageType] = LoadState.breakup;
            existExtra.Remove(uibase.GetInstanceID());
            uibase.BeforeClose(callback);
            CollectUI(uibase);
        }
    }

    private void CloseExtra(Type type, Action callback)
    {
        List<int> keys = new List<int>(existExtra.Keys);
        for (int index = existExtra.Count - 1; index >= 0; index--)
        {
            if (existExtra[keys[index]].GetType() == type)
            {
                CloseExtra(existExtra[keys[index]], callback);
            }
        }
    }

    public void Show<T>() where T : UIBase
    {
        Type pageType = typeof(T);
        if (IsExistUnique(pageType))
        {
            existUnique[typeof(T)].Show();
        }
    }

    public void Hide<T>() where T : UIBase
    {
        Type pageType = typeof(T);
        if (IsExistUnique(pageType))
        {
            existUnique[pageType].Hide();
        }
    }

    public void BatchShow()
    {
        foreach (UIBase ui in existUnique.Values)
        {
            if (ui) ui.Show();
        }
    }

    public void BatchHide()
    {
        foreach (UIBase ui in existUnique.Values)
        {
            if (ui) ui.Hide();
        }
    }

    public void BatchClose()
    {
        List<Type> pageTypes = new List<Type>(existUnique.Keys);
        foreach (Type pageType in pageTypes)
        {
            CloseUnique(pageType);
        }

        List<int> instanceIDs = new List<int>(existExtra.Keys);
        foreach (int instanceID in instanceIDs)
        {
            CloseExtra(existExtra[instanceID]);
        }

        backSequence.Clear();
        currentSession = null;
    }

    public bool IsExist<T>() where T : UIBase
    {
        return IsExistUnique(typeof(T)) || IsExistExtra(typeof(T));
    }

    private bool IsExistUnique(Type pageType)
    {
        return existUnique.ContainsKey(pageType) && existUnique[pageType];
    }

    private bool IsExistExtra(Type pageType)
    {
        foreach (var uiBase in existExtra.Values)
        {
            if (uiBase.GetType() == pageType)
                return true;
        }
        return false;
    }

    public bool IsExistPopup()
    {
        foreach (var type in existUnique.Keys)
        {
            if (type.BaseType.ToString().Contains("UIPopBase"))
                return true;
        }
        return false;
    }

    public T GetPage<T>() where T : UIBase
    {
        Type pageType = typeof(T);
        return GetPage(pageType) as T;
    }

    public UIBase GetPage(Type pageType)
    {
        return existUnique.ContainsKey(pageType) ? existUnique[pageType] : null;
    }

    public bool IsTransInScreen(Transform target, Vector3 offset)
    {
        Vector3 vecScreen = uiCamera.WorldToScreenPoint(target.position);
        vecScreen += offset;
        if (vecScreen.x > 0 && vecScreen.x < Screen.width && vecScreen.y > 0 && vecScreen.y < Screen.height)
            return true;
        return false;
    }

    public static void ScreenPointToUIPosition(Vector3 screenPoint, out Vector2 localPosition)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(Instance().layerRoot[0].rectTransform(), screenPoint, UICamera, out localPosition);
    }
}
