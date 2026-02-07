using UnityEngine;
using System;

public abstract class CombatUnit : MonoBehaviour
{
    public Action<CombatUnit> OnUnitDead;


    protected UnitStats stats;
    protected CombatUnit target;

    protected float attackTimer = 0f;
    
    public bool IsDead => stats.hp <= 0;

    public virtual void OnUpdate(float delta)
    {
        ProcessAttackLoop(delta);
    }

    protected void ProcessAttackLoop(float delta)
    {
        if (IsDead || target == null || target.IsDead) return;

        attackTimer += delta;
        if (attackTimer >= 1f / stats.attackSpeed)
        {
            Attack();
            attackTimer = 0f;
        }
    }
    public void SetTarget(CombatUnit inTarget)
    {
        target = inTarget;
        attackTimer = 0f;
    }

    public abstract void Init(UnitData unitData);
    public abstract void OnDead();
    public abstract void Attack();
    public abstract void TakeDamage(float damage);
    public abstract T GetStat<T>() where T : UnitStats;
}
