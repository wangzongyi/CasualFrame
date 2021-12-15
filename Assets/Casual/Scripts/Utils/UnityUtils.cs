using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using System;

public struct RectInt
{
    public int x;
    public int y;
    public int w;
    public int h;
}

public static class UnityUtils
{
    /// <summary>
    /// 根据宽/高进行适配
    /// </summary>
    /// <param name="scale">项目适配仅宽或高，故目前仅支持0：设定宽度自适应高度；1：设定高度自适应宽度</param>
    /// <param name="updateNorSet"></param>
    /// <returns></returns>
    public static void CalculateScreenSize()
    {
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        float presetWidth = GameConfigs.ScreenSizePreset.x;
        float presetHeight = GameConfigs.ScreenSizePreset.y;

        int scale = screenWidth / screenHeight > GameConfigs.AspectRatioPreset ? 1 : 0;

        Vector2 logicSize = scale == 0 ? new Vector2(presetWidth, screenHeight * presetWidth / screenWidth) : new Vector2(screenWidth * presetHeight / screenHeight, presetHeight);
        GameConfigs.CanvasMathch = scale;
        GameConfigs.AspectRatioLogic = logicSize.x / logicSize.y;
        GameConfigs.ScreenSizeLogic = logicSize;
        GameConfigs.AspectRatioPhysical = screenWidth * 1.0f / screenHeight;
        GameConfigs.ScreenSizePhysical = new Vector2(Screen.width, Screen.height);
        GameConfigs.LogicToPhysicalMul = new Vector2(screenWidth / logicSize.x, screenHeight / logicSize.y);
        GameConfigs.PhysicalToLogicMul = new Vector2(logicSize.x / screenWidth, logicSize.y / screenHeight);
    }

    public static float CalculateScreenScale()
    {
        float width = Screen.width;
        float height = Screen.height;
        return width / height > GameConfigs.AspectRatioPreset ? 1 : 0;
    }

    /// <summary>
    /// 2D摄像机适配，当前场景打开时，适配一次即可
    /// </summary>
    /// <param name="myCamera"></param>
    public static void TwoDCameraAdaptationWidth(Camera myCamera)
    {
        if (GameConfigs.AspectRatioLogic > GameConfigs.AspectRatioPreset)
        {
            myCamera.orthographicSize *= GameConfigs.AspectRatioPreset / GameConfigs.AspectRatioLogic;
        }
    }

    /// <summary>
    /// 2D摄像机适配，当前场景打开时，适配一次即可
    /// </summary>
    /// <param name="myCamera"></param>
    public static void TwoDCameraAdaptationHeight(Camera myCamera)
    {
        if (GameConfigs.AspectRatioLogic < GameConfigs.AspectRatioPreset)
        {
            myCamera.orthographicSize *= GameConfigs.AspectRatioPreset / GameConfigs.AspectRatioLogic;
        }
    }
    /// <summary>
    /// 3D摄像机适配，当前场景打开时，适配一次即可
    /// </summary>
    /// <param name="myCamera"></param>
    public static void ThreeDCameraAdaptation(Camera myCamera)
    {
        if (GameConfigs.AspectRatioLogic < GameConfigs.AspectRatioPreset)
        {
            myCamera.fieldOfView *= GameConfigs.AspectRatioPreset / GameConfigs.AspectRatioLogic;
        }
    }


    /// <summary>
    /// 遍历目录及其子目录
    /// </summary>
    public static List<string> Recursive(string root, SearchOption searchOption = SearchOption.AllDirectories, string ignoreExt = ".meta .cs .preset")
    {
        List<string> files = new List<string>();
        string[] names = Directory.GetFiles(root, "*.*", searchOption);
        foreach (string filename in names)
        {
            string ext = Path.GetExtension(filename);
            if (ignoreExt.Contains(ext)) continue;

            files.Add(filename.Replace('\\', '/'));
        }

        return files;
    }

    public static string GetAssetURL(string abName)
    {
        if (File.Exists(Path.Combine(Application.persistentDataPath, abName)))
        {
            return Path.Combine(Application.persistentDataPath, abName);
        }
        else
        {
            return Path.Combine(Application.streamingAssetsPath + "/AssetBundle", abName);
        }
    }

    public static string GetUncompressedAssetURL(string assetName)
    {
        if (File.Exists(Path.Combine(Application.persistentDataPath + "/Uncompressed", assetName)))
        {
            return Path.Combine(Application.persistentDataPath + "/Uncompressed", assetName);
        }
        else
        {
            return Path.Combine(Application.streamingAssetsPath + "/Uncompressed", assetName);
        }
    }

    public static RenderTexture RenderToTexture(this Camera camera, int width, int heigh)
    {
        RenderTexture rTexture = camera.targetTexture;

        if (rTexture == null)
        {
            rTexture = RenderTexture.GetTemporary(width, heigh, 24);
            rTexture.DiscardContents();
            camera.targetTexture = rTexture;
        }
        camera.Render();
        return rTexture;
    }

    /// <summary>
    /// 将文件转换成byte[] 数组
    /// </summary>
    /// <param name="fileUrl">文件路径文件名称</param>
    /// <returns>byte[]</returns>
    public static byte[] FileToBytes(string fileUrl)
    {
        return File.ReadAllBytes(fileUrl);
    }

    public static string ComputeHash(byte[] bytes)
    {
        MD5 md5 = new MD5CryptoServiceProvider();
        byte[] retVal = md5.ComputeHash(bytes);

        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < retVal.Length; i++)
        {
            sb.Append(retVal[i].ToString("x2"));
        }
        return sb.ToString();
    }


    static public Vector3 ToVector3(this IList<float> data)
    {
        switch (data.Count)
        {
            case 0:
            case 1:
            case 2:
                return ToVector2(data);
            default:
                return new Vector3(data[0], data[1], data[2]);
        }
    }

    static public Vector2 ToVector2(this IList<float> data)
    {
        switch (data.Count)
        {
            case 0:
                return Vector2.zero;
            case 1:
                return new Vector2(data[0], 0);
            case 2:
                return new Vector2(data[0], data[1]);
            default:
                return Vector2.zero;
        }
    }

    static public Vector2Int ToVector2Int(this IList<int> data)
    {
        switch (data.Count)
        {
            case 1:
                return new Vector2Int(data[0], 0);
            case 2:
                return new Vector2Int(data[0], data[1]);
            default:
                return Vector2Int.zero;
        }
    }

    static public Vector3Int ToVector3Int(this IList<int> data)
    {
        switch (data.Count)
        {
            case 1:
                return new Vector3Int(data[0], 0, 0);
            case 2:
                return new Vector3Int(data[0], data[1], 0);
            case 3:
                return new Vector3Int(data[0], data[1], data[2]);
            default:
                return Vector3Int.zero;
        }
    }

    static public Vector3 ToVector3(this IList<int> data)
    {
        switch (data.Count)
        {
            case 1:
                return new Vector3(data[0], 0, 0);
            case 2:
                return new Vector3(data[0], data[1], 0);
            case 3:
                return new Vector3(data[0], data[1], data[2]);
            default:
                return Vector3.zero;
        }
    }

    static public Vector3 ToVector3(string data)
    {
        string[] vector3Strs = data.Split(',');
        switch (vector3Strs.Length)
        {
            case 1:
                return new Vector3(int.Parse(vector3Strs[0]), 0, 0);
            case 2:
                return new Vector3(int.Parse(vector3Strs[0]), int.Parse(vector3Strs[1]), 0);
            case 3:
                return new Vector3(int.Parse(vector3Strs[0]), int.Parse(vector3Strs[1]), int.Parse(vector3Strs[2]));
            default:
                return Vector3.zero;
        }
    }

    static public List<Vector2> ToVector2Array(this IList<float> data)
    {
        List<Vector2> vecArray = new List<Vector2>();
        for (int index = 0, len = data.Count - 1; index <= len; index++)
        {
            vecArray.Add(new Vector2(data[index], len > index ? data[++index] : 0f));
        }
        return vecArray;
    }

    static public void RandomList<T>(IList<T> list)
    {
        RandomList(list, list.Count);
    }

    static public void RandomList<T>(IList<T> list, int randomNum)
    {
        for (int index = 0, len = randomNum - 1, lastListIndex = list.Count - 1; index <= len; index++)
        {
            int listBackIndex = lastListIndex - index;
            int frontRange = UnityEngine.Random.Range(0, listBackIndex + 1);
            if (listBackIndex != frontRange)
            {
                T thisAnswer = list[frontRange];
                list[frontRange] = list[listBackIndex];
                list[listBackIndex] = thisAnswer;
            }
        }
    }

    /// <summary>
    ///  遍历子物体使其层级修改
    /// </summary>
    /// <param name="go"></param>
    /// <param name="parent"></param>
    static public void SetLayer(this GameObject go, int layer, bool changeAll)
    {
        go.layer = layer;
        if (changeAll) SetLayer(go.transform, layer);
    }

    static public void SetLayer(this GameObject go, string layerName, bool changeAll)
    {
        SetLayer(go, LayerMask.NameToLayer(layerName), changeAll);
    }

    public static void SetLayer(this Transform trans, int thisLayer)
    {
        for (int i = 0, len = trans.childCount; i < len; i++)
        {
            Transform child = trans.GetChild(i);
            if (!child.gameObject.CompareTag("KeepLayer"))
            {
                child.gameObject.layer = thisLayer;
            }
            SetLayer(child, thisLayer);
        }
    }

    public static void WriteFile(string path, string info)
    {
        StreamWriter sw;
        FileInfo file = new FileInfo(path);
        sw = file.CreateText();
        sw.WriteLine(info);
        sw.Close();
        sw.Dispose();
    }

    public static string LoadFile(string path)
    {
        string info = null;
        StreamReader sr = null;
        if (File.Exists(path))
        {
            sr = File.OpenText(path);
            if ((info = sr.ReadToEnd()) == null)
            {
                Debug.Log("文件为空");
            }
            sr.Close();
            sr.Dispose();
        }
        else
        {
            Debug.Log("文件不存在");
        }

        return info;
    }

    /// <summary>
    /// 计算RectTransform的屏幕像素大小以及位置
    /// </summary>
    /// <param name="rectTrans"></param>
    /// <returns></returns>
    public static string ConvertRectTransformPixel(RectTransform rectTrans, float fixW = 0, float fixH = 0)
    {
        LitJson.JsonData json = new LitJson.JsonData();
        float x, y, width, height;
        fixW = fixW == 0 ? rectTrans.rect.width : fixW;
        fixH = fixH == 0 ? rectTrans.rect.height : fixH;
        if (GameConfigs.CanvasMathch > 0)
        {
            height = GameConfigs.ScreenSizePhysical.y / GameConfigs.ScreenSizePreset.y * fixH;
            width = fixW / fixH * height;
        }
        else
        {
            width = GameConfigs.ScreenSizePhysical.x / GameConfigs.ScreenSizePreset.x * fixW;
            height = fixH / fixW * width;
        }
        Vector2 screenPosition = RectTransformUtility.WorldToScreenPoint(UIManager.UICamera, rectTrans.position);
        x = screenPosition.x;
        y = screenPosition.y;
        json["x"] = x;
        json["y"] = y;
        json["width"] = width;
        json["height"] = height;
        json["actionId"] = 1;
        return LitJson.JsonMapper.ToJson(json);
    }

    //public void ConverToDate()
    //{
    //    DateTime dateTime
    //}

    public static void AndroidCall(string function, params object[] args)
    {
#if !UNITY_EDITOR && UNITY_ANDROID
        try
        {
            using (AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                using (AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    jo.Call(function, args);
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogFormat("AndroidCall Exception:{0}", ex);
        }
        finally
        {
        }
#endif
    }

    public static T AndroidCall<T>(string function, params object[] args)
    {
#if !UNITY_EDITOR && UNITY_ANDROID
        try
        {
            using (AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                using (AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    return jo.Call<T>(function, args);
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogFormat("AndroidCall Exception:{0}", ex);
        }
        finally
        {
        }

#endif
        return default(T);
    }

    public static void AndroidCallStatic(string className, string function, params object[] args)
    {
#if !UNITY_EDITOR && UNITY_ANDROID
        try
        {
            using (AndroidJavaClass jc = new AndroidJavaClass(className))
            {
                if (jc != null)
                {
                    jc.CallStatic(function, args);
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogFormat("AndroidCallStatic Exception:{0}", ex);
        }
        finally
        {
        }
#endif
    }

    public static T AndroidCallStatic<T>(string className, string function, params object[] args)
    {
#if !UNITY_EDITOR && UNITY_ANDROID
        try
        {
            using (AndroidJavaClass jc = new AndroidJavaClass(className))
            {
                if (jc != null)
                {
                    return jc.CallStatic<T>(function, args);
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogFormat("AndroidCallStatic Exception:{0}", ex);
        }
        finally
        {
        }
#endif
        return default;
    }


    public static string deviceUniqueIdentifier
    {
        get
        {
#if !UNITY_EDITOR && UNITY_ANDROID
            using (AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                using (AndroidJavaObject context = jc.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    using (AndroidJavaClass deviceInfoUtils = new AndroidJavaClass("com.zhexin.commonlib.utils.DeviceInfoUtils"))
                    {
                        return deviceInfoUtils.CallStatic<string>("getImei", context);
                    }
                }
            }
#endif
            return SystemInfo.deviceUniqueIdentifier;
        }
    }
}