using UnityEngine;
using System.Collections;
using System.IO;

public struct RectInt{
    public int x;
    public int y;
    public int w;
    public int h;
}

public static class UnityUtils {

    public static Color float2Color(Color clr, float[] vals) {
        if (vals == null)
            return Color.white;
        clr.r = vals[0];
        clr.g = vals[1];
        clr.b = vals[2];
        clr.a = vals[3];
        return clr;
    }

    public static float[] Int256ToFloatColor(int[] ic) {
        float[] fc = new float[ic.Length];
        for (int i = 0; i < ic.Length; i++) {
            fc[i] = ic[i] / 255f;
        }
        return fc;
    }

    public static void ChangeLayer(Transform trans, int layer){
        trans.gameObject.layer = layer;
        Transform ctrans = null;
        for (int i = 0; i < trans.childCount; i++) {
            ctrans = trans.GetChild(i);
            if (ctrans != null) {
                ChangeLayer(ctrans, layer);
            }
        }
    }

    public static T CheckAndAddMono<T>(GameObject go) where T : MonoBehaviour {

        T t = go.GetComponent<T>();
        if (t == null)
            t = go.AddComponent<T>();
        return t;
    }

    //
    public static int GetRenderTexturePtr(RenderTexture rt) {
        return rt.GetNativeTexturePtr().ToInt32();
    }

    public static Texture2D CreateTextureFromPtr(int ptrValue, int width, int height) {
        System.IntPtr ptr = new System.IntPtr(ptrValue);
        Texture2D t2d = Texture2D.CreateExternalTexture(width, height, TextureFormat.RGBA32, false, true, ptr);
        return t2d;
    }


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
        using (FileStream fs = new FileStream(fileUrl, FileMode.OpenOrCreate, FileAccess.ReadWrite))
        {
            byte[] buffur = new byte[fs.Length];
            using (BinaryWriter bw = new BinaryWriter(fs))
            {
                bw.Write(buffur);
                bw.Close();
            }
            return buffur;
        }
    }
}