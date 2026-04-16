using UnityEngine;

[System.Serializable]
public class DurahanPhasePassive : PassiveAbility
{
    [Header("Phase Settings")]
    [Tooltip("패턴이 발동할 체력 비율 (0.5 = 50%)")]
    public float hpThreshold = 0.5f;

    [Tooltip("소환할 머리 몬스터 데이터 (UnitData)")]
    public UnitData headUnitData;

    [Tooltip("본체에 걸어줄 99% 데미지 감소 버프")]
    [SerializeReference, SerializeReferenceDropdown]
    public StatusEffect invincibleBuff;

    private bool isTriggered = false;

    public DurahanPhasePassive()
    {
        combatEvent = CombatEvent.OnTakeDamage;
    }

    public override void OnEvent(CombatEvent eventType, CombatEventContext context)
    {
        if (isTriggered || owner == null || owner.IsDead) return;
        if (eventType == combatEvent && context.target == owner)
        {
            var stats = owner.GetStat<UnitStats>();
            float hpRatio = stats.hp / stats.maxHp.GetValue();

            // 체력이 50% 이하로 떨어지면 발동!
            if (hpRatio <= hpThreshold)
            {
                isTriggered = true;
                ExecutePhaseTransition();
            }
        }
    }

    private void ExecutePhaseTransition()
    {
        EnemyUnit enemyOwner = owner as EnemyUnit;
        if (enemyOwner == null) return;

        if (headUnitData != null)
        {
            GameManager.Battle.SpawnEnemyMidBattle(headUnitData, enemyOwner);
            GameManager.Sound.PlaySFX(SFX.Summon);
        }

        if (invincibleBuff != null)
        {
            enemyOwner.AddAbility(invincibleBuff.Clone());
        }

        // TODO : 머리 스폰 이펙트
    }
}