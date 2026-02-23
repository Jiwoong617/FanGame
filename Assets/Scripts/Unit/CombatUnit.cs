using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public abstract class CombatUnit : MonoBehaviour
{
    protected const float ATTACK_THRESHOLD = 1f;

    public Action<CombatUnit> OnUnitDead;

    protected UnitStats stats;
    protected CombatUnit target;
    protected float attackTimer = 0f;
    
    // 런타임 능력 리스트
    protected List<Ability> abilities = new List<Ability>();

    public bool IsDead => stats.hp <= 0;

    protected void ProcessAttackLoop(float delta)
    {
        if (IsDead || target == null || target.IsDead) return;

        attackTimer += (delta * stats.attackSpeed.GetValue());
        if (attackTimer >= ATTACK_THRESHOLD)
        {
            Attack();
            attackTimer = 0f;
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
                abilities.RemoveAt(i);
            }
        }
    }

    public abstract void Init(UnitData unitData);
    public abstract void OnDead();
    public abstract float TakeDamage(CombatUnit attacker, float damage);
    public abstract T GetStat<T>() where T : UnitStats;

    public virtual void Attack()
    {
        if (target == null) return;

        float damage = stats.attackDamage.GetValue();
        float isTargetHit = target.TakeDamage(this, damage);
        if(isTargetHit > 0)
        {
            TriggerAbility(CombatEvent.OnAttack, new CombatEventContext(this, target, damage));
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

        newAbility.Init(this);
        abilities.Add(newAbility);
    }

    public void TriggerAbility(CombatEvent type, CombatEventContext cec)
    {
        for (int i = 0; i < abilities.Count; i++)
            abilities[i].OnEvent(type, cec);
    }
}
