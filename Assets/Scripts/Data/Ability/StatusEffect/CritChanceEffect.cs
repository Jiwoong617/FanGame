using UnityEngine;

[System.Serializable]
public class CritChanceEffect : StatusEffect
{
    private StatModifier currentMod;

    public CritChanceEffect()
    {
        effectType = EffectType.CritUp;
        effectValue = 20f;
    }

    public CritChanceEffect(float duration, int stack, bool isPermanent, float effectValue)
    {
        this.duration = duration;
        this.stacks = stack;
        this.isPermanent = isPermanent;
        this.effectValue = effectValue;
        effectType = EffectType.CritUp;
    }

    protected override void OnAdded()
    {
        ApplyModifier();
        GameManager.VFX.PlayEffect(owner.transform.position, owner.transform.position, AttackVFXType.Buff, 0, Color.white);
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
            stats.criticalChance.AddModifier(currentMod);
        }
    }

    private void RemoveModifier()
    {
        if (currentMod != null)
        {
            var stats = owner.GetStat<UnitStats>();
            if (stats != null)
                stats.criticalChance.RemoveModifier(currentMod);

            currentMod = null;
        }
    }
}
