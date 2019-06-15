using UnityEngine;
using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections.Generic;

public class SoundManager : MonoSingleton<SoundManager>
{
    private AudioSource _bgmAudioSource;
    private AudioSource _defaultSoundAudioSource;

    private const string BGM_VOLUME_SCALE_KEY = "BgmVolumeScale";
    private const string SOUND_VOLUME_SCALE_KEY = "SoundVolumeScale";
    private const string BGM_TOGGLE_KEY = "BgmToggle";
    private const string SOUND_TOGGLE_KEY = "SoundToggle";

    [SerializeField, LabelText("初始背景音乐强度"), Range(0f, 1f)]
    private float _initBgmVolume = 1f;
    private float _bgmVolumeScale = 1f;
    public float BgmVolumeScale
    {
        get { return _bgmVolumeScale; }
        set
        {
            _bgmVolumeScale = Mathf.Clamp(value, 0f, 1f);
            _bgmAudioSource.volume = _bgmVolumeScale * _initBgmVolume;
            SetPlayerPrefsFloat(BGM_VOLUME_SCALE_KEY, _bgmVolumeScale);
        }
    }

    [SerializeField, LabelText("初始音效强度"), Range(0f, 1f)]
    private float _initSoundVolume = 1f;
    private float _soundVolumeScale = 1f;
    public float SoundVolumeScale
    {
        get { return _bgmVolumeScale; }
        set
        {
            _soundVolumeScale = Mathf.Clamp(value, 0f, 1f);
            _defaultSoundAudioSource.volume = _soundVolumeScale * _initSoundVolume;
            SetPlayerPrefsFloat(SOUND_VOLUME_SCALE_KEY, _soundVolumeScale);
        }
    }

    private bool _bgmToggle;
    public bool BgmToggle
    {
        get { return _bgmToggle; }
        set
        {
            _bgmToggle = value;
            _bgmAudioSource.mute = !_bgmToggle;
            PlayerPrefs.SetInt(BGM_TOGGLE_KEY, value ? 1 : 0);
        }
    }

    private bool _soundToggle;
    public bool SoundToggle
    {
        get { return _soundToggle; }
        set
        {
            _soundToggle = value;
            _defaultSoundAudioSource.mute = !_soundToggle;
            PlayerPrefs.SetInt(SOUND_TOGGLE_KEY, value ? 1 : 0);
        }
    }

    private void Start()
    {
        InitParam();
        InitBgmAudioSource();
        InitDefaultSoundAuidoSource();
    }

    private HashSet<string> delayPlayerPrefsSet = new HashSet<string>();

    private void SetPlayerPrefsFloat(string name, float value)
    {
        if (delayPlayerPrefsSet.Add(name))
        {
            CoroutineAgent.DelayOperation(3f, () =>
            {
                PlayerPrefs.SetFloat(name, value);
                delayPlayerPrefsSet.Remove(name);
            });
        }
    }

    private void InitParam()
    {
        _bgmToggle = PlayerPrefs.GetInt(BGM_TOGGLE_KEY, 1) == 0 ? false : true;
        _soundToggle = PlayerPrefs.GetInt(SOUND_TOGGLE_KEY, 1) == 0 ? false : true;

        _bgmVolumeScale = PlayerPrefs.GetFloat(BGM_VOLUME_SCALE_KEY, 1f);
        _soundVolumeScale = PlayerPrefs.GetFloat(SOUND_VOLUME_SCALE_KEY, 1f);
    }

    private void InitBgmAudioSource()
    {
        _bgmAudioSource = gameObject.AddComponent<AudioSource>();
        _bgmAudioSource.volume = _initBgmVolume * _bgmVolumeScale;
        _bgmAudioSource.mute = !BgmToggle;
        _bgmAudioSource.loop = true;
    }

    private void InitDefaultSoundAuidoSource()
    {
        _defaultSoundAudioSource = gameObject.AddComponent<AudioSource>();
        _defaultSoundAudioSource.volume = _initSoundVolume * _soundVolumeScale;
        _defaultSoundAudioSource.mute = !SoundToggle;
        _defaultSoundAudioSource.loop = false;
    }

    //int count = 1;
    //private void OnGUI()
    //{
    //    if (GUI.Button(new Rect(100, 100, 200, 50), "播放背景音乐"))
    //    {
    //        PlayBgm("bgm");
    //    }

    //    if (GUI.Button(new Rect(300, 100, 200, 50), "play one shot"))
    //    {
    //        PlayVoice((count++).ToString("000") + "_j");
    //    }

    //    if (GUI.Button(new Rect(100, 200, 200, 50), "bgmToggle"))
    //    {
    //        BgmToggle = !BgmToggle;
    //    }
    //    if (GUI.Button(new Rect(300, 200, 200, 50), "soundToggle"))
    //    {
    //        SoundToggle = !SoundToggle;
    //        //ResumeBgm();
    //    }
    //}

    public void PlayBgm(string clipName)
    {
        if (_bgmAudioSource.clip == null || _bgmAudioSource.clip.name != clipName)
        {
            ResourcesManager.LoadAsync<AudioClip>("Sounds/" + clipName, (clip) =>
            {
                _bgmAudioSource.clip = clip;
                _bgmAudioSource.Play();
            }, ExtensionType.wav);
        }
        else if (!_bgmAudioSource.isPlaying)
        {
            _bgmAudioSource.Play();
        }
    }

    public void PauseBgm()
    {
        if (_bgmAudioSource.isPlaying)
        {
            _bgmAudioSource.Pause();
        }
    }

    public void ResumeBgm()
    {
        if (!_bgmAudioSource.isPlaying)
        {
            _bgmAudioSource.Play();
        }
    }

    public void StopBgm(float duration = 0.25f)
    {
        _bgmAudioSource.DOFade(0f, duration).onComplete = () =>
        {
            _bgmAudioSource.Stop();
            _bgmAudioSource.volume = BgmVolumeScale;
        };
    }

    private bool IsSoundTrigger
    {
        get { return SoundToggle && SoundVolumeScale >= 0; }
    }

    public void PlayVoice(string clipName)
    {
        if (!IsSoundTrigger || string.IsNullOrEmpty(clipName))
            return;

        ResourcesManager.LoadAsync<AudioClip>("Sounds/" + clipName, (clip) =>
        {
            _defaultSoundAudioSource.clip = clip;
            _defaultSoundAudioSource.Play();
        }, ExtensionType.wav);
    }

    public void StopVoice(string clipName)
    {
        _defaultSoundAudioSource.clip = null;
    }

    public void PlayOneShot(string clipName, float volumeScale = 1)
    {
        if (!IsSoundTrigger || string.IsNullOrEmpty(clipName))
            return;

        ResourcesManager.LoadAsync<AudioClip>("Sounds/" + clipName, (clip) =>
        {
            _defaultSoundAudioSource.PlayOneShot(clip, volumeScale * SoundVolumeScale);
        }, ExtensionType.wav);
    }

    public void PlayOneShot(AudioClip clip, float volumeScale = 1)
    {
        if (!IsSoundTrigger || !clip)
            return;

        _defaultSoundAudioSource.PlayOneShot(clip, volumeScale);
    }

    public void PlayAtPoint(string clipName, Vector3 position)
    {
        if (!IsSoundTrigger || string.IsNullOrEmpty(clipName))
            return;

        ResourcesManager.LoadAsync<AudioClip>("Sounds/" + clipName, (clip) =>
        {
            AudioSource.PlayClipAtPoint(clip, position, SoundVolumeScale);
        }, ExtensionType.wav);
    }

    /// <summary>
    /// 注册环境音
    /// </summary>
    /// <param name="source"></param>
    public void RegisterAudioSource(AudioSource source)
    {

    }

    /// <summary>
    /// 注销环境音
    /// </summary>
    /// <param name="source"></param>
    public void RemoveAudioSource(AudioSource source)
    {

    }
}
