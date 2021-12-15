using System;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class UIPathMapping : SerializedScriptableObject
{
    [SerializeField, DictionaryDrawerSettings(KeyLabel = "类型", ValueLabel = "路径")]
    private Dictionary<string/*Type*/, string/*Path*/> Mapping = new Dictionary<string, string>();

    private static UIPathMapping instance;

    public static UIPathMapping Instance()
    {
        if (instance == null)
#if UNITY_EDITOR
            instance = ResourcesManager.LoadAssetAtPath<UIPathMapping>("Config/UIPathMapping", ExtensionType.asset);
#else
            instance = ResourcesManager.LoadSync<UIPathMapping>("Config/UIPathMapping", ExtensionType.asset);
#endif

        return instance;
    }

#if UNITY_EDITOR
    [Button("批处理映射", ButtonSizes.Gigantic)]
    public void BatchMapping()
    {
        Mapping.Clear();
        List<string> paths = UnityUtils.Recursive("Assets/Casual/Bundle/Prefab/UI"); ;

        for (int index = 0, len = paths.Count; index < len; index++)
        {
            UIBase uiBase = AssetDatabase.LoadAssetAtPath<UIBase>(paths[index]);
            if (uiBase)
            {
                AppendMapping(uiBase.GetType(), paths[index]);
            }
            EditorUtility.DisplayProgressBar("Hold...", string.Format("{0}/{1}", index, len), index * 1.0f / len);
        }
        EditorUtility.ClearProgressBar();
        EditorUtility.SetDirty(this);
    }
    public bool IsMapping(Type uibase)
    {
        return Mapping != null && Mapping.ContainsKey(uibase.ToString());
    }

    public void AppendMapping(Type uibase, string path)
    {
        if (Mapping == null)
            Mapping = new Dictionary<string, string>();

        if (Mapping.ContainsKey(uibase.ToString()))
        {
            Debug.LogErrorFormat("已存在类型:{0}，请检查是否正常！", uibase);
        }
        else
        {
            Mapping[uibase.ToString()] = path;
        }
        EditorUtility.SetDirty(this);
    }
#endif

    public string GetAssetPath(Type uibase)
    {
        string type = uibase.ToString();
        return Mapping.ContainsKey(type) ? Mapping[type] : null;
    }
}
