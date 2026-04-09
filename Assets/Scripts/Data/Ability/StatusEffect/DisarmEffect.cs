using UnityEngine;

[System.Serializable]
public class DisarmEffect : StatusEffect
{
    private StatModifier mod;

    public DisarmEffect()
    {
        effectType = EffectType.Disarm;
    }

    public DisarmEffect(float duration, int stack, bool isPermanent)
    {
        effectType = EffectType.Disarm;
        this.duration = duration;
        this.stacks = stack;
        this.isPermanent = isPermanent;
    }

    protected override void OnAdded()
    {
        // 곱연산으로 해서 다른 버프가 있어도 무조건 0으로 만듦
        mod = new StatModifier(0f, StatModType.PercentMult);
        owner.GetStat<UnitStats>().attackSpeed.AddModifier(mod);

        GameManager.VFX.PlayEffect(owner.transform.position, owner.transform.position, AttackVFXType.Disarm, 0, Color.white);
    }

    public override void OnRemoved()
    {
        owner.GetStat<UnitStats>().attackSpeed.RemoveModifier(mod);
    }
}