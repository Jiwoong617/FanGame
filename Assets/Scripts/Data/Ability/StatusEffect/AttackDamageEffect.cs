using UnityEngine;

[System.Serializable]
public class AttackDamageEffect : StatusEffect
{
    public StatModType modType = StatModType.PercentAdd;

    private StatModifier currentMod;

    public AttackDamageEffect()
    {
        effectType = EffectType.AttackUp;
        modType = StatModType.PercentAdd;
    }

    public AttackDamageEffect(float duration, int stack, bool isPermanent, float effectValue, StatModType modType, EffectType effectType)
    {
        this.duration = duration;
        this.stacks = stack;
        this.isPermanent = isPermanent;
        this.effectValue = effectValue;
        this.modType = modType;
        this.effectType = effectType;
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

    protected void ApplyModifier()
    {
        var stats = owner.GetStat<UnitStats>();
        if (stats != null)
        {
            float finalValue = 0f;
            if (modType == StatModType.Flat)
                finalValue = effectValue * stacks;
            else
                finalValue = Mathf.Pow(effectValue, stacks);

            currentMod = new StatModifier(finalValue, modType);
            stats.attackDamage.AddModifier(currentMod);
        }
    }

    protected void RemoveModifier()
    {
        if (currentMod != null)
        {
            var stats = owner.GetStat<UnitStats>();
            if (stats != null)
            {
                stats.attackDamage.RemoveModifier(currentMod);
                currentMod = null;
            }
        }
    }
}