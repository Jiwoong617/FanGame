using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SacrificeSummonPattern : EnemyPattern
{
    [Header("Sacrifice Settings")]
    [Tooltip("소환 시 소모할 체력 (자해 데미지)")]
    public float selfDamageAmount = 30f;

    [Header("Summon Settings")]
    [Tooltip("여기 등록된 몹들 중 하나가 랜덤으로 소환됩니다.")]
    public List<UnitData> randomSummonPool;

    public float castDuration = 1.0f;

    public override bool OnUpdate(EnemyUnit unit, float delta)
    {
        if (unit.IsAttacking) return false;

        unit.PlayActionAnimation(actionSprite, castDuration, () =>
        {
            CombatEventContext selfDmgCtx = new CombatEventContext(
                unit, unit, selfDamageAmount, DamageType.Fixed, true, false, null
            );
            unit.TakeDamage(selfDmgCtx);

            if (randomSummonPool != null && randomSummonPool.Count > 0)
            {
                int randomIndex = Random.Range(0, randomSummonPool.Count);
                UnitData randomMob = randomSummonPool[randomIndex];

                GameManager.Battle.SpawnEnemyMidBattle(randomMob, unit);
            }
        });

        return true;
    }
}