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

    private float _maxStamina;
    private float _stamina;
    private float _staminaRegen;

    private float _maxFp;
    private float _fp;

    public float dodgeCost { get; private set; }
    public float parryCost { get; private set; }

    public float maxStamina
    {
        get => _maxStamina;
        set
        {
            _maxStamina = value;
            OnStaminaChanged?.Invoke(_stamina, _maxStamina);
        }
    }

    public float stamina
    {
        get => _stamina;
        set
        {
            _stamina = value;
            OnStaminaChanged?.Invoke(_stamina, _maxStamina);
        }
    }

    public float staminaRegen
    {
        get => _staminaRegen;
        set
        {
            _staminaRegen = value;
            OnStaminaRegenChanged?.Invoke(_staminaRegen);
        }
    }

    public float maxFp
    {
        get => _maxFp;
        set
        {
            _maxFp = value;
            OnFpChanged?.Invoke(_fp, _maxFp);
        }
    }

    public float fp
    {
        get => _fp;
        set
        {
            _fp = value;
            OnFpChanged?.Invoke(_fp, _maxFp);
        }
    }


    public PlayerStats(PlayerData data) : base(data)
    {
        _maxStamina = data.stamina;
        _stamina = data.stamina;
        _staminaRegen = data.staminaRegen;
        _maxFp = data.fp;
        _fp = data.fp;
        dodgeCost = data.dodgeCost;
        parryCost = data.parryCost;
    }
}