using UnityEngine;

[System.Serializable]
public class GenericStatAbility : BaseTriggerAbility
{
    [Header("Stat Settings")]
    [SerializeField] private StatType statTarget = StatType.AttackDamage;
    [SerializeField] private float modifierValue = 1f;

    protected override void ExecuteEffect(CombatEventContext context)
    {
        UnitStats unitStats = owner.GetStat<UnitStats>();
        if (unitStats == null) return;

        Stat stat = ResolveStat(unitStats);
        if (stat == null) return;

        stat.SetBaseValue(stat.GetBaseValue() + modifierValue);
    }

    private Stat ResolveStat(UnitStats unitStats)
    {
        switch (statTarget)
        {
            case StatType.MaxHP:          return unitStats.maxHp;
            case StatType.Defense:        return unitStats.defense;
            case StatType.AttackDamage:   return unitStats.attackDamage;
            case StatType.AttackSpeed:    return unitStats.attackSpeed;
            case StatType.CriticalChance: return unitStats.criticalChance;
            case StatType.CriticalDamage: return unitStats.criticalDamage;
        }

        if (unitStats is PlayerStats playerStats)
        {
            switch (statTarget)
            {
                case StatType.Stamina:       return playerStats.maxStamina;
                case StatType.StaminaRegen:  return playerStats.staminaRegen;
                case StatType.MaxFp:         return playerStats.maxFp;
                case StatType.SkillCoolTime: return playerStats.skillCoolTime;
                case StatType.DodgeCost:     return playerStats.dodgeCost;
                case StatType.ParryCost:     return playerStats.parryCost;
            }
        }

        return null;
    }
}
