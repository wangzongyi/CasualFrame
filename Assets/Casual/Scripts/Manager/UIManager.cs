using System;
using System.Collections.Generic;
using UnityEngine;

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
    [SerializeField]
    private Camera uiCamera;
    public static Camera UICamera { get { return Instance().uiCamera; } }

    [SerializeField]
    private Canvas[] layerRoot = new Canvas[(int)UILayerType.MAX];

    private HashSet<Type> loadingUI = new HashSet<Type>();
    private Dictionary<Type, UIBase> allPages = new Dictionary<Type, UIBase>();
    //private Queue<UIBase>
    private UISession currentSession;
    private Stack<UISession> backSequence = new Stack<UISession>();

    private const string UI_PREFAB_PATH = "{0}/Prefab/UI/{1}.prefab";

    public override void Init()
    {
        ObjectPoolManager.Instance().RegistCreater<UISession>(() =>
        {
            return new UISession();
        });
    }

    public void Open<T>(Action callback = null) where T : UIBase
    {
        Open<T>(typeof(T).Name, callback);
    }

    public void Open<T>(string prefabName) where T : UIBase
    {
        Open<T>(prefabName, null, null);
    }

    public void Open<T>(string prefabName, Action callback) where T : UIBase
    {
        Open<T>(prefabName, null, callback);
    }

    public void Open<T>(string prefabName, object data, Action callback = null) where T : UIBase
    {
        Type pageType = typeof(T);
        Open(pageType, prefabName, data, callback, true);
    }

    private void Open(Type pageType, string prefabName, object data, Action callback = null, bool pushCurrentToBack = true)
    {
        if (IsExist(pageType))
        {
            allPages[pageType].InitData(data);
            allPages[pageType].OnOpen(callback);
        }
        else if (!loadingUI.Contains(pageType))
        {
            loadingUI.Add(pageType);
            prefabName = string.Format(UI_PREFAB_PATH, GameConfigs.AssetRoot, prefabName);
            ResourcesManager.LoadAsyncWithFullPath<GameObject>(prefabName, (prefab) =>
            {
                LoadDone(pageType, prefab, prefabName, data, callback, pushCurrentToBack);
            });
        }
    }

    private void OpenByFullPath(Type pageType, string fullPrefabName, object data, Action callback = null, bool pushCurrentToBack = true, Action closeAction = null)
    {
        if (IsExist(pageType))
        {
            allPages[pageType].InitData(data);
            allPages[pageType].OnOpen(callback);
        }
        else if (!loadingUI.Contains(pageType))
        {
            loadingUI.Add(pageType);
            ResourcesManager.LoadAsyncWithFullPath<GameObject>(fullPrefabName, (prefab) =>
            {
                LoadDone(pageType, prefab, fullPrefabName, data, callback, pushCurrentToBack);
                closeAction?.Invoke();
            });
        }
    }

    private void LoadDone(Type pageType, GameObject prefab, string fullPrefabName, object data, Action callback, bool pushCurrentToBack)
    {
        GameObject uiInstance = GameObjectPoolManager.Instance().FetchObject<UIPool>(prefab);
        UIBase uiPage = (UIBase)(uiInstance.GetComponent(pageType) ?? uiInstance.AddComponent(pageType));

        uiInstance.transform.SetParent(layerRoot[(int)uiPage.LayerType].transform, false);

        uiPage.InitData(data);
        uiPage.InitPrefabName(fullPrefabName);

        allPages[pageType] = uiPage;
        loadingUI.Remove(pageType);

        uiPage.OnOpen(callback);

        if (uiPage.LayerType == UILayerType.NORMAL)
        {
            if (currentSession != null && IsExist(currentSession.PageType))
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
                Close(currentPageType);
            }

            UISession uisession = ObjectPoolManager.Instance().FetchObject<UISession>();
            uisession.Init(pageType, data, fullPrefabName, callback);

            currentSession = uisession;
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
        GameObjectPoolManager.Instance().ReturnObject(uibase.gameObject);
    }

    public void Close<T>(Action callback = null) where T : UIBase
    {
        Close(typeof(T), callback);
    }

    public void Close(Type pageType, Action callback = null)
    {
        if (IsExist(pageType))
        {
            UIBase uiInstance = GetPage(pageType);

            if (uiInstance)
            {
                uiInstance.CloseAction(callback);
                CollectUI(uiInstance);
                Remove(pageType);
            }
        }
    }

    public void Show<T>() where T : UIBase
    {
        Type pageType = typeof(T);
        if (IsExist(pageType))
        {
            allPages[typeof(T)].Show();
        }
    }

    public void Hide<T>() where T : UIBase
    {
        Type pageType = typeof(T);
        if (IsExist(pageType))
        {
            allPages[pageType].Hide();
        }
    }

    public void BatchShow()
    {
        foreach (UIBase ui in allPages.Values)
        {
            if (ui) ui.Show();
        }
    }

    public void BatchHide()
    {
        foreach (UIBase ui in allPages.Values)
        {
            if (ui) ui.Hide();
        }
    }

    public void BatchClose()
    {
        List<Type> pageTypes = new List<Type>(allPages.Keys);
        foreach (Type pageType in pageTypes)
        {
            Close(pageType);
        }
        backSequence.Clear();
        currentSession = null;
    }

    public void Remove(Type pageType)
    {
        allPages.Remove(pageType);
    }

    public bool IsExist<T>() where T : UIBase
    {
        Type pageType = typeof(T);
        return IsExist(pageType);
    }

    public bool IsExist(Type pageType)
    {
        return allPages.ContainsKey(pageType) && allPages[pageType];
    }

    private T GetPage<T>() where T : UIBase
    {
        Type pageType = typeof(T);
        return GetPage(pageType) as T;
    }

    private UIBase GetPage(Type pageType)
    {
        return allPages.ContainsKey(pageType) ? allPages[pageType] : null;
    }
}
