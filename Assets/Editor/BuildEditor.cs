using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

public enum ValidBuildTarget
{
    //NoTarget = -2,        --doesn't make sense
    //iPhone = -1,          --deprecated
    //BB10 = -1,            --deprecated
    //MetroPlayer = -1,     --deprecated
    //StandaloneOSXUniversal = 2,
    //StandaloneOSXIntel = 4,
    StandaloneWindows = 5,
    //WebPlayer = 6,
    //WebPlayerStreamed = 7,
    iOS = 9,
    //PS3 = 10,
    //XBOX360 = 11,
    Android = 13,
}

internal enum CompressOptions
{
    [LabelText("不压缩")]
    Uncompressed = 0,
    [LabelText("LZMA")]
    StandardCompression,
    [LabelText("LZ4")]
    ChunkBasedCompression,
}

[Flags]
internal enum BundleOptions
{
    None = BuildAssetBundleOptions.None,
    ExcludeTypeInformation = BuildAssetBundleOptions.DisableWriteTypeTree,
    ForceRebuild = BuildAssetBundleOptions.ForceRebuildAssetBundle,
    IgnoreTypeTreeChanges = BuildAssetBundleOptions.IgnoreTypeTreeChanges,
    AppendHash = BuildAssetBundleOptions.AppendHashToAssetBundleName,
    StrictMode = BuildAssetBundleOptions.StrictMode,
    DryRunBuild = BuildAssetBundleOptions.DryRunBuild,
}

[Title("当前环境： " +
#if UNITY_STANDALONE_WIN
    "StandaloneWindows"
#elif UNITY_ANDROID
    "Android"
#elif UNITY_IPHONE
    "IOS"
#endif
    )]
public class BuildEditor : SerializedScriptableObject
{
    private static readonly string INIT_ASSET_ROOT = GameConfigs.GameRoot + "/Editor/BuildEditor.asset";
    private const string StreamingAssetPath = "Assets/StreamingAssets/AssetBundle";

    [SerializeField]
    private static BuildEditor instance;

    internal static BuildEditor Instance
    {
        get
        {
            if (!instance)
            {
                instance = AssetDatabase.LoadAssetAtPath<BuildEditor>(INIT_ASSET_ROOT);

                if (instance == null)
                {
                    instance = CreateInstance<BuildEditor>();
                    UnityEditor.AssetDatabase.CreateAsset(instance, INIT_ASSET_ROOT);
                    UnityEditor.AssetDatabase.Refresh();
                }
            }

            return instance;
        }
    }
    [NonSerialized]
    internal ValidBuildTarget BuildTarget =
#if UNITY_STANDALONE_WIN
        ValidBuildTarget.StandaloneWindows;
#elif UNITY_ANDROID
        ValidBuildTarget.Android;
#elif UNITY_IPHONE
        ValidBuildTarget.iOS;
#endif
    [TabGroup("AssetBundle"), SerializeField, FolderPath]
    internal string OutputPath;

    [TabGroup("AssetBundle"), SerializeField]
    internal bool CopyToStreamingAssets = false;

    [TabGroup("AssetBundle"), LabelText("压缩格式"), SerializeField]
    internal CompressOptions CompressOptions = CompressOptions.StandardCompression;

    [TabGroup("AssetBundle"), LabelText("资源选项"), SerializeField]
    internal BundleOptions BundleOptions = BundleOptions.None;

    [TabGroup("AssetBundle"), SerializeField]
    internal BundleEditorUtil BundlePolicy;

    [TabGroup("AssetBundle"), Button("批处理资源", ButtonSizes.Medium)]
    public void Batch()
    {
        BundlePolicy.Batch();
    }

    [TabGroup("AssetBundle"), Button("资源打包", ButtonSizes.Medium)]
    public void BuildBundle()
    {
        string outputPath = string.Format("{0}/{1}", OutputPath, BuildTarget);

        if (string.IsNullOrEmpty(outputPath))
        {
            Debug.LogError("AssetBundle Build: No valid output path for build.");
            return;
        }
        if ((BundleOptions.ForceRebuild & BundleOptions) == BundleOptions.ForceRebuild)
        {
            string message = "Do you want to delete all files in the directory " + outputPath;
            if (CopyToStreamingAssets)
                message += " and " + StreamingAssetPath;
            message += "?";
            if (EditorUtility.DisplayDialog("File delete confirmation", message, "Yes", "No"))
            {
                try
                {
                    if (Directory.Exists(outputPath))
                        Directory.Delete(outputPath, true);

                    if (CopyToStreamingAssets)
                        if (Directory.Exists(StreamingAssetPath))
                            Directory.Delete(StreamingAssetPath, true);
                }
                catch (System.Exception e)
                {
                    Debug.LogException(e);
                }
            }

            if (!Directory.Exists(outputPath))
                Directory.CreateDirectory(outputPath);
        }
        BuildAssetBundleOptions opt = BuildAssetBundleOptions.None;
        if (CompressOptions == CompressOptions.Uncompressed)
            opt |= BuildAssetBundleOptions.UncompressedAssetBundle;
        else if (CompressOptions == CompressOptions.ChunkBasedCompression)
            opt |= BuildAssetBundleOptions.ChunkBasedCompression;
        opt |= (BuildAssetBundleOptions)BundleOptions;

        CreateAssetBundleList();

        BuildPipeline.BuildAssetBundles(outputPath, opt, (BuildTarget)BuildTarget);

        string projectPath = new DirectoryInfo(Application.dataPath + "/..").FullName;
        string manifestPath = string.Format("{0}/{1}/{2}", projectPath, outputPath, BuildTarget);
        string newManifestPath = string.Format("{0}/{1}/StreamingAssets", projectPath, outputPath);

        // 重命名 manifest 文件
        if (File.Exists(manifestPath))
        {
            if (File.Exists(newManifestPath))
                File.Delete(newManifestPath);
            File.Move(manifestPath, newManifestPath);
        }
        if (File.Exists(manifestPath + ".manifest"))
        {
            if (File.Exists(newManifestPath + ".manifest"))
                File.Delete(newManifestPath + ".manifest");
            File.Move(manifestPath + ".manifest", newManifestPath + ".manifest");
        }

        if (CopyToStreamingAssets)
            DirectoryCopy(outputPath, StreamingAssetPath);

        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
    }

    [SerializeField, TabGroup("ExportProject")]
    internal GameConfigs GameConfigs;

    [TabGroup("ExportProject"), FolderPath]
    public string ExportProjectPath;
    [TabGroup("ExportProject"), FolderPath, ShowIf("ShowNativePath")]
    public string NativeProjectPath;

    internal string RealExportPath { get { return ExportProjectPath + "_" + BuildTarget.ToString().ToLower(); } }

#if UNITY_EDITOR
    private bool ShowNativePath { get { return instance.BuildTarget == ValidBuildTarget.Android; } }
#endif

    [TabGroup("ExportProject"), Button("导出工程", ButtonSizes.Medium)]
    public void ExportProject()
    {
        if (EditorUtility.DisplayDialog("导出工程", "是否导出工程？", "Yes", "No"))
        {
            if (Directory.Exists(RealExportPath))
            {
                Directory.Delete(RealExportPath, true);
            }

            BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, RealExportPath, (BuildTarget)BuildTarget, BuildOptions.AcceptExternalModificationsToPlayer);
        }
    }

    public static void CreateAssetBundleList()
    {
        string bundleFilePath = GameConfigs.AssetRoot + "/" + GameConfigs.BundleFileName + ".txt";
        string projectPath = System.Environment.CurrentDirectory;
        string mapOfAssetPath = Path.Combine(projectPath, bundleFilePath);

        if (!File.Exists(mapOfAssetPath))
        {
            FileStream fileStream = File.Create(mapOfAssetPath);
            fileStream.Dispose();

            AssetDatabase.Refresh();
        }

        AssetImporter assetImporter = AssetImporter.GetAtPath(bundleFilePath);
        assetImporter.assetBundleName = GameConfigs.BundleFileName;
        assetImporter.assetBundleVariant = GameConfigs.ExtName.Replace(".", "");

        HashSet<string> bundleHashMap = new HashSet<string>();
        StringBuilder bundleBuilder = new StringBuilder();
        string[] allAssetBundleNames = AssetDatabase.GetAllAssetBundleNames();
        foreach (string abName in allAssetBundleNames)
        {
            string[] assetPaths = AssetDatabase.GetAssetPathsFromAssetBundle(abName);
            foreach (string assetPath in assetPaths)
            {
                if (bundleHashMap.Contains(assetPath))
                {
                    Debug.LogError("已存在" + assetPath);
                }
                else
                {
                    bundleHashMap.Add(assetPath);
                    bundleBuilder.AppendLine(string.Format("{0}|{1}", abName, assetPath.ToLower()));
                }
            }
        }
        File.WriteAllText(mapOfAssetPath, bundleBuilder.ToString());

        AssetDatabase.Refresh();
    }

    private static void DirectoryCopy(string sourceDirName, string destDirName)
    {
        // If the destination directory doesn't exist, create it.
        if (!Directory.Exists(destDirName))
        {
            Directory.CreateDirectory(destDirName);
        }

        foreach (string folderPath in Directory.GetDirectories(sourceDirName, "*", SearchOption.AllDirectories))
        {
            if (!Directory.Exists(folderPath.Replace(sourceDirName, destDirName)))
                Directory.CreateDirectory(folderPath.Replace(sourceDirName, destDirName));
        }

        foreach (string filePath in Directory.GetFiles(sourceDirName, "*.*", SearchOption.AllDirectories))
        {
            var fileDirName = Path.GetDirectoryName(filePath).Replace("\\", "/");
            var fileName = Path.GetFileName(filePath);
            string newFilePath = Path.Combine(fileDirName.Replace(sourceDirName, destDirName), fileName);

            if (!filePath.EndsWith(".manifest"))
            {
                File.Copy(filePath, newFilePath, true);
            }
        }
    }

    private const string AssetPath = "/src/main/assets";
    private const string LibPath = "/src/main/jniLibs";

    private const string PostProcessBuildPath = "PostProcessBuild/";

    [PostProcessBuild(100)]
    public static void OnPostProcessBuild(BuildTarget target, string pathToBuiltProject)
    {
        if (target == UnityEditor.BuildTarget.Android && Directory.Exists(Instance.NativeProjectPath))
        {
            if (Directory.Exists(Instance.NativeProjectPath + AssetPath))
            {
                Directory.Delete(Instance.NativeProjectPath + AssetPath, true);
            }
            DirectoryCopy(Instance.ExportProjectPath + "/" + PlayerSettings.productName + AssetPath, Instance.NativeProjectPath + AssetPath);
            DirectoryCopy(Instance.ExportProjectPath + "/" + PlayerSettings.productName + LibPath, Instance.NativeProjectPath + LibPath);
            DirectoryCopy(PostProcessBuildPath + UnityEditor.BuildTarget.Android, Instance.NativeProjectPath + AssetPath);
        }
    }
}
