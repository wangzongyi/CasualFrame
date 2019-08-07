using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Casual;

public enum BundleNameType
{
    [LabelText("忽略")]
    None = 0,
    [LabelText("上层文件夹名"), HideInInspector]
    Folder = 1,
    [LabelText("递归至根目录")]
    RootFolder = 2,
    [LabelText("自身文件名")]
    FileName = 3,
}

[TypeInfoBox("文件夹【Assets/Casual/Bundle】下所有文件打包策略:默认按文件上层文件夹名打包")]
public class BundleEditorUtil : SerializedScriptableObject
{
    [FoldoutGroup("忽略列表"), LabelText("文件夹"), FolderPath()]
    [InfoBox("忽略列表下的文件将不会被打进包里")]
    public  List<string> IgnoreFolders = new List<string>();

    [FoldoutGroup("忽略列表"), LabelText("文件名"), FilePath()]
    public  List<string> IgnoreFiles = new List<string>();

    [FoldoutGroup("自身文件名"), HideLabel, FolderPath()]
    [InfoBox("Asset Bundle Name：自身文件名（不包含文件名后缀）")]
    public  List<string> SelfNameFolders = new List<string>();

    private const string INIT_ASSET_NAME = "Editor/BundlePolicy.asset";

    [Button("开始批处理", ButtonSizes.Gigantic)]
    public  void Batch()
    {
        List<string> paths = new List<string>();
        EditorUtils.Recursive(paths, Path.GetFullPath("Assets/Casual/Bundle"));
        for (int index = 0, len = paths.Count; index < len; index++)
        {
            EditorUtility.DisplayProgressBar("Set bundle name", string.Format("{0}/{1}", index, len), index * 1.0f / len);

            if (CheckIgnore(paths[index]) || CheckSelfName(paths[index]))
                continue;

            SetBundleName(paths[index], BundleNameType.Folder);
        }

        EditorUtility.ClearProgressBar();
        AssetDatabase.RemoveUnusedAssetBundleNames();
    }

     bool CheckIgnore(string filePath)
    {
        foreach (var ignoreFile in IgnoreFiles)
        {
            if (!string.IsNullOrEmpty(ignoreFile) && filePath.Contains(ignoreFile))
            {
                SetBundleName(filePath, BundleNameType.None);
                return true;
            }
        }

        foreach (var ignoreDir in IgnoreFolders)
        {
            if (!string.IsNullOrEmpty(ignoreDir) && filePath.Contains(ignoreDir))
            {
                SetBundleName(filePath, BundleNameType.None);
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 自身文件名
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
     bool CheckSelfName(string filePath)
    {
        FileInfo fileInfo = new FileInfo(filePath);
        string dirPath = fileInfo.Directory.FullName;

        foreach (var dir in SelfNameFolders)
        {
            if (!string.IsNullOrEmpty(dir) && Path.GetFullPath(dir) == dirPath)
            {
                SetBundleName(filePath, BundleNameType.FileName);
                return true;
            }
        }
        return false;
    }

    public static void SetBundleName(string filePath, BundleNameType bundleNameType)
    {
        string assetRoot = Path.Combine(Environment.CurrentDirectory, GameConfigs.AssetRoot).Replace('\\', '/');//D:\汤姆猫英语\TTEnglish-U3D\Assets\Casual\Bundle
        string rootDir = Path.GetDirectoryName(assetRoot).Replace('\\', '/') + "/";//D:/汤姆猫英语/TTEnglish-U3D/Assets/Casual/Bundle
        string relativePath = filePath.Replace(rootDir, "");//Animation/*
        relativePath = relativePath.Replace(Path.GetExtension(relativePath), "");
        string relativeDir = Path.GetDirectoryName(relativePath).Replace('\\', '/');

        string assetPath = filePath.Replace(Environment.CurrentDirectory.Replace('\\', '/') + "/", "");
        AssetImporter assetImporter = AssetImporter.GetAtPath(assetPath);

        if (assetImporter != null)
        {
            switch (bundleNameType)
            {
                case BundleNameType.FileName:
                    assetImporter.assetBundleName = relativeDir + "/" + Path.GetFileNameWithoutExtension(filePath);
                    assetImporter.assetBundleVariant = GameConfigs.ExtName.Replace(".", "");
                    break;
                case BundleNameType.Folder:
                    string parentFolder = Path.GetDirectoryName(filePath);
                    bool hasOtherFolder = new DirectoryInfo(parentFolder).GetDirectories().Length > 0;

                    assetImporter.assetBundleName = relativeDir + (hasOtherFolder ? "/" + Path.GetFileNameWithoutExtension(filePath) : "");
                    assetImporter.assetBundleVariant = GameConfigs.ExtName.Replace(".", "");
                    break;
                case BundleNameType.None:
                    assetImporter.assetBundleName = null;
                    break;
            }
        }
        else
        {
            Debug.LogWarning ("cant find path creat  AssetImporter: " + assetPath);
        }

    }

    private static BundleEditorUtil instance;
    public static BundleEditorUtil Instance
    {
        get
        {
            if (!instance)
            {
                instance = UnityEditor.AssetDatabase.LoadAssetAtPath<BundleEditorUtil>(GameConfigs.GameRoot + "/" + INIT_ASSET_NAME);
                if (instance == null)
                {
                    instance = CreateInstance<BundleEditorUtil>();
                    UnityEditor.AssetDatabase.CreateAsset(instance, GameConfigs.GameRoot + "/" + INIT_ASSET_NAME);
                    UnityEditor.AssetDatabase.Refresh();
                }
            }
            return instance;
        }
    }
}
