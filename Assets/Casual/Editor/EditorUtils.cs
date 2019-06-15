using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public class EditorUtils : Editor
{
    [MenuItem("Assets/Texture/Set Packing Tag")]
    protected static void SetPackingTag()
    {
        string str_selectedPath = AssetDatabase.GetAssetPath(Selection.activeObject).Replace("\\", "/");
        string str_packingTag = str_selectedPath.Substring(str_selectedPath.LastIndexOf("/") + 1);

        var files = Directory.GetFiles(str_selectedPath, "*.png");

        for (int i = 0, len = files.Length; i < len; i++)
        {
            TextureImporter importer = AssetImporter.GetAtPath(files[i]) as TextureImporter;
            importer.textureType = TextureImporterType.Sprite;

            //Image tex = Image.FromFile(files[i]);

            //int maxTextureSize = Mathf.Clamp(Mathf.Max(tex.Width, tex.Height), 32, 2048);

            TextureImporterPlatformSettings androidSettings = new TextureImporterPlatformSettings()
            {
                overridden = true,
                maxTextureSize = 2048,
                format = TextureImporterFormat.ETC_RGB4,
                allowsAlphaSplitting = true,
                compressionQuality = 100,
                name = "Android",
            };

            TextureImporterPlatformSettings iphoneSettings = new TextureImporterPlatformSettings()
            {
                overridden = true,
                maxTextureSize = 2048,
                format = TextureImporterFormat.PVRTC_RGBA4,
                allowsAlphaSplitting = false,
                compressionQuality = 100,
                name = "iPhone",
            };

            //importer.SetPlatformTextureSettings("Android", Mathf.Max(tex.width, tex.height), TextureImporterFormat.ETC_RGB4, 100, true);
            //importer.SetPlatformTextureSettings("iPhone", Mathf.Max(tex.width, tex.height), TextureImporterFormat.RGBA16, 100, false);
            importer.SetPlatformTextureSettings(androidSettings);
            importer.SetPlatformTextureSettings(iphoneSettings);

            importer.spritePackingTag = str_packingTag;
            //importer.mipmapEnabled = false;
            importer.SaveAndReimport();

            EditorUtility.DisplayProgressBar("Set Packing Tag", string.Format("{0}/{1}", i, len), i * 1.0f / len);
        }
        EditorUtility.ClearProgressBar();
    }

    [MenuItem("Assets/Texture/Set Packing Tag", true)]
    protected static bool IskDirectory()
    {
        if (Selection.activeObject)
        {
            string projectPath = Environment.CurrentDirectory;
            string str_selectedPath = Path.Combine(projectPath, AssetDatabase.GetAssetPath(Selection.activeObject)).Replace("\\", "/");
            FileInfo fileInfo = new FileInfo(str_selectedPath);
            if ((fileInfo.Attributes & FileAttributes.Directory) == 0)
            {
                return false;
            }

            return true;
        }
        return false;
    }

    [MenuItem("Assets/Texture/Set ETC2 or PVRTC 4bits", true)]
    protected static bool CheckSetETC4()
    {
        return IskDirectory();
    }

    [MenuItem("Assets/Texture/Set ETC2 or PVRTC 4bits")]
    protected static void SetETC4()
    {
        string str_selectedPath = AssetDatabase.GetAssetPath(Selection.activeObject).Replace("\\", "/");
        var files = Directory.GetFiles(str_selectedPath, "*.png");

        for (int i = 0, len = files.Length; i < len; i++)
        {
            TextureImporter importer = AssetImporter.GetAtPath(files[i]) as TextureImporter;
            importer.textureType = TextureImporterType.Sprite;

            //int maxTextureSize = Mathf.Clamp(Mathf.Max(tex.Width, tex.Height), 32, 2048);
            TextureImporterPlatformSettings androidSettings = new TextureImporterPlatformSettings()
            {
                overridden = true,
                maxTextureSize = 2048,
                format = TextureImporterFormat.ETC2_RGB4,
                allowsAlphaSplitting = true,
                compressionQuality = 100,
                name = "Android",
            };

            TextureImporterPlatformSettings iphoneSettings = new TextureImporterPlatformSettings()
            {
                overridden = true,
                maxTextureSize = 2048,
                format = TextureImporterFormat.PVRTC_RGB4,
                allowsAlphaSplitting = false,
                compressionQuality = 100,
                name = "iPhone",
            };

            //importer.SetPlatformTextureSettings("Android", Mathf.Max(tex.width, tex.height), TextureImporterFormat.ETC_RGB4, 100, true);
            //importer.SetPlatformTextureSettings("iPhone", Mathf.Max(tex.width, tex.height), TextureImporterFormat.RGBA16, 100, false);
            importer.textureType = TextureImporterType.Default;
            importer.mipmapEnabled = false;
            importer.npotScale = TextureImporterNPOTScale.ToNearest;

            importer.SetPlatformTextureSettings(androidSettings);
            importer.SetPlatformTextureSettings(iphoneSettings);

            importer.SaveAndReimport();

            EditorUtility.DisplayProgressBar("Set 4bits", string.Format("{0}/{1}", i, len), i * 1.0f / len);
        }
        EditorUtility.ClearProgressBar();
    }

    class BundleBuildInfo
    {
        public string AssetRoot;
        public string AssetPath;
    }

    /// <summary>
    /// 一个文件夹一个Bundle
    /// </summary>
    [MenuItem("Assets/Asset Bundle/完整文件夹名")]
    protected static void MarkFullPackage()
    {
        SetBundleName(BundleNameType.RootFolder);
    }

    /// <summary>
    /// 一个文件夹一个Bundle
    /// </summary>
    [MenuItem("Assets/Asset Bundle/文件夹名")]
    protected static void MarkPackage()
    {
        SetBundleName(BundleNameType.Folder);
    }

    /// <summary>
    /// 单个文件一个Bundle
    /// </summary>
    [MenuItem("Assets/Asset Bundle/文件名")]
    protected static void MarkAlone()
    {
        SetBundleName(BundleNameType.FileName);
    }

    /// <summary>
    /// 清除Bundle
    /// </summary>
    [MenuItem("Assets/Asset Bundle/清除名字")]
    protected static void ClearBundleName()
    {
        SetBundleName(BundleNameType.None);
    }

    private static void SetBundleName(BundleNameType bundleNameType)
    {
        List<BundleBuildInfo> bbis = GetAssetPaths();

        string projectPath = Environment.CurrentDirectory.Replace("\\", "/");

        for (int index = 0, len = bbis.Count; index < len; index++)
        {
            BundleEditorUtil.SetBundleName(bbis[index].AssetPath, bbis[index].AssetRoot, bundleNameType);
            EditorUtility.DisplayProgressBar("Set bundle name", string.Format("{0}/{1}", index, len), index * 1.0f / len);
        }

        EditorUtility.ClearProgressBar();
        AssetDatabase.RemoveUnusedAssetBundleNames();
    }

    static List<BundleBuildInfo> GetAssetPaths()
    {
        List<BundleBuildInfo> bundleBuildInfos = new List<BundleBuildInfo>();

        string projectPath = Environment.CurrentDirectory;

        foreach (var selectObj in Selection.objects)
        {
            string str_selectedPath = Path.Combine(projectPath, AssetDatabase.GetAssetPath(selectObj)).Replace("\\", "/");
            FileInfo fileInfo = new FileInfo(str_selectedPath);

            if ((fileInfo.Attributes & FileAttributes.Directory) == 0)
            {
                EditorUtility.DisplayDialog("Invalid Path", fileInfo.Name + " 不是文件夹形式! \n", "OK");
                continue;
            }

            Recursive(str_selectedPath, bundleBuildInfos, str_selectedPath);
        }
        return bundleBuildInfos;
    }

    /// <summary>
    /// 遍历目录及其子目录
    /// </summary>
    static void Recursive(string selected, List<BundleBuildInfo> bbis, string root, string ignoreExt = ".meta .cs")
    {
        string[] names = Directory.GetFiles(root);
        string[] dirs = Directory.GetDirectories(root);
        foreach (string filename in names)
        {
            string ext = Path.GetExtension(filename);
            if (ignoreExt.Contains(ext)) continue;

            BundleBuildInfo bbi = new BundleBuildInfo()
            {
                AssetRoot = selected,
                AssetPath = filename.Replace('\\', '/'),
            };
            bbis.Add(bbi);
        }
        foreach (string dir in dirs)
        {
            Recursive(selected, bbis, dir.Replace('\\', '/'));
        }
    }

    /// <summary>
    /// 遍历目录及其子目录
    /// </summary>
    public static void Recursive(List<string> files, string root, string ignoreExt = ".meta .cs")
    {
        string[] names = Directory.GetFiles(root);
        string[] dirs = Directory.GetDirectories(root);
        foreach (string filename in names)
        {
            string ext = Path.GetExtension(filename);
            if (ignoreExt.Contains(ext)) continue;

            files.Add(filename.Replace('\\', '/'));
        }
        foreach (string dir in dirs)
        {
            Recursive(files, dir.Replace('\\', '/'));
        }
    }

    [MenuItem("GameObject/重命名同级物体", false, 18)]
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
                    if (brother != tran)
                    {
                        brother.gameObject.name = string.Format("{0}_{1}", tran.name, index);
                    }
                }
            }
        }
    }

    //[MenuItem("Assets/ClearCodeSign")]
    static void ClearCodeSign()
    {
        string projectPath = Environment.CurrentDirectory;

        foreach (var selectObj in Selection.objects)
        {
            string str_selectedPath = Path.Combine(projectPath, AssetDatabase.GetAssetPath(selectObj)).Replace("\\", "/");
            string[] files = Directory.GetFiles(str_selectedPath, "*.cs", SearchOption.AllDirectories);

            for (int index = 0, len = files.Length; index < len; index++)
            {
                string file = files[index];
                string code = File.ReadAllText(file);
                code = new Regex(@"^/\*\s{1,2}http://www\.cgsoso\.com/forum-211-1\.html(\s{1,4}.*){1,10}\*/\s+").Replace(code, "");

                File.WriteAllText(file, code);
                EditorUtility.DisplayProgressBar("Clear Bundle", string.Format("{0}/{1}", index, len), index * 1.0f / len);
            }
            EditorUtility.ClearProgressBar();
        }
        AssetDatabase.Refresh();
    }
}
