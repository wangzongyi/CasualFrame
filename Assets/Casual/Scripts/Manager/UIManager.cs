using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoSingleton<UIManager>
{
    [SerializeField]
    Canvas[] layerRoot = new Canvas[(int)UILayerType.MAX];

    HashSet<Type> _loading = new HashSet<Type>();
    Dictionary<Type, UIBase> _allPages = new Dictionary<Type, UIBase>();

    const string UI_PREFAB_PATH = "{0}/Prefab/UI/{1}.prefab";

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
        if (IsExist(pageType))
        {
            _allPages[pageType].Init(data);
            _allPages[pageType].Open(callback);
        }
        else if (!_loading.Contains(pageType))
        {
            _loading.Add(pageType);
            prefabName = string.Format(UI_PREFAB_PATH, GameConfigs.AssetRoot, prefabName);
            ResourcesManager.Instance().LoadAsyncWithFullPath<GameObject>(prefabName, (prefab) =>
            {
                LoadDone<T>(prefab, prefabName, data, callback);
            });
        }
    }

    void LoadDone<T>(GameObject prefab, string prefabName, object data, Action callback) where T : UIBase
    {
        Type pageType = typeof(T);

        GameObject uiInstance = GameObjectPoolManager.Instance().FetchObject<UIPool>(prefab);
        UIBase uiPage = uiInstance.GetComponent<T>() ?? uiInstance.AddComponent<T>();

        uiInstance.transform.SetParent(layerRoot[(int)uiPage.LayerType].transform, false);

        uiPage.Init(data, prefabName);
        uiPage.Open(callback);

        _allPages[pageType] = uiPage;
        _loading.Remove(pageType);
    }

    public void Close<T>() where T : UIBase
    {
        Type pageType = typeof(T);
        Close(pageType);
    }

    public void Close(Type pageType)
    {
        if (IsExist(pageType))
        {
            _allPages[pageType].Close();
        }
    }

    public void Show<T>() where T : UIBase
    {
        Type pageType = typeof(T);
        if(IsExist(pageType))
        {
            _allPages[typeof(T)].Show();
        }
    }

    public void Hide<T>() where T : UIBase
    {
        Type pageType = typeof(T);
        if (IsExist(pageType))
        {
            _allPages[pageType].Hide();
        }
    }

    public void BatchShow()
    {
        foreach (UIBase ui in _allPages.Values)
        {
            if (ui) ui.Show();
        }
    }

    public void BatchHide()
    {
        foreach (UIBase ui in _allPages.Values)
        {
            if (ui) ui.Hide();
        }
    }

    public void BatchClose()
    {
        List<Type> pageTypes = new List<Type>(_allPages.Keys);
        foreach(Type pageType in pageTypes)
        {
            Close(pageType);
        }
    }

    public void Remove<T>() where T : UIBase
    {
        Remove(typeof(T));
    }

    public void Remove(Type pageType)
    {
        _allPages.Remove(pageType);
    }

    public bool IsExist<T>() where T : UIBase
    {
        Type pageType = typeof(T);
        return IsExist(pageType);
    }

    public bool IsExist(Type pageType)
    {
        return _allPages.ContainsKey(pageType) && _allPages[pageType];
    }

    public T GetPage<T>() where T : UIBase
    {
        Type pageType = typeof(T);
        return _allPages.ContainsKey(pageType) ? _allPages[pageType] as T : null;
    }
}
