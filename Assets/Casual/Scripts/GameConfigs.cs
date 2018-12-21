using UnityEngine;
using System.Collections;

public enum PublishMode
{
    Release,
    Debug,
}

/// <summary>
/// 游戏配置总控
/// </summary>
public class GameConfigs : ScriptableObject
{
    public const string CONST_VALUE_PATH = "GameConfigs";
    public const string CONST_VALUE_ASSET_PATH = "Assets/Casual/Resources/" + CONST_VALUE_PATH + ".asset";

    public static Vector3 DISABLE_POSITION = new Vector3(10000f, 0f, 0f);

    [SerializeField]
    private string webURL = "";
    public static string WebURL { get { return Instance.webURL; } }

    public static string AssetDir = "StreamingAssets";
    [SerializeField]
    private string extName = ".unity3d";
    public static string ExtName { get { return Instance.extName; } }                   //素材扩展名
    public static string AssetPathInfoName = "AssetPathInfo"; //x路径的素材所对应的Bundle
    public static string AssetRoot = "Assets/Casual/Bundle";

    [SerializeField]
    private PublishMode publishMode = PublishMode.Debug;
    public static PublishMode PublishMode { get { return Instance.publishMode; } }
    public static bool DebugMode { get { return Instance.publishMode == PublishMode.Debug; } }

    private static GameConfigs instance;
    public static GameConfigs Instance
    {
        get
        {
            if (!instance && CONST_VALUE_PATH != null)
            {
                if (Application.isPlaying)
                {
                    instance = Resources.Load<GameConfigs>(CONST_VALUE_PATH);
                    DontDestroyOnLoad(instance);
                }
                else
                {
#if UNITY_EDITOR
                    instance = UnityEditor.AssetDatabase.LoadAssetAtPath<GameConfigs>(CONST_VALUE_ASSET_PATH);
#endif
                }
            }
            return instance;
        }
    }
}