using UnityEngine;

[System.Serializable]
public class RetaliationPassive : PassiveAbility
{
    [Header("Retaliation Settings")]
    [Tooltip("몇 회 피격 시 반격할 것인가?")]
    public int requiredHitCount = 5;

    [Tooltip("반격 시 플레이어에게 줄 데미지 (고정 수치)")]
    public float retaliationDamage = 10f;

    private int currentHitCount = 0;

    public RetaliationPassive()
    {
        combatEvent = CombatEvent.OnTakeDamage;
    }

    public override void OnEvent(CombatEvent eventType, CombatEventContext context)
    {
        if (eventType == combatEvent && owner != null && context.target == owner && !owner.IsDead)
        {
            if (context.isReflectDamage) return;

            currentHitCount++;

            if (currentHitCount >= requiredHitCount)
            {
                ExecuteRetaliation(context.source);
                currentHitCount = 0;
            }
        }
    }

    private void ExecuteRetaliation(CombatUnit attacker)
    {
        if (attacker == null || attacker.IsDead) return;

        CombatEventContext retCtx = new CombatEventContext(
            owner,
            attacker,
            retaliationDamage,
            DamageType.Fixed,
            true,
            false,
            null
        );

        attacker.TakeDamage(retCtx);
    }
}