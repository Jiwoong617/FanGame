using UnityEngine;

[System.Serializable]
public class VampireEffect : StatusEffect
{
    public float vampRatePerStack = 0.05f; // 5%
    
    public VampireEffect()
    {
        effectType = EffectType.Vampire;
        combatEvent = CombatEvent.OnAttack;
    }

    public VampireEffect(float duration, int stack, bool isPermanent)
    {
        effectType = EffectType.Vampire;
        combatEvent = CombatEvent.OnAttack;
        this.duration = duration;
        stacks = stack;
        this.isPermanent = isPermanent;
    }

    public override void OnEvent(CombatEvent eventType, CombatEventContext ctx)
    {
        //OnAttack
        if (eventType == combatEvent && ctx.source == owner)
        {
            if (ctx.value > 0)
            {
                float healAmount = ctx.value * (vampRatePerStack * stacks);
                var stats = owner.GetStat<UnitStats>();
                stats.hp += healAmount;
                Debug.Log($"[흡혈] {owner.name}이 {healAmount:F1} 회복!");
            }
        }
    }
}
