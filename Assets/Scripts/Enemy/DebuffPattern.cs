using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DebuffPattern : EnemyPattern
{
    [Header("Debuff Only Settings")]
    [Tooltip("적용할 디버프 목록")]
    [SerializeReference, SerializeReferenceDropdown]
    public List<StatusEffect> debuffs = new List<StatusEffect>();

    [Tooltip("제자리 시전(False)이 기본이지만, 돌진(True)도 가능")]
    public bool useMoveAnim = false;

    public override bool OnUpdate(EnemyUnit unit, float delta)
    {
        var target = unit.GetTarget();
        if (target == null || target.IsDead) return true;
        if (unit.IsAttacking) return false;

        unit.Attack(target, 0f, true, true, debuffs, useMoveAnim);

        return true;
    }
}


[System.Serializable]
public class AttackDebuffPattern : EnemyPattern
{
    [Header("Attack & Debuff Settings")]
    public float damagePercent = 1.0f;

    [Tooltip("일반 데미지/고정 데미지")]
    public DamageType damageType = DamageType.Normal;

    [SerializeReference, SerializeReferenceDropdown]
    public List<StatusEffect> debuffs = new List<StatusEffect>();

    public bool useMoveAnim = true;

    public override bool OnUpdate(EnemyUnit unit, float delta)
    {
        var target = unit.GetTarget();
        if (target == null || target.IsDead) return true;
        if (unit.IsAttacking) return false;

        var stats = unit.GetStat<UnitStats>();
        float damage = stats.attackDamage.GetValue() * damagePercent;

        unit.Attack(target, damage, true, true, debuffs, useMoveAnim, damageType);

        return true;
    }
}