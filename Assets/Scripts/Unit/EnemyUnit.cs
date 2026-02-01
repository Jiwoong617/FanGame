using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class EnemyUnit : CombatUnit
{
    public void Init(UnitData unitData)
    {
        stats = new UnitStats(unitData);
    }

    public override void OnUpdate(float delta)
    {
        ProcessAttackLoop(delta);
    }

    public override void OnDead()
    {
        Debug.Log($"{name} Dead");
    }

    public override void Attack()
    {
        if (target != null)
        {
            Debug.Log($"[Enemy] {name} attacks {target.name} for {stats.attackDamage} damage!");
            target.TakeDamage(stats.attackDamage);
        }
    }


    public override void TakeDamage(float damage)
    {
        float finalDamage = Mathf.Max(1, damage - stats.defense);
        stats.hp -= finalDamage;

        if (stats.hp <= 0)
        {
            stats.hp = 0;
            OnUnitDead?.Invoke(this);
            OnDead();
        }
    }
}
