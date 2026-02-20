using UnityEngine;

[System.Serializable]
public class SlowEffect : StatusEffect
{
    private StatModifier currentMod;

    public SlowEffect()
    {
        effectType = EffectType.Slow;
        effectValue = 0.8f; // 기본 20% 감소
    }

    public SlowEffect(float duration, int stack, bool isPermanent, float effectValue) : this()
    {
        this.duration = duration;
        this.stacks = stack;
        this.isPermanent = isPermanent;
        this.effectValue = effectValue;
    }

    protected override void OnAdded() { ApplyModifier(); }

    protected override void OnStackUpdated()
    {
        // 스택이 쌓일 때 기존 모디파이어를 지우고 새로 계산된 1개만 넣음
        if (currentMod != null)
            owner.GetStat<UnitStats>().attackSpeed.RemoveModifier(currentMod);
        ApplyModifier();
    }

    public override void OnRemoved()
    {
        if (currentMod != null)
            owner.GetStat<UnitStats>().attackSpeed.RemoveModifier(currentMod);
    }

    private void ApplyModifier()
    {
        float finalMult = Mathf.Pow(effectValue, stacks);
        currentMod = new StatModifier(finalMult, StatModType.PercentMult);
        owner.GetStat<UnitStats>().attackSpeed.AddModifier(currentMod);
    }
}


[System.Serializable]
public class InspireEffect : StatusEffect
{
    private StatModifier currentMod;

    public InspireEffect()
    {
        effectType = EffectType.Inspire;
        effectValue = 1.2f; // 기본 20% 증가
    }

    public InspireEffect(float duration, int stack, bool isPermanent, float effectValue) : this()
    {
        this.duration = duration;
        this.stacks = stack;
        this.isPermanent = isPermanent;
        this.effectValue = effectValue;
    }

    protected override void OnAdded() { ApplyModifier(); }
    protected override void OnStackUpdated()
    {
        if (currentMod != null)
            owner.GetStat<UnitStats>().attackSpeed.RemoveModifier(currentMod);
        ApplyModifier();
    }
    public override void OnRemoved()
    {
        if (currentMod != null)
            owner.GetStat<UnitStats>().attackSpeed.RemoveModifier(currentMod);
    }
    private void ApplyModifier()
    {
        float finalMult = Mathf.Pow(effectValue, stacks);
        currentMod = new StatModifier(finalMult, StatModType.PercentMult);
        owner.GetStat<UnitStats>().attackSpeed.AddModifier(currentMod);
    }
}