using UnityEngine;
using System;
using System.Collections.Generic;

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
    public Sprite unitBasicAttackSprite;
    public Sprite unitDeadSprite;
    public AttackVFXType attackVFXType;

    [Header("Combat Stats")]
    public float hp;
    public float defense;
    public float attackDamage;
    public float attackSpeed = 1;
    public float criticalChance;
    public float criticalDamage;

    [Header("Description"), TextArea]
    public string unitDescription;

    [Header("Abilities")]
    [SerializeReference, SerializeReferenceDropdown]
    public List<Ability> startingAbilities = new List<Ability>();
}


/// <summary>
/// 복사용 클래스
/// </summary>

[System.Serializable]
public class UnitStats
{
    // 값이 변경될 때 UI 갱신 등을 위한 이벤트
    public event Action<float, float> OnHpChanged;
    public event Action<float> OnDefenseChanged;
    public event Action<float> OnAttackDamageChanged;
    public event Action<float> OnAttackSpeedChanged;
    public event Action<float> OnCriticalChanceChanged;
    public event Action<float> OnCriticalDamageChanged;

    // 능력치 (Stat 시스템 적용)
    public Stat maxHp;
    public Stat defense;
    public Stat attackDamage;
    public Stat attackSpeed;
    public Stat criticalChance;
    public Stat criticalDamage;

    private float _lastMaxHp;
    // 얘는 상태니까 그냥 놔둔거
    private float _hp;

    public float hp
    {
        get => _hp;
        set
        {
            _hp = Mathf.Clamp(value, 0, maxHp.GetValue());
            OnHpChanged?.Invoke(_hp, maxHp.GetValue());
        }
    }

    public UnitStats(UnitData data)
    {
        maxHp = new Stat(data.hp);
        defense = new Stat(data.defense);
        attackDamage = new Stat(data.attackDamage);
        attackSpeed = new Stat(data.attackSpeed);
        criticalChance = new Stat(data.criticalChance);
        criticalDamage = new Stat(data.criticalDamage);
        _hp = data.hp;

        maxHp.OnStatChanged += () => OnHpChanged?.Invoke(_hp, maxHp.GetValue());
        defense.OnStatChanged += () => OnDefenseChanged?.Invoke(defense.GetValue());
        attackDamage.OnStatChanged += () => OnAttackDamageChanged?.Invoke(attackDamage.GetValue());
        attackSpeed.OnStatChanged += () => OnAttackSpeedChanged?.Invoke(attackSpeed.GetValue());
        criticalChance.OnStatChanged += () => OnCriticalChanceChanged?.Invoke(criticalChance.GetValue());
        criticalDamage.OnStatChanged += () => OnCriticalDamageChanged?.Invoke(criticalDamage.GetValue());

        _lastMaxHp = maxHp.GetValue();
        maxHp.OnStatChanged += HandleMaxHpChange;
    }

    private void HandleMaxHpChange()
    {
        float newMaxHp = maxHp.GetValue();

        if (newMaxHp > _lastMaxHp)
        {
            float diff = newMaxHp - _lastMaxHp;
            _hp += diff;
            OnHpChanged?.Invoke(_hp, newMaxHp);
        }
        else if (newMaxHp < _lastMaxHp)
        {
            if (_hp > newMaxHp)
            {
                _hp = newMaxHp;
                OnHpChanged?.Invoke(_hp, newMaxHp);
            }
        }

        _lastMaxHp = newMaxHp;
    }
}