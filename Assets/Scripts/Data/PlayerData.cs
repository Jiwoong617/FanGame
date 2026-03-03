using UnityEngine;
using System;

[CreateAssetMenu(fileName = "PlayerData", menuName = "Scriptable Objects/PlayerData")]
public class PlayerData : UnitData
{
    [Header("Combat Resource")]
    public Sprite unitBackSprite;
    public Sprite attackVFXSprite;
    public Sprite unitSkillSprite;
    public Sprite unitParrySprite;

    public float stamina;
    public float staminaRegen;
    public float fp;

    //스킬 코스트는 1 고정으로 그냥 삭제 했음
    public float skillCoolTime;

    public float dodgeCost = 10f;
    public float parryCost = 7f;

    public float dodgeDuration = 0.3f;
    public float parryDuration = 0.1f;

    public float dodgeCoolTime = 0.5f;
    public float parrayCoolTime = 0.5f;

    [Header("Skill")]
    public Sprite skillIcon;
    public string skillName;
    public string skillDesc;
}

[System.Serializable]
public class PlayerStats : UnitStats
{
    public event Action<float, float> OnStaminaChanged;
    public event Action<float, float> OnFpChanged;
    public event Action<float> OnStaminaRegenChanged;

    // Stat 시스템 적용
    public Stat maxStamina;
    public Stat staminaRegen;
    public Stat maxFp;

    public Stat skillCoolTime;
    public Stat dodgeCost;
    public Stat parryCost;

    // 현재 상태 (단순 변수)
    private float _stamina;
    private float _fp;

    public float dodgeDuration { get; private set; }
    public float parryDuration { get; private set; }
    public float dodgeCoolTime { get; private set; }
    public float parrayCoolTime { get; private set; }


    public float stamina
    {
        get => _stamina;
        set
        {
            _stamina = Mathf.Clamp(value, 0, maxStamina.GetValue());
            OnStaminaChanged?.Invoke(_stamina, maxStamina.GetValue());
        }
    }

    public float fp
    {
        get => _fp;
        set
        {
            _fp = Mathf.Clamp(value, 0, maxFp.GetValue());
            OnFpChanged?.Invoke(_fp, maxFp.GetValue());
        }
    }


    public PlayerStats(PlayerData data) : base(data)
    {
        maxStamina = new Stat(data.stamina);
        staminaRegen = new Stat(data.staminaRegen);
        maxFp = new Stat(data.fp);
        skillCoolTime = new Stat(data.skillCoolTime);
        dodgeCost = new Stat(data.dodgeCost);
        parryCost = new Stat(data.parryCost);

        _stamina = data.stamina;
        _fp = data.fp;
        dodgeDuration = data.dodgeDuration;
        parryDuration = data.parryDuration;
        dodgeCoolTime = data.dodgeCoolTime;
        parrayCoolTime = data.parrayCoolTime;

        maxStamina.OnStatChanged += () => OnStaminaChanged?.Invoke(_stamina, maxStamina.GetValue());
        staminaRegen.OnStatChanged += () => OnStaminaRegenChanged?.Invoke(staminaRegen.GetValue());
        maxFp.OnStatChanged += () => OnFpChanged?.Invoke(_fp, maxFp.GetValue());

    }
}