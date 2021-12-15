using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

public abstract class BaseEditorScriptable<T> : SerializedScriptableObject where T : SerializedScriptableObject
{
    public static string ASSET_PATH = GameConfigs.GameRoot + "/Editor/{0}.asset";

    [SerializeField]
    private static T instance;

    internal static T Instance
    {
        get
        {
            if (!instance)
            {
                instance = AssetDatabase.LoadAssetAtPath<T>(string.Format(ASSET_PATH, typeof(T)));

                if (instance == null)
                {
                    instance = CreateInstance<T>();
                    AssetDatabase.CreateAsset(instance, string.Format(ASSET_PATH, typeof(T)));
                    AssetDatabase.Refresh();
                }
            }

            return instance;
        }
    }
}
