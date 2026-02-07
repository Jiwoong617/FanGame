using UnityEngine;
using System;

/// <summary>
/// Sciptable Objects
/// </summary>

[CreateAssetMenu(fileName = "UnitData", menuName = "Scriptable Objects/UnitData")]
public class UnitData : ScriptableObject
{
    [Header("Basic Info")]
    public string unitName;
    public GameObject prefab;
    public Sprite unitSprite;

    [Header("Combat Stats")]
    public float hp;
    public float defense;
    public float attackDamage;
    public float attackSpeed;

    [Header("Description"), TextArea]
    public string unitDescription;
}


/// <summary>
/// 복사용 클래스
/// </summary>

[System.Serializable]
public class UnitStats
{
    public event Action<float, float> OnHpChanged;
    public event Action<float> OnDefenseChanged;
    public event Action<float> OnAttackDamageChanged;
    public event Action<float> OnAttackSpeedChanged;

    private float _maxHp;
    private float _hp;
    private float _defense;
    private float _attackDamage;
    private float _attackSpeed;

    public float maxHp
    {
        get => _maxHp;
        set
        {
            _maxHp = value;
            OnHpChanged?.Invoke(_hp, _maxHp);
        }
    }

    public float hp
    {
        get => _hp;
        set
        {
            _hp = value;
            OnHpChanged?.Invoke(_hp, _maxHp);
        }
    }

    public float defense
    {
        get => _defense;
        set
        {
            _defense = value;
            OnDefenseChanged?.Invoke(_defense);
        }
    }

    public float attackDamage
    {
        get => _attackDamage;
        set
        {
            _attackDamage = value;
            OnAttackDamageChanged?.Invoke(_attackDamage);
        }
    }

    public float attackSpeed
    {
        get => _attackSpeed;
        set
        {
            _attackSpeed = value;
            OnAttackSpeedChanged?.Invoke(_attackSpeed);
        }
    }

    public UnitStats(UnitData data)
    {
        _maxHp = data.hp;
        _hp = data.hp;
        _defense = data.defense;
        _attackDamage = data.attackDamage;
        _attackSpeed = data.attackSpeed;
    }
}