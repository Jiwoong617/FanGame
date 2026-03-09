using UnityEngine;

[System.Serializable]
public class DamageReductionEffect : StatusEffect
{
    [Header("Reduction Settings")]
    [Tooltip("피해 감소율 (0.1 = 10%, 0.5 = 50%)")]
    public float reductionPercent = 0.0f;

    public DamageReductionEffect()
    {
        effectType = EffectType.DamageReduction; 
        combatEvent = CombatEvent.OnBeforeTakeDamage;
    }

    public DamageReductionEffect(float percent, int duration, bool isPermanent) : this()
    {
        this.reductionPercent = percent;
        this.duration = duration;
        this.isPermanent = isPermanent;
        this.stacks = 1;
    }

    public override void OnEvent(CombatEvent eventType, CombatEventContext ctx)
    {
        if (eventType == combatEvent && ctx.target == owner)
        {
            if (ctx.value > 0)
            {
                float originalDamage = ctx.value;
                float reducedDamage = originalDamage * (1.0f - reductionPercent);

                ctx.value = Mathf.Max(0, reducedDamage);
            }
        }
    }
}