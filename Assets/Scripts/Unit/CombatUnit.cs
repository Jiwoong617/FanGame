using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public abstract class CombatUnit : MonoBehaviour
{
    [SerializeField] protected CombatUnitUI combatUI;


    protected const float ATTACK_THRESHOLD = 1f;

    public Action<CombatUnit> OnUnitDead;
    public event Action<CombatEventContext> OnDamageTextRequested;
    public event Action<float> OnHealTextRequested;

    //이건 ui 띄울것들임
    public event Action<float> OnActionBarUpdated;
    public event Action<StatusEffect> OnBuffAdded;
    public event Action<StatusEffect> OnBuffRemoved;
    public event Action<StatusEffect> OnBuffUpdated;

    protected UnitStats stats;
    protected CombatUnit target;
    protected float attackTimer = 0f;
    
    // 런타임 능력 리스트
    protected List<Ability> abilities = new List<Ability>();

    public bool IsDead => stats.hp <= 0;


    protected HitFlash hitEffect;


    protected virtual void Start()
    {
        hitEffect = GetComponent<HitFlash>();
    }

    protected void ProcessAttackLoop(float delta)
    {
        if (IsDead || target == null || target.IsDead) return;

        attackTimer += (delta * stats.attackSpeed.GetValue());
        OnActionBarUpdated?.Invoke(Mathf.Clamp01(attackTimer / ATTACK_THRESHOLD));

        if (attackTimer >= ATTACK_THRESHOLD)
        {
            Attack();
            attackTimer = 0f;
            OnActionBarUpdated?.Invoke(0f);
        }
    }

    public virtual void SetTarget(CombatUnit inTarget)
    {
        target = inTarget;
    }

    public CombatUnit GetTarget()
    {
        return target;
    }

    public virtual void OnBattleStart()
    {
        TriggerAbility(CombatEvent.OnBattleStart, new CombatEventContext(this, target, 0));
    }

    public virtual void OnBattleEnd()
    {
        TriggerAbility(CombatEvent.OnBattleEnd, new CombatEventContext(this, target, 0));

        for (int i = abilities.Count - 1; i >= 0; i--)
        {
            if (abilities[i] is StatusEffect status)
            {
                if (!status.isPermanent || status.IsFinished)
                {
                    status.OnRemoved();
                    OnBuffRemoved?.Invoke(status);
                    abilities.RemoveAt(i);
                }
            }
        }
    }

    public virtual void OnUpdate(float delta)
    {
        if (IsDead) return;

        for (int i = abilities.Count - 1; i >= 0; i--)
        {
            var ability = abilities[i];
            ability.OnUpdate(delta);
            
            if (ability.IsFinished)
            {
                ability.OnRemoved();
                if (ability is StatusEffect status)
                    OnBuffRemoved?.Invoke(status);

                abilities.RemoveAt(i);
            }
        }
    }

    public abstract void Init(UnitData unitData);
    public abstract void OnDead();
    public abstract float TakeDamage(CombatEventContext info);
    public abstract T GetStat<T>() where T : UnitStats;

    public virtual void Attack()
    {
        if (target == null || IsDead) return;

        float damage = stats.attackDamage.GetValue();
        bool isCrit = false;

        if (UnityEngine.Random.Range(0f, 100f) < stats.criticalChance.GetValue())
        {
            isCrit = true;
            damage *= (stats.criticalDamage.GetValue() / 100f);
        }

        CombatEventContext attackCtx = new CombatEventContext(this, target, damage, DamageType.Normal, false, isCrit);
        float actualDamage = target.TakeDamage(attackCtx);

        if (actualDamage > 0 && !IsDead)
        {
            //이거 방어력 깎인 최종 데미지로 교체
            attackCtx.value = actualDamage;
            TriggerAbility(CombatEvent.OnAttack, attackCtx);
        }
    }

    protected void InitializeAbilities(UnitData data)
    {
        abilities.Clear();
        if (data.startingAbilities != null)
        {
            foreach (var ability in data.startingAbilities)
            {
                if (ability != null)
                {
                    AddAbility(ability.Clone());
                }
            }
        }
    }

    public virtual void AddAbility(Ability newAbility)
    {
        if (newAbility == null) return;

        if (newAbility is StatusEffect newStatus)
        {
            foreach (var ability in abilities)
            {
                if (ability is StatusEffect existingStatus &&
                    existingStatus.effectType == newStatus.effectType &&
                    existingStatus.isPermanent == newStatus.isPermanent &&
                    Mathf.Approximately(existingStatus.effectValue, newStatus.effectValue))
                {
                    existingStatus.AddStack(newStatus.stacks, newStatus.duration);
                    OnBuffUpdated?.Invoke(existingStatus);
                    return;
                }
            }
        }

        newAbility.Init(this);
        abilities.Add(newAbility);

        if (newAbility is StatusEffect addedStatus)
        {
            OnBuffAdded?.Invoke(addedStatus);
        }
    }

    public void TriggerAbility(CombatEvent type, CombatEventContext cec)
    {
        for (int i = 0; i < abilities.Count; i++)
            abilities[i].OnEvent(type, cec);
    }

    public void Heal(float amount)
    {
        if (IsDead || amount <= 0) return;

        stats.hp += amount;

        OnHealTextRequested?.Invoke(amount);
    }

    protected void RequestDamageText(CombatEventContext ctx)
    {
        OnDamageTextRequested?.Invoke(ctx);
    }

    protected void RequestActionBarUpdate(float value)
    {
        OnActionBarUpdated?.Invoke(value);
    }
}
