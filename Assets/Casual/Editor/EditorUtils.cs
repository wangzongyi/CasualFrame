using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using UnityEditor;
using UnityEngine;

public class EditorUtils : Editor
{
    [MenuItem("Assets/SetPackingTag")]
    protected static void SetPackingTag()
    {
        string str_selectedPath = AssetDatabase.GetAssetPath(Selection.activeObject).Replace("\\", "/");

        string str_packingTag = str_selectedPath.Substring(str_selectedPath.LastIndexOf("/") + 1);

        var files = Directory.GetFiles(str_selectedPath);

        for (int i = 0; i < files.Length; i++)
        {
            if (files[i].EndsWith(".png"))
            {
                TextureImporter importer = AssetImporter.GetAtPath(files[i]) as TextureImporter;
                importer.textureType = TextureImporterType.Sprite;

                Image tex = Image.FromFile(files[i]);

                //int maxTextureSize = Mathf.Clamp(Mathf.Max(tex.Width, tex.Height), 32, 2048);
                TextureImporterPlatformSettings androidSettings = new TextureImporterPlatformSettings();
                TextureImporterPlatformSettings iphoneSettings = new TextureImporterPlatformSettings();

                androidSettings.overridden = true;
                androidSettings.maxTextureSize = 2048;
                androidSettings.format = TextureImporterFormat.ETC_RGB4;
                androidSettings.allowsAlphaSplitting = true;
                androidSettings.compressionQuality = 100;
                androidSettings.name = "Android";

                iphoneSettings.overridden = true;
                iphoneSettings.maxTextureSize = 2048;
                iphoneSettings.format = TextureImporterFormat.PVRTC_RGBA4;
                iphoneSettings.allowsAlphaSplitting = false;
                iphoneSettings.compressionQuality = 100;
                iphoneSettings.name = "iPhone";

                //importer.SetPlatformTextureSettings("Android", Mathf.Max(tex.width, tex.height), TextureImporterFormat.ETC_RGB4, 100, true);
                //importer.SetPlatformTextureSettings("iPhone", Mathf.Max(tex.width, tex.height), TextureImporterFormat.RGBA16, 100, false);
                importer.SetPlatformTextureSettings(androidSettings);
                importer.SetPlatformTextureSettings(iphoneSettings);

                importer.spritePackingTag = str_packingTag;
                //importer.mipmapEnabled = false;
                importer.SaveAndReimport();
            }
        }
    }

    /// <summary>
    /// 一个文件夹一个Bundle
    /// </summary>
    [MenuItem("Assets/Bundle Name/文件夹名")]
    protected static void MarkPackage()
    {
        string projectPath = new DirectoryInfo(Application.dataPath + "/..").FullName.Replace("\\", "/");

        List<string> assetPaths = new List<string>();

        foreach (var selectObj in Selection.objects)
        {
            string str_selectedPath = Path.Combine(projectPath, AssetDatabase.GetAssetPath(selectObj)).Replace("\\", "/");
            FileInfo fileInfo = new FileInfo(str_selectedPath);

            if ((fileInfo.Attributes & FileAttributes.Directory) == 0)
            {
                EditorUtility.DisplayDialog("Invalid Path", fileInfo.Name + " 不是文件夹形式! \n", "OK");
                continue;
            }

            Recursive(assetPaths, str_selectedPath);
        }

        for (int index = 0, len = assetPaths.Count; index < len; index++)
        {
            string assetPath = assetPaths[index];
            AssetImporter assetImporter = AssetImporter.GetAtPath(assetPath.Replace(projectPath + "/", ""));
            assetImporter.assetBundleName = Path.GetFileName(Path.GetDirectoryName(assetPath));
            assetImporter.assetBundleVariant = GameConfigs.ExtName.Replace(".", "");

            EditorUtility.DisplayProgressBar("Set bundle name", string.Format("{0}/{1}", index, len), index * 1.0f / len);
        }
        EditorUtility.ClearProgressBar();
        AssetDatabase.RemoveUnusedAssetBundleNames();
    }

    /// <summary>
    /// 单个文件一个Bundle
    /// </summary>
    [MenuItem("Assets/Bundle Name/文件名")]
    protected static void MarkAlone()
    {
        string projectPath = new DirectoryInfo(Application.dataPath + "/..").FullName.Replace("\\", "/");

        List<string> assetPaths = new List<string>();

        foreach (var selectObj in Selection.objects)
        {
            string str_selectedPath = Path.Combine(projectPath, AssetDatabase.GetAssetPath(selectObj)).Replace("\\", "/");
            FileInfo fileInfo = new FileInfo(str_selectedPath);

            if ((fileInfo.Attributes & FileAttributes.Directory) == 0)
            {
                EditorUtility.DisplayDialog("Invalid Path", fileInfo.Name + " 不是文件夹形式! \n", "OK");
                continue;
            }

            Recursive(assetPaths, str_selectedPath);
        }

        for (int index = 0, len = assetPaths.Count; index < len; index++)
        {
            string assetPath = assetPaths[index];
            AssetImporter assetImporter = AssetImporter.GetAtPath(assetPath.Replace(projectPath + "/", ""));
            assetImporter.assetBundleName = Path.GetFileNameWithoutExtension(assetPath);
            assetImporter.assetBundleVariant = GameConfigs.ExtName.Replace(".", "");

            EditorUtility.DisplayProgressBar("Set bundle name", string.Format("{0}/{1}", index, len), index * 1.0f / len);
        }

        EditorUtility.ClearProgressBar();
        AssetDatabase.RemoveUnusedAssetBundleNames();
    }

    /// <summary>
    /// 清除Bundle
    /// </summary>
    [MenuItem("Assets/Bundle Name/清除名字")]
    protected static void ClearBundleName()
    {
        string projectPath = new DirectoryInfo(Application.dataPath + "/..").FullName.Replace("\\", "/");

        List<string> assetPaths = new List<string>();

        foreach (var selectObj in Selection.objects)
        {
            string str_selectedPath = Path.Combine(projectPath, AssetDatabase.GetAssetPath(selectObj)).Replace("\\", "/");
            FileInfo fileInfo = new FileInfo(str_selectedPath);

            if ((fileInfo.Attributes & FileAttributes.Directory) == 0)
            {
                EditorUtility.DisplayDialog("Invalid Path", fileInfo.Name + " 不是文件夹形式! \n", "OK");
                continue;
            }

            Recursive(assetPaths, str_selectedPath);
        }

        for (int index = 0, len = assetPaths.Count; index < len; index++)
        {
            string assetPath = assetPaths[index];
            AssetImporter assetImporter = AssetImporter.GetAtPath(assetPath.Replace(projectPath + "/", ""));
            assetImporter.assetBundleName = null;

            EditorUtility.DisplayProgressBar("Set bundle name", string.Format("{0}/{1}", index, len), index * 1.0f / len);
        }
        EditorUtility.ClearProgressBar();
        AssetDatabase.RemoveUnusedAssetBundleNames();
    }


    /// <summary>
    /// 遍历目录及其子目录
    /// </summary>
    static void Recursive(List<string> paths, string root, string ignoreExt = ".meta .cs")
    {
        string[] names = Directory.GetFiles(root);
        string[] dirs = Directory.GetDirectories(root);
        foreach (string filename in names)
        {
            string ext = Path.GetExtension(filename);
            if (ignoreExt.Contains(ext)) continue;
            paths.Add(filename.Replace('\\', '/'));
        }
        foreach (string dir in dirs)
        {
            Recursive(paths, dir.Replace('\\', '/'));
        }
    }
    [MenuItem("GameObject/RenameBrother", false, 18)]
    static void RenameBrother()
    {
        Transform[] transforms = Selection.GetTransforms(SelectionMode.Assets);
        foreach (Transform tran in transforms)
        {
            if (tran.parent)
            {
                for (int index = 0, len = tran.parent.childCount; index < len; index++)
                {
                    Transform brother = tran.parent.GetChild(index);
                    if(brother!=tran)
                    {
                        brother.gameObject.name = string.Format("{0}_{1}", tran.name, index);
                    }
                }
            }
        }
    }
}
