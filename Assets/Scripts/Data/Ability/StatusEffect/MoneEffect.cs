using UnityEngine;

[System.Serializable]
public class MoneEffect : StatusEffect
{
    private StatModifier critChanceMod;
    private StatModifier critDmgMod;

    public MoneEffect()
    {
        effectType = EffectType.Mone;
        combatEvent = CombatEvent.OnAttack;
    }

    public override void Init(CombatUnit owner)
    {
        base.Init(owner);
        duration = 100.0f;
        isPermanent = false;
    }

    protected override void OnAdded()
    {
        critChanceMod = new StatModifier(100f, StatModType.Flat);
        critDmgMod = new StatModifier(50f, StatModType.Flat);

        var stats = owner.GetStat<UnitStats>();
        stats.criticalChance.AddModifier(critChanceMod);
        stats.criticalDamage.AddModifier(critDmgMod);
    }

    public override void OnEvent(CombatEvent eventType, CombatEventContext ctx)
    {
        if (eventType == CombatEvent.OnAttack && ctx.source == owner)
        {
            IsFinished = true;
        }
    }

    public override void OnRemoved()
    {
        // 버프가 사라질 때 스탯 보너스 회수
        var stats = owner.GetStat<UnitStats>();
        if (stats != null)
        {
            if (critChanceMod != null)
                stats.criticalChance.RemoveModifier(critChanceMod);
            if (critDmgMod != null)
                stats.criticalDamage.RemoveModifier(critDmgMod);
        }
    }
}