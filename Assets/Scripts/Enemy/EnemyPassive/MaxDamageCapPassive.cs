using UnityEngine;

[System.Serializable]
public class MaxDamageCapPassive : PassiveAbility
{
    [Header("Damage Cap Settings")]
    [Tooltip("한 번 피격 시 받을 수 있는 최대 데미지 한도")]
    public float maxDamageCap = 5f;

    public MaxDamageCapPassive()
    {
        combatEvent = CombatEvent.OnBeforeTakeDamage;
    }

    public override void OnEvent(CombatEvent eventType, CombatEventContext context)
    {
        if (eventType == combatEvent && context.target == owner && owner != null && !owner.IsDead)
        {
            if (context.value > maxDamageCap)
            {
                context.value = maxDamageCap;
            }
        }
    }
}