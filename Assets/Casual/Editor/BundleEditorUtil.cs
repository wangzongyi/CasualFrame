using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public enum BundleNameType
{
    [LabelText("忽略")]
    None = 0,
    [LabelText("上层文件夹名"), HideInInspector]
    Folder = 1,
    [LabelText("递归至根目录")]
    RootFolder = 2,
    [LabelText("自身文件名")]
    FileName = 4,
    [LabelText("递归目录路径至文件")]
    FolderDeepPath = 8,
}

[TypeInfoBox("文件夹【Assets/Casual/Bundle】下所有文件打包策略:默认按文件上层文件夹名打包")]
public class BundleEditorUtil : SerializedScriptableObject
{
    [FoldoutGroup("配置文件列表"), LabelText("文件夹"), FolderPath(), InfoBox("每次清空")]
    public List<string> IgnoreConfigFolders = new List<string>();

    [FoldoutGroup("忽略列表"), LabelText("文件夹"), FolderPath()]
    [InfoBox("忽略列表下的文件将不会被打进包里")]
    public List<string> IgnoreFolders = new List<string>();

    [FoldoutGroup("忽略列表"), LabelText("文件名"), FilePath()]
    public List<string> IgnoreFiles = new List<string>();

    [FoldoutGroup("自身文件名"), HideLabel, FolderPath()]
    [InfoBox("Asset Bundle Name：自身文件名（不包含文件名后缀）")]
    public List<string> SelfNameFolders = new List<string>();

    [FoldoutGroup("保留文件路径"), HideLabel, FolderPath()]
    [InfoBox("Asset Bundle Name：从根目录递归至文件上层文件夹")]
    public List<string> RootFolders = new List<string>();

    private const string INIT_ASSET_NAME = "Editor/BundlePolicy.asset";
    private string AssetBundlePathRoot = Path.GetFullPath("Assets/Casual/Bundle/");

    [Button("开始批处理", ButtonSizes.Gigantic)]
    public void Batch()
    {
        clearConfig();
        List<string> paths = new List<string>();
        EditorUtils.Recursive(paths, Path.GetFullPath("Assets/Casual/Bundle"));
        for (int index = 0, len = paths.Count; index < len; index++)
        {
            EditorUtility.DisplayProgressBar("Set bundle name", string.Format("{0}/{1}", index, len), index * 1.0f / len);
            if (CheckIgnore(paths[index]) || CheckSelfName(paths[index]) || CheckRootFolder(paths[index]))
                continue;

            SetBundleName(paths[index], AssetBundlePathRoot, BundleNameType.FolderDeepPath);
        }

        AssetDatabase.RemoveUnusedAssetBundleNames();
        AssetDatabase.Refresh();
        EditorUtility.ClearProgressBar();
    }


    void clearConfig()
    {
        foreach (var item in IgnoreConfigFolders)
        {
            var fullPath = Path.GetFullPath(item);
            DirectoryInfo directory = new DirectoryInfo(fullPath);
            FileInfo[] fileInfo = directory.GetFiles("*", SearchOption.AllDirectories);
            for (int i = 0; i < fileInfo.Length; i++)
            {
                fileInfo[i].Delete();
            }
        }
    }

    bool CheckIgnore(string filePath)
    {
        foreach (var ignoreFile in IgnoreFiles)
        {
            if (!string.IsNullOrEmpty(ignoreFile) && filePath.Contains(ignoreFile))
            {
                SetBundleName(filePath, null, BundleNameType.None);
                return true;
            }
        }

        foreach (var ignoreDir in IgnoreFolders)
        {
            if (!string.IsNullOrEmpty(ignoreDir) && filePath.Contains(ignoreDir))
            {
                SetBundleName(filePath, null, BundleNameType.None);
                return true;
            }
        }

        return false;
    }

    bool CheckSelfName(string filePath)
    {
        foreach (var dir in SelfNameFolders)
        {
            if (!string.IsNullOrEmpty(dir) && filePath.Contains(dir))
            {
                SetBundleName(filePath, null, BundleNameType.FileName);
                return true;
            }
        }
        return false;
    }

    bool CheckRootFolder(string filePath)
    {
        foreach (var rootFolder in RootFolders)
        {
            if (!string.IsNullOrEmpty(rootFolder) && filePath.Contains(rootFolder))
            {
                SetBundleName(filePath, Path.Combine(Environment.CurrentDirectory, rootFolder).Replace('\\', '/'), BundleNameType.RootFolder);
                return true;
            }
        }
        return false;
    }

    public static void SetBundleName(string filePath, string assetRoot, BundleNameType bundleNameType)
    {
        string assetPath = filePath.Replace(Environment.CurrentDirectory.Replace('\\', '/') + "/", "");
        AssetImporter assetImporter = AssetImporter.GetAtPath(assetPath);
        switch (bundleNameType)
        {
            case BundleNameType.FileName:
                assetImporter.assetBundleName = Path.GetFileNameWithoutExtension(filePath);
                assetImporter.assetBundleVariant = GameConfigs.ExtName.Replace(".", "");
                break;
            case BundleNameType.Folder:
                assetImporter.assetBundleName = Path.GetFileName(Path.GetDirectoryName(filePath));
                assetImporter.assetBundleVariant = GameConfigs.ExtName.Replace(".", "");
                break;
            case BundleNameType.RootFolder:
                string rootDir = Path.GetDirectoryName(assetRoot).Replace('\\', '/') + "/";
                string relativePath = filePath.Replace(rootDir, "");
                relativePath = relativePath.Replace(Path.GetExtension(relativePath), "");
                string relativeDir = Path.GetDirectoryName(relativePath).Replace('\\', '/');
                assetImporter.assetBundleName = relativeDir;
                assetImporter.assetBundleVariant = GameConfigs.ExtName.Replace(".", "");
                break;
            case BundleNameType.FolderDeepPath:
                string root_path = Path.GetDirectoryName(assetRoot).Replace('\\', '/') + "/";
                string ab_path_name = filePath.Replace(root_path, "").Replace(Path.GetExtension(filePath), "");
                assetImporter.assetBundleName = ab_path_name;
                assetImporter.assetBundleVariant = GameConfigs.ExtName.Replace(".", "");
                break;
            case BundleNameType.None:
                assetImporter.assetBundleName = null;
                break;
        }
    }

    private static BundleEditorUtil instance;
    public static BundleEditorUtil Instance
    {
        get
        {
            if (!instance)
            {
#if UNITY_EDITOR
                instance = UnityEditor.AssetDatabase.LoadAssetAtPath<BundleEditorUtil>(GameConfigs.GameRoot + "/" + INIT_ASSET_NAME);
                if (instance == null)
                {
                    instance = CreateInstance<BundleEditorUtil>();
                    UnityEditor.AssetDatabase.CreateAsset(instance, GameConfigs.GameRoot + "/" + INIT_ASSET_NAME);
                    UnityEditor.AssetDatabase.Refresh();
                }
#endif
            }
            return instance;
        }
    }
}
