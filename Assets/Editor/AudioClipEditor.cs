using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[TypeInfoBox("设置文件夹【Assets/Basic/Bundle/Sounds】下所有声音文件压缩以及格式." +
    "\n音频长度大于5：Vorbis & Streaming" +
    "\n音频长度大于2：ADPCM & Compressed In Memory" +
    "\n其他：        PCM & DecompressOnLoad")]
public class AudioClipEditor : BaseEditorScriptable<AudioClipEditor>
{
    /// <summary>
    /// 经常播放的短音频：PCM & Decompress On Load 内存占用大,cpu占用小
    /// 经常播放的中等音频：ADPCM & Compress In Memory 内存占用中等，cpu占用比Vorbis少
    /// 少播放断音频：ADPCM & Compressed In Memory
    /// 少播放中等音频：Vorbis & Compressed In Memory 内存占用大，cpu占用小
    /// </summary>
    [Button("批处理", ButtonSizes.Gigantic)]
    public void Batch()
    {
        //Android设置
        AudioImporterSampleSettings androidSettings = new AudioImporterSampleSettings();
        androidSettings.quality = 0.75f;
        androidSettings.sampleRateSetting = AudioSampleRateSetting.OverrideSampleRate; //采样率控制到22050左右
        androidSettings.sampleRateOverride = 22050;

        //iOS设置
        AudioImporterSampleSettings iOSSettings = new AudioImporterSampleSettings();
        iOSSettings.quality = 0.75f;
        iOSSettings.sampleRateSetting = AudioSampleRateSetting.OverrideSampleRate; //采样率控制到22050左右
        iOSSettings.sampleRateOverride = 22050;

        List<string> files = UnityUtils.Recursive(GameConfigs.AssetRoot + "/Sounds");
        for (int index = 0, len = files.Count - 1; index < len; index++)
        {
            EditorUtility.DisplayProgressBar("Set Audio LoadType & CompressionFormat", string.Format("{0}/{1}", index, len), index * 1.0f / len);
            AudioImporter importer = AssetImporter.GetAtPath(files[index]) as AudioImporter;
            AudioClip clip = ResourcesManager.LoadAssetAtPath<AudioClip>(files[index]);
            importer.preloadAudioData = false;
            if (clip.length > 5)
            {
                androidSettings.loadType = AudioClipLoadType.Streaming;
                androidSettings.compressionFormat = AudioCompressionFormat.Vorbis;

                iOSSettings.loadType = AudioClipLoadType.Streaming;
                iOSSettings.compressionFormat = AudioCompressionFormat.MP3;
            }
            else if (clip.length > 2)
            {
                androidSettings.loadType = AudioClipLoadType.CompressedInMemory;
                androidSettings.compressionFormat = AudioCompressionFormat.ADPCM;

                iOSSettings.loadType = AudioClipLoadType.CompressedInMemory;
                iOSSettings.compressionFormat = AudioCompressionFormat.MP3;
            }
            else
            {
                androidSettings.loadType = AudioClipLoadType.DecompressOnLoad;
                androidSettings.compressionFormat = AudioCompressionFormat.PCM;

                iOSSettings.loadType = AudioClipLoadType.DecompressOnLoad;
                iOSSettings.compressionFormat = AudioCompressionFormat.MP3;
            }

            importer.SetOverrideSampleSettings("Android", androidSettings);
            importer.SetOverrideSampleSettings("ios", iOSSettings);

            importer.SaveAndReimport();
        }
        EditorUtility.ClearProgressBar();
    }
}
