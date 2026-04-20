using System.Collections.Generic;
using UnityEngine;

public enum AttackEvadeType
{
    Both, //둘다가능
    DodgeOnly, //회피만 가능
    ParryOnly, //패리만 가능
    None //그냥 불가
}

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
    OnBeforeAttack,
    OnHeal,
    OnReflect, // 반사 발동 시 (ctx.source = 반사한 유닛, ctx.target = 반사 대상(적), ctx.value = 반사량)
    OnBeforeHeal, // 회복 적용 전
    OnStaminaUsed, // 스태미나 소모 시 (ctx.source = 플레이어, ctx.value = 소모량)
    
    //이거 무조건 아래에다가 추가해야됨 안그럼 인스펙터에서 조정하는거 다시 해야됨
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

    public AttackEvadeType evadeType;
    public DamageType damageType;
    public bool isReflectDamage;
    public bool isCritical;

    public List<StatusEffect> debuffs;

    public CombatEventContext(CombatUnit source, CombatUnit target, float value, DamageType damageType = DamageType.Normal,
        bool isReflectDamage = false, bool isCritical = false, List<StatusEffect> debuffs = null, AttackEvadeType evadeType = AttackEvadeType.Both)
    {
        this.source = source;
        this.target = target;
        this.value = value;
        this.damageType = damageType;
        this.isReflectDamage = isReflectDamage;
        this.isCritical = isCritical;
        this.debuffs = debuffs;
        this.evadeType = evadeType;
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
