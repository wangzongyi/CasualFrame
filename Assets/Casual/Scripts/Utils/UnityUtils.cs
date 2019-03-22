using UnityEngine;
using System.Collections;
using System.IO;

public static class UnityUtils {

    public static string GetAssetURL(string abName)
    {
        if (File.Exists(Path.Combine(Application.persistentDataPath, abName)))
        {
            return Path.Combine(Application.persistentDataPath, abName);
        }
        else
        {
            return Path.Combine(Application.streamingAssetsPath, abName);
        }
    }

    public static RenderTexture RenderToTexture(this Camera camera, int width, int heigh)
    {
        RenderTexture rTexture = camera.targetTexture;

        if(rTexture == null)
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

    public static void SetPositionX()
    {

    }

    public static void SetPositionY()
    {

    }

    public static void SetPositionZ()
    {

    }

    public static void SetPositionXY(Transform trans, Vector2 vector2)
    {
        Vector3 tempVector3 = new Vector3(vector2.x, vector2.y, trans.position.z);
        trans.position = tempVector3;
    }
}