using UnityEngine;

[System.Serializable]
public class VampireEffect : StatusEffect
{
    public VampireEffect()
    {
        effectType = EffectType.Vampire;
        combatEvent = CombatEvent.OnAttack;
        effectValue = 0.05f; // 기본값 5%
    }

    public VampireEffect(float duration, int stack, bool isPermanent, float effectValue) : this()
    {
        this.duration = duration;
        this.stacks = stack;
        this.isPermanent = isPermanent;
        this.effectValue = effectValue;
    }

    public override void OnEvent(CombatEvent eventType, CombatEventContext ctx)
    {
        if (eventType == combatEvent && ctx.source == owner)
        {
            if (ctx.value > 0)
            {
                float healAmount = ctx.value * (effectValue * stacks);
                owner.Heal(healAmount);
            }
        }
    }
}