using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

public enum PublishMode
{
    [LabelText("正式")]
    Release,
    [LabelText("测试")]
    Debug,
}

/// <summary>
/// 汤姆猫游戏配置总控
/// </summary>
public class GameConfigs : SerializedScriptableObject
{
    public const string INIT_GAMECONFIG_NAME = "GameConfigs";
    public const string INIT_GAMECONFIG_ASSET_ROOT = GameRoot + "/Resources/" + INIT_GAMECONFIG_NAME + ".asset";
    public const string GameRoot = "Assets/Casual";
    public const string AssetRoot = GameRoot + "/Bundle";

    public static string BundleFileName = "BundleFile"; //资源的路径所对应的Bundle
    public static string AssetDir = "StreamingAssets";

    public static Vector3 DISABLE_POSITION = Vector3.one * 10000;

    public const int TRY_DOWNLOAD_TIMES = 3;
    public const int DDOWNLOAD_TIMEOUT = 30;
    public const int HEART_BEAT_INTERVAL = 60 * 5;

    [SerializeField, LabelText("服务器地址"), ValueDropdown("Nets", AppendNextDrawer = true, FlattenTreeView = true)]
    private string webURL = "";
    public static string WebURL { get { return Instance.webURL; } }

    public static string ExactWebURL;

    [SerializeField, LabelText("资源拓展名"), DisableInEditorMode]
    private string extName = ".unity3d";
    public static string ExtName { get { return Instance.extName; } }//素材扩展名

    [SerializeField, LabelText("发布模式")]
    private PublishMode publishMode = PublishMode.Debug;
    public static PublishMode PublishMode
    {
        get
        {
#if UNITY_EDITOR
            return Instance.publishMode;
#else
            return PublishMode.Release;
#endif
        }
    }

    [SerializeField, LabelText("开发者模式")]
    private bool gameMaker;
    public static bool GameMaker
    {
        get
        {
#if UNITY_EDITOR
            return Instance.gameMaker;
#else
            return false;
#endif
        }
    }

    [SerializeField, LabelText("是否打印日志")]
    private bool logEnabled = false;
    public static bool LogEnabled { get { return Instance.logEnabled; } }

    [SerializeField, LabelText("默认字体")]
    private Font defaultFont;
    public static Font DefaultFont { get { return Instance.defaultFont; } }

    [SerializeField, LabelText("版本"), OnValueChanged("OnVersionChange")]
    private string version;

    /// <summary>
    /// 物理长宽比，为手机属性，不可更改
    /// </summary>
    public static float AspectRatioPhysical;
    /// <summary>
    /// 物理分辨率，为手机属性，不可更改
    /// </summary>
    public static Vector2 ScreenSizePhysical;
    /// <summary>
    /// 预设长宽比，为策划设定，不可轻易修改
    /// </summary>
    public const float AspectRatioPreset = 16.0f / 9;
    /// <summary>
    /// 计算所得逻辑长宽比，根据物理属性和策划预设计算所得。服务于屏幕逻辑尺寸和Canvas默认值，可根据实际情况自行调整
    /// </summary>
    public static float AspectRatioLogic;
    /// <summary>
    /// 预设分辨率，为策划设定，不可轻易修改
    /// </summary>
    public static readonly Vector2 ScreenSizePreset = new Vector2(1920, 1080);
    /// <summary>
    /// 计算所得逻辑分辨率，根据物理属性和策划预设计算所得。服务于屏幕逻辑尺寸和Canvas默认值，可根据实际情况自行调整
    /// </summary>
    public static Vector2 ScreenSizeLogic;
    /// <summary>
    /// 逻辑尺寸到物理尺寸转换系数
    /// </summary>
    public static Vector2 LogicToPhysicalMul;
    /// <summary>
    /// 物理尺寸到逻辑尺寸转换系数，避免频繁使用浮点除法，提高运算效率用
    /// </summary>
    public static Vector2 PhysicalToLogicMul;
    /// <summary>
    /// Canvas宽高适配值
    /// </summary>
    public static float CanvasMathch;

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
    private static string[] Nets = new string[]
    {
        "",
        "https://"
    };

    void OnVersionChange()
    {
        UnityEditor.PlayerSettings.bundleVersion = version;
    }
#endif
}