using UnityEngine;

[System.Serializable]
public class CritDamageEffect : StatusEffect
{
    private StatModifier currentMod;

    public CritDamageEffect()
    {
        effectType = EffectType.CritDamageUp;
        effectValue = 10f;
    }

    public CritDamageEffect(float duration, int stack, bool isPermanent, float effectValue) : this()
    {
        this.duration = duration;
        this.stacks = stack;
        this.isPermanent = isPermanent;
        this.effectValue = effectValue;
    }

    protected override void OnAdded()
    {
        ApplyModifier();
    }

    protected override void OnStackUpdated()
    {
        if (currentMod != null)
            RemoveModifier();

        ApplyModifier();
    }

    public override void OnRemoved()
    {
        RemoveModifier();
    }

    private void ApplyModifier()
    {
        var stats = owner.GetStat<UnitStats>();
        if (stats != null)
        {
            float finalValue = effectValue * stacks;
            currentMod = new StatModifier(finalValue, StatModType.Flat);
            stats.criticalDamage.AddModifier(currentMod);
        }
    }

    private void RemoveModifier()
    {
        if (currentMod != null)
        {
            var stats = owner.GetStat<UnitStats>();
            if (stats != null)
                stats.criticalDamage.RemoveModifier(currentMod);

            currentMod = null;
        }
    }
}
