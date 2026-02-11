using UnityEngine;
using System;

[CreateAssetMenu(fileName = "PlayerData", menuName = "Scriptable Objects/PlayerData")]
public class PlayerData : UnitData
{
    [Header("Combat Resource")]
    public float stamina;
    public float staminaRegen;
    public float fp;

    public float dodgeCost;
    public float parryCost;
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

    // 현재 상태 (단순 변수)
    private float _stamina;
    private float _fp;

    public float dodgeCost { get; private set; }
    public float parryCost { get; private set; }

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

        _stamina = data.stamina;
        _fp = data.fp;
        dodgeCost = data.dodgeCost;
        parryCost = data.parryCost;

        maxStamina.OnStatChanged += () => OnStaminaChanged?.Invoke(_stamina, maxStamina.GetValue());
        staminaRegen.OnStatChanged += () => OnStaminaRegenChanged?.Invoke(staminaRegen.GetValue());
        maxFp.OnStatChanged += () => OnFpChanged?.Invoke(_fp, maxFp.GetValue());
    }
}