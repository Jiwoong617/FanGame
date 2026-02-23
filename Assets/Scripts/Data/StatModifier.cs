public enum StatModType
{
    Flat,           // 깡스탯 (공격력 +10)
    PercentAdd,     // 합연산 퍼센트 (공격력 +10%)
    PercentMult,    // 곱연산 퍼센트 (최종 데미지 2배)
}

public class StatModifier
{
    public float Value;
    public StatModType Type;

    public StatModifier(float value, StatModType type)
    {
        Value = value;
        Type = type;
    }
}
