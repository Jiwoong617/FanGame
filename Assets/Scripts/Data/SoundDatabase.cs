using System;
using UnityEngine;

public enum BGM
{
    None = 0,
    Title,
    Stage1,
    Stage2,
    Stage3,
    Rest,
    Boss,
    Ending,
}

public enum SFX
{
    None = 0,

    UIClick,

    RewardGet,
}



[CreateAssetMenu(fileName = "SoundDatabase", menuName = "Sound/SoundDatabase")]
public class SoundDatabase : ScriptableObject
{
    [Serializable]
    public struct BGMEntry
    {
        public BGM id;
        public AudioClip clip;
    }

    [Serializable]
    public struct SFXEntry
    {
        public SFX id;
        public AudioClip clip;
    }

    [Header("BGM")]
    public BGMEntry[] bgmEntries;

    [Header("SFX")]
    public SFXEntry[] sfxEntries;
}
