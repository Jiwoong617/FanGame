using System;
using UnityEngine;

public enum BGM
{
    None = 0,
    Title,
    Stage1,
    Stage2,
    Stage3,
    Battle,
    Rest,
    Die,
    Ending,
}

public enum SFX
{
    None = 0,

    //Battle
    Hit,
    Dodge,
    Parry,
    Heal,
    Summon,
    
    //Player Skill
    HasiyoIce,
    HasiyoWood,
    MoneSkll,
    RoseSkill,

    //Buff/Debuff
    Buff,
    Debuff,
    IronFortress,


    //UI
    UIClick,
    Rest_Sleep,
    Rest_Train,
    Rest_Meditate,
}



[CreateAssetMenu(fileName = "SoundDatabase", menuName = "Sound/SoundDatabase")]
public class SoundDatabase : ScriptableObject
{
    [Serializable]
    public struct BGMEntry
    {
        public BGM id;
        public AudioClip clip;
        public GameState state; // 자동 재생될 GameState
    }

    [Serializable]
    public struct SFXEntry
    {
        public SFX id;
        public AudioClip clip;
    }

    [Header("BGM")]
    public BGMEntry[] bgmEntries;

    [Header("스테이지 BGM (인덱스 = CurrentStageIndex)")]
    public BGM[] stageBGMs;

    [Header("SFX")]
    public SFXEntry[] sfxEntries;
}
