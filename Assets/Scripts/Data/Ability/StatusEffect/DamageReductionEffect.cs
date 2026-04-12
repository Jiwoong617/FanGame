using UnityEngine;

[System.Serializable]
public class DamageReductionEffect : StatusEffect
{
    public DamageReductionEffect()
    {
        effectType = EffectType.DamageReduction; 
        combatEvent = CombatEvent.OnBeforeTakeDamage;
    }

    public DamageReductionEffect(int duration, bool isPermanent, float effectValue, int stack) : this()
    {
        this.effectValue = effectValue;
        this.duration = duration;
        this.isPermanent = isPermanent;
        this.stacks = stack;
    }

    protected override void OnAdded()
    {
        GameManager.VFX.PlayEffect(owner.transform.position, owner.transform.position, AttackVFXType.Buff, 0, Color.white);
    }

    public override void OnEvent(CombatEvent eventType, CombatEventContext ctx)
    {
        if (eventType == combatEvent && ctx.target == owner)
        {
            if (ctx.value > 0 && ctx.damageType == DamageType.Normal)
            {
                float multiplier = 1f - (effectValue * stacks);
                multiplier = Mathf.Max(0, multiplier);

                float reducedDamage = ctx.value * multiplier;
                ctx.value = Mathf.Max(1, reducedDamage);
            }
        }
    }
}