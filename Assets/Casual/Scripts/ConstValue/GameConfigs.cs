using UnityEngine;
using Sirenix.OdinInspector;

public enum PublishMode
{
    [LabelText("正式")]
    Release,
    [LabelText("测试")]
    Debug,
}

/// <summary>
/// 游戏配置总控
/// </summary>
[TypeInfoBox("项目基础设置")]
public class GameConfigs : SerializedScriptableObject
{
    private const string INIT_GAMECONFIG_NAME = "GameConfigs";
    private const string INIT_GAME_ROOT_PATH = "Assets/Casual";
    private const string INIT_GAMECONFIG_ASSET_ROOT = "Assets/Resources/" + INIT_GAMECONFIG_NAME + ".asset";
    public static string BundleFileName = "BundleFile"; //资源的路径所对应的Bundle
    public static string AssetDir = "AssetBundleManifest";

    public const int TRY_DOWNLOAD_TIMES = 3;
    public const int DDOWNLOAD_TIMEOUT = 30;
    public const int HEART_BEAT_INTERVAL = 60 * 5;

    public static Vector3 DISABLE_POSITION = new Vector3(10000f, 0f, 0f);

    [SerializeField, LabelText("发布模式")]
    private PublishMode publishMode = PublishMode.Debug;
    public static PublishMode PublishMode { get { return Instance.publishMode; } }
    public static bool DebugMode { get { return Instance.publishMode == PublishMode.Debug; } }

    [SerializeField, LabelText("服务器地址"), ValueDropdown("Nets", AppendNextDrawer = true)]
    private string webURL = "";
    public static string WebURL { get { return Instance.webURL; } }

    [SerializeField, LabelText("项目根目录"), FolderPath, OnValueChanged("OnValueChangeGameRoot")]
    private string gameRoot = INIT_GAME_ROOT_PATH;
    public static string GameRoot { get { return Instance.gameRoot; } }

    [SerializeField, LabelText("资源根目录"), FolderPath, DisableInEditorMode]
    private string assetRoot = INIT_GAME_ROOT_PATH + "/Bundle";
    public static string AssetRoot { get { return Instance.assetRoot; } }

    [SerializeField, LabelText("资源后缀名"), DisableInEditorMode]
    private string extName = ".unity3d";
    public static string ExtName { get { return Instance.extName; } }

    private static GameConfigs instance;
    public static GameConfigs Instance
    {
        get
        {
            if (!instance && INIT_GAMECONFIG_NAME != null)
            {
                if (Application.isPlaying)
                {
                    instance = Resources.Load<GameConfigs>(INIT_GAMECONFIG_NAME);
                }
                else
                {
#if UNITY_EDITOR
                    instance = UnityEditor.AssetDatabase.LoadAssetAtPath<GameConfigs>(INIT_GAMECONFIG_ASSET_ROOT);
                    if (instance == null)
                    {
                        instance = CreateInstance<GameConfigs>();
                        UnityEditor.AssetDatabase.CreateAsset(instance, INIT_GAMECONFIG_ASSET_ROOT);
                        UnityEditor.AssetDatabase.Refresh();
                    }
#endif
                }
            }
            return instance;
        }
    }

#if UNITY_EDITOR
    private static string[] Nets = new string[] { "http://172.27.43.92:8080", "https://www.google.com", "https://" };

    [SerializeField, LabelText("默认字体")]
    private Font defaultFont;
    public static Font DefaultFont { get { return Instance.defaultFont; } }

    void OnValueChangeGameRoot()
    {
        assetRoot = gameRoot + "/Bundle";
    }

#endif
}