using UnityEngine;

[System.Serializable]
public class BuffPattern : EnemyPattern
{
    [Header("Buff Settings")]
    [Tooltip("true면 본인만, false면 적 전체")]
    public bool isSelf = true;

    [SerializeReference, SerializeReferenceDropdown]
    public StatusEffect buffEffect;

    public override bool OnUpdate(EnemyUnit unit, float delta)
    {
        if (unit.IsAttacking) return false;

        if(isSelf)
        {
            if (buffEffect != null)
                unit.AddAbility(buffEffect.Clone());

            //TODO : 버프 이펙트
        }
        else
        {
            var allies = GameManager.Battle.GetAliveEnemies();
            foreach (var ally in allies)
            {
                if (buffEffect != null)
                    ally.AddAbility(buffEffect.Clone());

                //TODO : 버프 이펙트
            }
        }

        return true;
    }
}


[System.Serializable]
public class HealPattern : EnemyPattern
{
    [Header("Heal Settings")]
    [Tooltip("true면 본인만, false면 적 전체")]
    public bool isSelf = true;
    [Tooltip("체크하면 최대 체력 비례 퍼센트 회복 (30 = 30%)\n체크 해제하면 고정 수치 회복")]
    public bool isPercent = false;
    [Tooltip("회복량 (고정 수치 or 퍼센트)")]
    public float healAmount = 10f;

    public override bool OnUpdate(EnemyUnit unit, float delta)
    {
        if (unit.IsAttacking) return false;

        if(isSelf)
        {
            ApplyHeal(unit);
        }
        else
        {
            var friends = GameManager.Battle.GetAliveEnemies();
            foreach (var friend in friends)
                ApplyHeal(friend);
        }

        return true;
    }

    private void ApplyHeal(CombatUnit target)
    {
        float finalAmount = healAmount;
        if (isPercent)
        {
            var stats = target.GetStat<UnitStats>();
            if (stats != null)
                finalAmount = stats.maxHp.GetValue() * (healAmount/100);
        }

        target.Heal(finalAmount);
    }
}