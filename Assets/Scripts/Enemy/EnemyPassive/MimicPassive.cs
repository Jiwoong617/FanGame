using UnityEngine;

[System.Serializable]
public class MimicPassive : PassiveAbility
{
    [Header("Damage Reduction Settings")]
    [Tooltip("받는 데미지 감소 비율 (0~1, 기본 0.5 = 50%)")]
    public float reductionRate = 0.5f;

    public MimicPassive()
    {
        combatEvent = CombatEvent.OnBeforeTakeDamage;
    }

    public override void OnEvent(CombatEvent eventType, CombatEventContext context)
    {
        if (eventType == combatEvent && context.target == owner && owner != null && !owner.IsDead)
        {
            if (context.value > 0 && context.damageType == DamageType.Normal)
            {
                float multiplier = Mathf.Clamp01(1f - reductionRate);
                context.value = Mathf.Max(1, context.value * multiplier);
            }
        }
    }
}
