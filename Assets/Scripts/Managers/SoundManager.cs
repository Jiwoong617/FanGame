using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Audio;

public class SoundManager
{
    private const int SFX_POOL_INITIAL_SIZE = 8;
    private const float BGM_FADE_DURATION   = 1.0f;

    private AudioMixer audioMixer;
    private AudioSource bgmSource;
    private GameObject soundRoot;
    private Tween bgmFadeTween;

    // SFX AudioSource 풀: 동시에 여러 효과음을 독립적으로 재생
    private readonly List<AudioSource> sfxPool = new List<AudioSource>();
    private readonly Dictionary<BGM, AudioClip> bgmClips = new Dictionary<BGM, AudioClip>();
    private readonly Dictionary<SFX, AudioClip> sfxClips = new Dictionary<SFX, AudioClip>();

    public float MasterVolume { get; private set; } = 1f;
    public float BGMVolume    { get; private set; } = 1f;
    public float SFXVolume    { get; private set; } = 1f;

    public void Init()
    {
        audioMixer = Resources.Load<AudioMixer>("Sounds/MasterMixer");
        if (audioMixer == null)
        {
            Debug.LogError("[SoundManager] AudioMixer 없음");
            return;
        }

        LoadDatabase();

        soundRoot = new GameObject("@Sound");
        Object.DontDestroyOnLoad(soundRoot);

        bgmSource = soundRoot.AddComponent<AudioSource>();
        bgmSource.outputAudioMixerGroup = audioMixer.FindMatchingGroups("BGM")[0];
        bgmSource.loop = true;

        for (int i = 0; i < SFX_POOL_INITIAL_SIZE; i++)
            AddSfxSourceToPool();

        MasterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        BGMVolume    = PlayerPrefs.GetFloat("BGMVolume",    1f);
        SFXVolume    = PlayerPrefs.GetFloat("SFXVolume",    1f);

        SetMasterVolume(MasterVolume);
        SetBGMVolume(BGMVolume);
        SetSFXVolume(SFXVolume);
    }

    public void PlayBGM(BGM id, float pitch = 1.0f)
    {
        if (!bgmClips.TryGetValue(id, out AudioClip clip))
        {
            Debug.LogWarning($"[SoundManager] BGM 미등록: {id}  — SoundDatabase를 확인하세요.");
            return;
        }
        PlayBGM(clip, pitch);
    }

    public void PlayBGM(AudioClip clip, float pitch = 1.0f)
    {
        if (clip == null) return;
        if (bgmSource.clip == clip && bgmSource.isPlaying) return;

        bgmFadeTween?.Kill();

        bgmFadeTween = DOTween.Sequence()
            .Append(bgmSource.DOFade(0f, BGM_FADE_DURATION))
            .AppendCallback(() =>
            {
                bgmSource.Stop();
                bgmSource.clip   = clip;
                bgmSource.pitch  = pitch;
                bgmSource.volume = 0f;
                bgmSource.Play();
            })
            .Append(bgmSource.DOFade(1f, BGM_FADE_DURATION));
    }

    public void StopBGM()
    {
        bgmFadeTween?.Kill();
        bgmFadeTween = bgmSource.DOFade(0f, BGM_FADE_DURATION)
            .OnComplete(() =>
            {
                bgmSource.Stop();
                bgmSource.volume = 1f;
            });
    }

    public void PlaySFX(SFX id, float pitch = 1.0f)
    {
        if (!sfxClips.TryGetValue(id, out AudioClip clip))
        {
            Debug.LogWarning($"[SoundManager] SFX 미등록: {id}  — SoundDatabase를 확인하세요.");
            return;
        }
        PlaySFX(clip, pitch);
    }

    public void PlaySFX(AudioClip clip, float pitch = 1.0f)
    {
        if (clip == null) return;

        AudioSource source = GetAvailableSfxSource();
        source.pitch = pitch;
        source.clip  = clip;
        source.Play();
    }

    #region 볼륨 설정
    public void SetMasterVolume(float volume)
    {
        MasterVolume = volume;
        SetMixerVolume("Master", volume);
        PlayerPrefs.SetFloat("MasterVolume", volume);
    }

    public void SetBGMVolume(float volume)
    {
        BGMVolume = volume;
        SetMixerVolume("BGM", volume);
        PlayerPrefs.SetFloat("BGMVolume", volume);
    }

    public void SetSFXVolume(float volume)
    {
        SFXVolume = volume;
        SetMixerVolume("SFX", volume);
        PlayerPrefs.SetFloat("SFXVolume", volume);
    }

    private void SetMixerVolume(string parameterName, float volume)
    {
        if (audioMixer == null) return;

        // 볼륨이 0이면 음소거(-80dB), 아니면 로그 스케일로 변환
        // Mathf.Log10(volume) * 20 : 0.1 -> -20dB, 1 -> 0dB
        float db = (volume <= 0.0001f) ? -80f : Mathf.Log10(volume) * 20f;
        audioMixer.SetFloat(parameterName, db);
    }
    #endregion

    #region 유틸리티
    private void LoadDatabase()
    {
        SoundDatabase db = Resources.Load<SoundDatabase>("Sounds/SoundDatabase");
        if (db == null)
        {
            Debug.LogWarning("[SoundManager] SoundDatabase 없음 — " +
                             "Assets > Create > Sound > SoundDatabase 로 생성 후 " +
                             "Resources/Sounds/ 폴더에 저장하세요.");
            return;
        }

        foreach (SoundDatabase.BGMEntry entry in db.bgmEntries)
        {
            if (entry.id == BGM.None || entry.clip == null) continue;
            bgmClips[entry.id] = entry.clip;
        }

        foreach (SoundDatabase.SFXEntry entry in db.sfxEntries)
        {
            if (entry.id == SFX.None || entry.clip == null) continue;
            sfxClips[entry.id] = entry.clip;
        }
    }

    private AudioSource GetAvailableSfxSource()
    {
        // 재생이 끝난 소스 재사용
        foreach (AudioSource src in sfxPool)
        {
            if (!src.isPlaying) return src;
        }

        // 풀에 여유가 없으면 동적으로 1개 추가
        Debug.LogWarning("[SoundManager] SFX 풀 소진 — 소스를 동적으로 추가합니다.");
        return AddSfxSourceToPool();
    }

    private AudioSource AddSfxSourceToPool()
    {
        AudioSource src = soundRoot.AddComponent<AudioSource>();
        src.outputAudioMixerGroup = audioMixer.FindMatchingGroups("SFX")[0];
        src.loop = false;
        sfxPool.Add(src);
        return src;
    }
    #endregion
}