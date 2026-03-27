using System.Collections.Generic;
using UnityEngine;

public enum CombatEvent
{
    OnBattleStart,
    OnBattleEnd,
    OnAttack,
    OnBeforeTakeDamage,
    OnTakeDamage,
    OnParrySuccess,
    OnCritical, //크리티컬 명중 시
    OnRest,
    OnUseSkill,
    OnDodgeSuccess,
    OnAllyDead, // 아군 사망 시
    OnBeforeDead,
}

public enum DamageType
{
    Normal,
    Fixed
}

public class CombatEventContext
{
    public CombatUnit source;
    public CombatUnit target;
    public float value;
    
    public DamageType damageType;
    public bool isReflectDamage;
    public bool isCritical;

    public List<StatusEffect> debuffs;

    public CombatEventContext(CombatUnit source, CombatUnit target, float value, DamageType damageType = DamageType.Normal,
        bool isReflectDamage = false, bool isCritical = false, List<StatusEffect> debuffs = null)
    {
        this.source = source;
        this.target = target;
        this.value = value;
        this.damageType = damageType;
        this.isReflectDamage = isReflectDamage;
        this.isCritical = isCritical;
        this.debuffs = debuffs;
    }
}

[System.Serializable]
public abstract class Ability
{
    protected CombatUnit owner;
    [SerializeField] protected CombatEvent combatEvent;

    public bool IsFinished { get; protected set; } 

    public virtual void Init(CombatUnit owner)
    {
        this.owner = owner;
        OnAdded();
    }

    public virtual Ability Clone()
    {
        return (Ability)this.MemberwiseClone();
    }

    protected virtual void OnAdded() { }
    public virtual void OnRemoved() { }
    public virtual void OnUpdate(float delta) { }
    public virtual void OnEvent(CombatEvent eventType, CombatEventContext context) { }

    public void MakeFinish() => IsFinished = true;
}
