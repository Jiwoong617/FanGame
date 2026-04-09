using UnityEngine;

[System.Serializable]
public class AttackBuffPattern : EnemyPattern
{
    [Header("Attack Settings")]
    [Tooltip("공격력의 몇 %로 때릴지 (1.0 = 100%)")]
    public float damagePercent = 1.0f;

    [Tooltip("일반 데미지/고정 데미지")]
    public DamageType damageType = DamageType.Normal;

    [Tooltip("직접 가서 때릴지(True), 제자리 공격할지(False)")]
    public bool useMoveAnim = true;

    [Header("Buff Settings")]
    [Tooltip("버프를 적용할 대상")]
    public BuffTargetType targetType = BuffTargetType.Self;

    [Tooltip("적용할 버프/상태이상")]
    [SerializeReference, SerializeReferenceDropdown]
    public StatusEffect buffEffect;

    public override bool OnUpdate(EnemyUnit unit, float delta)
    {
        var target = unit.GetTarget();
        if (target == null || target.IsDead) return true;
        if (unit.IsAttacking) return false;

        var stats = unit.GetStat<UnitStats>();
        float damage = stats.attackDamage.GetValue() * damagePercent;

        unit.Attack(target, damage, true, true, null, useMoveAnim, damageType, (actualDamage) =>
        {
            ApplyBuff(unit);
        });

        return true;
    }

    private void ApplyBuff(EnemyUnit unit)
    {
        if (buffEffect == null) return;

        var allies = GameManager.Battle.GetAliveEnemies();
        switch (targetType)
        {
            case BuffTargetType.Self:
                unit.AddAbility(buffEffect.Clone());
                break;
            case BuffTargetType.Other:
                var others = allies.FindAll(a => a != unit);
                if (others.Count > 0)
                {
                    var t = others[Random.Range(0, others.Count)];
                    t.AddAbility(buffEffect.Clone());
                }
                break;
            case BuffTargetType.OtherEnemies:
                foreach (var ally in allies)
                {
                    if (ally == unit) continue;
                    ally.AddAbility(buffEffect.Clone());
                }
                break;
            case BuffTargetType.AllEnemies:
                foreach (var ally in allies)
                {
                    ally.AddAbility(buffEffect.Clone());
                }
                break;
            case BuffTargetType.AllCharacter:
                foreach (var ally in allies)
                {
                    ally.AddAbility(buffEffect.Clone());
                }
                if (GameManager.Instance.Player != null && !GameManager.Instance.Player.IsDead)
                {
                    GameManager.Instance.Player.AddAbility(buffEffect.Clone());
                }
                break;
        }
    }
}