using UnityEngine;

[System.Serializable]
public class DamageAmplificationEffect : StatusEffect
{
    public DamageAmplificationEffect()
    {
        effectType = EffectType.DamageAmplification;
        combatEvent = CombatEvent.OnBeforeTakeDamage;
    }

    public DamageAmplificationEffect(float duration, bool isPermanent, float effectValue, int stack) : this()
    {
        this.duration = duration;
        this.isPermanent = isPermanent;
        this.effectValue = effectValue;
        this.stacks = stack;
    }

    protected override void OnAdded()
    {
        GameManager.VFX.PlayEffect(owner.transform.position, owner.transform.position, AttackVFXType.Debuff, 0, Color.white);
    }

    public override void OnEvent(CombatEvent eventType, CombatEventContext ctx)
    {
        if (eventType == combatEvent && ctx.target == owner)
        {
            if (ctx.value > 0 && ctx.damageType == DamageType.Normal)
            {
                float multiplier = 1f + (effectValue * stacks);
                ctx.value *= multiplier;
            }
        }
    }
}