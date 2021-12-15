using UnityEngine;
using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System;
using System.Collections;

public class SoundSequence
{
    public object ID;
    public Action OnStart;
    public Action OnComplete;
    public SoundEvent[] Sequence;
    public Coroutine Coroutine;
    public MonoBehaviour MonoBehaviour;
}

public class SoundEvent
{
    public string ClipName;
    public Action OnStart;
    public Action OnComplete;
    public SoundType SoundType = SoundType.Voice;
}

public enum SoundType
{
    Voice,
    OneShot,
    BGM,
    Placeholder,//空音频 占位
}

public class SoundManager : MonoSingleton<SoundManager>
{
    public class AudioEvent
    {
        public string ClipName;
        public Action Callback;
        public MonoBehaviour monoBehaviour;
    }

    public const string BGM_VOLUME_SCALE_KEY = "BgmVolumeScale";
    public const string SOUND_VOLUME_SCALE_KEY = "SoundVolumeScale";
    private const string BGM_TOGGLE_KEY = "BgmToggle";
    private const string SOUND_TOGGLE_KEY = "SoundToggle";

    private AudioSource[] _bgmSources;

    [SerializeField, LabelText("初始背景音乐强度"), Range(0f, 1f)]
    private float _initBgmVolume = 1f;
    private float _bgmVolumeScale = 1f;
    public float BgmVolumeScale
    {
        get { return _bgmVolumeScale; }
        set
        {
            _bgmVolumeScale = Mathf.Clamp(value, 0f, 1f);
            SetBGMVolume(_bgmVolumeScale * _initBgmVolume);
            SetPlayerPrefsFloat(BGM_VOLUME_SCALE_KEY, _bgmVolumeScale);
        }
    }

    [SerializeField, LabelText("初始音效强度"), Range(0f, 1f)]
    private float _initSoundVolume = 1f;
    private float _soundVolumeScale = 1f;
    public float SoundVolumeScale
    {
        get { return _soundVolumeScale; }
        set
        {
            _soundVolumeScale = Mathf.Clamp(value, 0f, 1f);
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
            SetBGMMute(!_bgmToggle);
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
            PlayerPrefs.SetInt(SOUND_TOGGLE_KEY, value ? 1 : 0);
        }
    }

    private GameObject audioSourcePrefab;
    private Dictionary<string, float> delayPlayerPrefsSet = new Dictionary<string, float>();
    private Dictionary<MonoBehaviour, AudioSource> audioSourcePools = new Dictionary<MonoBehaviour, AudioSource>();
    private Dictionary<object, SoundSequence> SoundSequencePools = new Dictionary<object, SoundSequence>();


    private void Start()
    {
        InitParam();
        InitBgmAudioSource();
    }

    private void SetPlayerPrefsFloat(string name, float value)
    {
        if (!delayPlayerPrefsSet.ContainsKey(name))
        {
            CoroutineAgent.WaitForSeconds(0.5f, () =>
            {
                PlayerPrefs.SetFloat(name, delayPlayerPrefsSet[name]);
                delayPlayerPrefsSet.Remove(name);
            });
        }
        delayPlayerPrefsSet[name] = value;
    }

    private void SetBGMVolume(float volume)
    {
        for (int index = 0, len = _bgmSources.Length; index < len; index++)
        {
            _bgmSources[index].volume = volume;
        }
    }

    private void SetBGMMute(bool mute)
    {
        for (int index = 0, len = _bgmSources.Length; index < len; index++)
        {
            _bgmSources[index].mute = mute;
        }
    }

    private void InitParam()
    {
        audioSourcePrefab = gameObject.GetChild("AudioSourcePrefab");

        _bgmToggle = PlayerPrefs.GetInt(BGM_TOGGLE_KEY, 1) == 0 ? false : true;
        _soundToggle = PlayerPrefs.GetInt(SOUND_TOGGLE_KEY, 1) == 0 ? false : true;

        _bgmVolumeScale = PlayerPrefs.GetFloat(BGM_VOLUME_SCALE_KEY, 1f);
        _soundVolumeScale = PlayerPrefs.GetFloat(SOUND_VOLUME_SCALE_KEY, 1f);
    }

    private void InitBgmAudioSource()
    {
        _bgmSources = new AudioSource[2];
        for (int index = 0, len = _bgmSources.Length; index < len; index++)
        {
            _bgmSources[index] = gameObject.AddComponent<AudioSource>();
            _bgmSources[index].volume = _initBgmVolume * _bgmVolumeScale;
            _bgmSources[index].mute = !BgmToggle;
            _bgmSources[index].loop = true;
        }
    }

    private AudioSource CreateAudioSource(MonoBehaviour monoBehaviour)
    {
        AudioSource audioSource = null;
        if (audioSourcePools.ContainsKey(monoBehaviour) && audioSourcePools[monoBehaviour])
        {
            audioSource = audioSourcePools[monoBehaviour];
        }
        else
        {
            GameObject source = GameObjectAgent.FetchObject<NullPool>(transform, audioSourcePrefab, monoBehaviour);
            audioSource = source.GetComponent<AudioSource>();
            audioSourcePools[monoBehaviour] = audioSource;
        }
        audioSource.name = monoBehaviour.name;
        SetAudioSourceParam(audioSource);
        return audioSource;
    }

    private void SetAudioSourceParam(AudioSource audioSource)
    {
        audioSource.volume = _initSoundVolume * _soundVolumeScale;
        audioSource.mute = !SoundToggle;
        audioSource.loop = false;
    }

    public void PlayBgm(string clipName)
    {
        PlayBgm(clipName, 0);
    }

    public void PlayBgm(string clipName, int track)
    {
        if (track >= _bgmSources.Length)
        {
            DebugUtils.LogError("不存在：{0}声道，请检查代码。", track);
            return;
        }

        DOTween.Kill("StopBgm", true);

        AudioSource bgmSource = _bgmSources[track];

        if (bgmSource.clip == null || bgmSource.clip.name != clipName)
        {
            ResourcesManager.LoadAsync<AudioClip>(string.Format(AssetPath.SOUND_SHORT_ROOT, clipName), (clip) =>
            {
                bgmSource.clip = clip;
                bgmSource.Play();
            }, ExtensionType.mp3, this);
        }
        else if (!bgmSource.isPlaying)
        {
            bgmSource.Play();
        }
    }

    public void PauseAllBgm()
    {
        for (int index = 0, len = _bgmSources.Length; index < len; index++)
        {
            PauseBgm(_bgmSources[index]);
        }
    }

    public void PauseBgm(int track = 0)
    {
        if (track >= _bgmSources.Length)
        {
            DebugUtils.LogError("不存在：{0}声道，请检查代码。", track);
            return;
        }

        PauseBgm(_bgmSources[track]);
    }

    private void PauseBgm(AudioSource source)
    {
        if (source.isPlaying)
        {
            source.Pause();
        }
    }

    public void ResumeAllBgm()
    {
        for (int index = 0, len = _bgmSources.Length; index < len; index++)
        {
            ResumeBgm(_bgmSources[index]);
        }
    }

    public void ResumeBgm(int track)
    {
        if (track >= _bgmSources.Length)
        {
            DebugUtils.LogError("不存在：{0}声道，请检查代码。", track);
            return;
        }
        ResumeBgm(_bgmSources[track]);
    }

    private void ResumeBgm(AudioSource source)
    {
        if (!source.isPlaying)
        {
            source.Play();
        }
    }

    public void StopAllBgm(float duration = 0.25f)
    {
        for (int index = 0, len = _bgmSources.Length; index < len; index++)
        {
            StopBgm(_bgmSources[index], duration);
        }
    }

    public void StopBgm(int track = 0, float duration = 0.25f)
    {
        if (track >= _bgmSources.Length)
        {
            DebugUtils.LogError("不存在：{0}声道，请检查代码。", track);
            return;
        }
        StopBgm(_bgmSources[track], duration);
    }

    private void StopBgm(AudioSource source, float duration)
    {
        source.Stop();
        source.volume = BgmVolumeScale * _initBgmVolume;
    }

    private bool IsSoundTrigger
    {
        get { return SoundToggle && SoundVolumeScale >= 0; }
    }

    /// <summary>
    /// 后面播放的音频会停止上个音频
    /// </summary>
    /// <param name="clipName"></param>
    public void PlayVoice(string clipName)
    {
        PlayVoice(clipName, this);
    }

    public void PlaySequence(SoundSequence sequence)
    {
        KillSequence(sequence.ID, false);

        if (!SoundSequencePools.ContainsKey(sequence.ID))
            SoundSequencePools[sequence.ID] = sequence;
        sequence.Coroutine = CoroutineAgent.StartCoroutine(CoPlaySequence(sequence), sequence.MonoBehaviour);
    }

    private IEnumerator CoPlaySequence(SoundSequence sequence)
    {
        sequence.OnStart?.Invoke();
        foreach (var soundEvent in sequence.Sequence)
        {
            soundEvent.OnStart?.Invoke();
            if (soundEvent.SoundType == SoundType.Voice)
            {
                yield return CoroutineAgent.StartCoroutine(CoPlayVoice(soundEvent.ClipName, sequence.MonoBehaviour, soundEvent.OnComplete), sequence.MonoBehaviour);
            }
            else if (soundEvent.SoundType == SoundType.OneShot)
            {
                yield return CoroutineAgent.StartCoroutine(CoPlayOneShot(soundEvent.ClipName, sequence.MonoBehaviour, soundEvent.OnComplete, 1f), sequence.MonoBehaviour);
            }
            else if (soundEvent.SoundType == SoundType.Placeholder)
            {
                float.TryParse(soundEvent.ClipName, out float delay);
                yield return Yielders.WaitForSeconds(delay);
                soundEvent.OnComplete?.Invoke();
            }
            else
            {
                PlayBgm(soundEvent.ClipName);
                yield return null;
            }
        }
        KillSequence(sequence.ID, true);
    }

    public void KillSequence(object id, bool complete)
    {
        if (SoundSequencePools.ContainsKey(id))
        {
            SoundSequence sequence = SoundSequencePools[id];
            if (sequence.Coroutine != null) StopCoroutine(sequence.Coroutine);
            SoundSequencePools.Remove(sequence.ID);
            if (complete) sequence.OnComplete?.Invoke();
        }
    }

    /// <summary>
    /// 后面播放的音频会停止上个音频
    /// </summary>
    /// <param name="clipName"></param>
    public Coroutine PlayVoice(string clipName, MonoBehaviour monoBehaviour, Action callback = null)
    {
        if (!IsSoundTrigger || string.IsNullOrEmpty(clipName))
        {
            callback?.Invoke();
            return null;
        }

        return CoroutineAgent.StartCoroutine(CoPlayVoice(clipName, monoBehaviour, callback), monoBehaviour);
    }

    private IEnumerator CoPlayVoice(string clipName, MonoBehaviour monoBehaviour, Action callback = null)
    {
        if (!IsSoundTrigger || string.IsNullOrEmpty(clipName))
        {
            callback?.Invoke();
            yield break;
        }

        AudioClip thisClip = null;
        yield return ResourcesManager.LoadAsync<AudioClip>(string.Format(AssetPath.SOUND_SHORT_ROOT, clipName), (clip) =>
        {
            thisClip = clip;
        }, ExtensionType.mp3, this);

        PlayVoice(thisClip, null, monoBehaviour);
        yield return Yielders.WaitForSeconds(thisClip ? thisClip.length : 0);
        callback?.Invoke();
    }

    public AudioSource PlayVoice(AudioClip clip)
    {
        return PlayVoice(clip, null, null);
    }
    public AudioSource PlayVoice(AudioClip clip, Action callback, MonoBehaviour monoBehaviour)
    {
        monoBehaviour = monoBehaviour ?? (this);
        AudioSource audioSource = CreateAudioSource(monoBehaviour);

        if (!IsSoundTrigger || !clip)
        {
            callback?.Invoke();
            return audioSource;
        }

        audioSource.clip = clip;
        audioSource.Play();

        if (monoBehaviour && monoBehaviour.gameObject.activeSelf && callback != null)
        {
            CoroutineAgent.WaitForSeconds(clip.length, callback, monoBehaviour);
        }

        return audioSource;
    }

    public void PauseVoice(MonoBehaviour monoBehaviour)
    {
        monoBehaviour = monoBehaviour ?? (this);
        AudioSource audioSource = CreateAudioSource(monoBehaviour);

        if (audioSource.isPlaying)
        {
            audioSource.Pause();
        }
    }

    public void ResumeVoice(MonoBehaviour monoBehaviour)
    {
        monoBehaviour = monoBehaviour ?? (this);
        AudioSource audioSource = CreateAudioSource(monoBehaviour);

        if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }

    public void StopVoice(MonoBehaviour monoBehaviour)
    {
        monoBehaviour = monoBehaviour ?? (this);
        AudioSource audioSource = CreateAudioSource(monoBehaviour);

        audioSource.Stop();
        audioSource.clip = null;
    }

    public void PlayOneShot(string clipName)
    {
        PlayOneShot(clipName, this);
    }

    public void PlayOneShot(string clipName, MonoBehaviour monoBehaviour, Action callback = null)
    {
        PlayOneShot(clipName, monoBehaviour, callback, 1);
    }

    /// <summary>
    /// 后面播放的音频不会停止上个音频
    /// </summary>
    /// <param name="clipName"></param>
    private Coroutine PlayOneShot(string clipName, MonoBehaviour monoBehaviour, Action callback, float volumeScale)
    {
        if (!IsSoundTrigger || string.IsNullOrEmpty(clipName))
        {
            callback?.Invoke();
            return null;
        }

        return CoroutineAgent.StartCoroutine(CoPlayOneShot(clipName, monoBehaviour, callback, volumeScale), monoBehaviour);
    }

    private IEnumerator CoPlayOneShot(string clipName, MonoBehaviour monoBehaviour, Action callback, float volumeScale)
    {
        if (!IsSoundTrigger || string.IsNullOrEmpty(clipName))
        {
            callback?.Invoke();
            yield break;
        }

        AudioClip audioClip = null;
        yield return ResourcesManager.LoadAsync<AudioClip>(string.Format(AssetPath.SOUND_SHORT_ROOT, clipName), (clip) =>
        {
            audioClip = clip;
        }, ExtensionType.mp3, this);

        if (audioClip)
        {
            monoBehaviour = monoBehaviour ?? (this);
            AudioSource audioSource = CreateAudioSource(monoBehaviour);
            audioSource.PlayOneShot(audioClip, volumeScale * SoundVolumeScale);
            yield return Yielders.WaitForSeconds(audioClip ? audioClip.length : 0f);
        }

        callback?.Invoke();
    }
}