using UnityEngine;

public enum CombatEvent
{
    OnBattleStart,
    OnBattleEnd,
    OnAttack,
    OnBeforeTakeDamage,
    OnTakeDamage,
    OnParrySuccess,
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

    public CombatEventContext(CombatUnit source, CombatUnit target, float value, DamageType damageType = DamageType.Normal, bool isReflectDamage = false, bool isCritical = false)
    {
        this.source = source;
        this.target = target;
        this.value = value;
        this.damageType = damageType;
        this.isReflectDamage = isReflectDamage;
        this.isCritical = isCritical;
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
}
