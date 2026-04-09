using System.Collections.Generic;

public static class BuffSlotTooltipDB
{
    private static readonly Dictionary<EffectType, (string name, string desc)> statusTooltips = new Dictionary<EffectType, (string, string)>()
    {
        { EffectType.IronFortress, ("무효화", "받는 피해를 1회 무시합니다.") },
        { EffectType.Vampire, ("흡혈", "공격 시 피해량의 일부를 회복합니다.") },
        { EffectType.Reflect, ("반사", "받은 피해의 일부를 되돌려줍니다.") },
        { EffectType.Disarm, ("무장해제", "잠시 공격할 수 없습니다.") },
        { EffectType.Slow, ("공격속도 감소", "대상의 공격속도가 감소합니다.") },
        { EffectType.Inspire, ("공격속도 증가", "대상의 공격속도가 증가합니다.") },
        { EffectType.Mone, ("기습", "다음 공격에 치명타가 발생합니다.") },
        { EffectType.AttackDown, ("공격력 감소", "대상의 공격력이 감소합니다.") },
        { EffectType.AttackUp, ("공격력 증가", "대상의 공격력이 증가합니다.") },
        { EffectType.DamageReduction, ("피해 감소", "받는 피해량이 감소합니다.") },
        { EffectType.DamageAmplification, ("피해 증가", "받는 피해량이 증가합니다.") },
        { EffectType.Taunt, ("도발", "대상이 도발 당해 타겟 변경이 불가능합니다.") },
        { EffectType.Shackle, ("구속", "패링과 회피가 불가능합니다.") },
    };


    public static (string name, string desc) GetStatusTooltip(EffectType type)
    {
        if (statusTooltips.TryGetValue(type, out var tooltip))
            return tooltip;
        return ("알 수 없는 버프", "효과를 알 수 없습니다.");
    }
}