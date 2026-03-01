using UnityEngine;
using UnityEngine.Audio; // [필수] 오디오 믹서 사용

public class SoundManager
{
    private AudioMixer audioMixer;
    private AudioSource bgmSource;
    private AudioSource sfxSource;

    // 볼륨 데이터 (0.0 ~ 1.0)
    public float MasterVolume { get; private set; } = 1f;
    public float BGMVolume { get; private set; } = 1f;
    public float SFXVolume { get; private set; } = 1f;

    public void Init()
    {
        audioMixer = Resources.Load<AudioMixer>("Sounds/MasterMixer");
        if (audioMixer == null)
        {
            Debug.LogError("AudioMixer 없음");
            return;
        }

        GameObject root = new GameObject("@Sound");
        Object.DontDestroyOnLoad(root);

        bgmSource = root.AddComponent<AudioSource>();
        bgmSource.outputAudioMixerGroup = audioMixer.FindMatchingGroups("BGM")[0]; // BGM 그룹 연결
        bgmSource.loop = true;

        sfxSource = root.AddComponent<AudioSource>();
        sfxSource.outputAudioMixerGroup = audioMixer.FindMatchingGroups("SFX")[0]; // SFX 그룹 연결
        sfxSource.loop = false;

        MasterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        BGMVolume = PlayerPrefs.GetFloat("BGMVolume", 1f);
        SFXVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);

        SetMasterVolume(MasterVolume);
        SetBGMVolume(BGMVolume);
        SetSFXVolume(SFXVolume);
    }

    public void PlayBGM(string path, float pitch = 1.0f)
    {
        AudioClip clip = Resources.Load<AudioClip>($"Sounds/BGM/{path}");
        PlayBGM(clip, pitch);
    }

    public void PlayBGM(AudioClip clip, float pitch = 1.0f)
    {
        if (clip == null) return;
        if (bgmSource.isPlaying) bgmSource.Stop();

        bgmSource.clip = clip;
        bgmSource.pitch = pitch;
        bgmSource.Play();
    }

    public void StopBGM() => bgmSource.Stop();

    public void PlaySFX(string path, float pitch = 1.0f)
    {
        AudioClip clip = Resources.Load<AudioClip>($"Sounds/SFX/{path}");
        PlaySFX(clip, pitch);
    }

    public void PlaySFX(AudioClip clip, float pitch = 1.0f)
    {
        if (clip == null) return;
        sfxSource.pitch = pitch;
        sfxSource.PlayOneShot(clip); //TODO: 일단 이거로 중첩 재생 했음 나중에 풀링으로 바꿔도 됨
    }

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
}