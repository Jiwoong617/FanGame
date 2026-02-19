using UnityEngine;

[System.Serializable]
public class SlowEffect : StatusEffect
{
    public float slowValue = 0.8f; // 1스택당 곱해질 값 (20% 감소)
    private StatModifier currentMod;

    public SlowEffect()
    {
        effectType = EffectType.Slow;
    }

    public SlowEffect(float duration, int stack, bool isPermanent)
    {
        effectType = EffectType.Slow;
        this.duration = duration;
        stacks = stack;
        this.isPermanent = isPermanent;
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
        float finalMult = Mathf.Pow(slowValue, stacks);
        currentMod = new StatModifier(finalMult, StatModType.PercentMult);
        owner.GetStat<UnitStats>().attackSpeed.AddModifier(currentMod);
    }
}


[System.Serializable]
public class InspireEffect : StatusEffect
{
    public float inspireValue = 1.2f; // 1스택당 20% 증가
    private StatModifier currentMod;

    public InspireEffect()
    {
        effectType = EffectType.Inspire;
    }

    public InspireEffect(float duration, int stack, bool isPermanent)
    {
        effectType = EffectType.Inspire;
        this.duration = duration;
        stacks = stack;
        this.isPermanent = isPermanent;
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
        float finalMult = Mathf.Pow(inspireValue, stacks);
        currentMod = new StatModifier(finalMult, StatModType.PercentMult);
        owner.GetStat<UnitStats>().attackSpeed.AddModifier(currentMod);
    }
}