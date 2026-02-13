using UnityEngine;

public enum CombatEvent
{
    OnBattleStart,
    OnBattleEnd,
    OnAttack,
    OnTakeDamage,
    OnParrySuccess,
}

public class CombatEventContext
{
    public CombatUnit source;
    public CombatUnit target;
    public float value;

    public CombatEventContext(CombatUnit source, CombatUnit target, float value)
    {
        this.source = source;
        this.target = target;
        this.value = value;
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

    public virtual void OnAdded() { }
    public virtual void OnRemoved() { }
    public virtual void OnUpdate(float delta) { }
    public virtual void OnEvent(CombatEvent eventType, CombatEventContext context) { }
}
