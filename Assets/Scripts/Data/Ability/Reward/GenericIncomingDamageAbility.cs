using UnityEngine;

[System.Serializable]
public class GenericIncomingDamageAbility : RewardAbility
{
    [Header("OnBeforeTakeDamage로 설정할 것")]
    [Tooltip("받는 데미지 배율 (2.0 = 2배 아픔)")]
    [SerializeField] private float damageMultiplier = 1.0f;

    public override void OnEvent(CombatEvent eventType, CombatEventContext context)
    {
        if (eventType != combatEvent) return;

        if (context != null)
        {
            float original = context.value;
            context.value *= damageMultiplier;
        }
    }
}