using UnityEngine;

public enum StatType
{
    MaxHP,
    Defense,
    AttackDamage,
    AttackSpeed,
    Stamina,
    StaminaRegen,
    MaxFp,
    SkillCoolTime,
    DodgeCost,
    ParryCost
}

[CreateAssetMenu(fileName = "NewStatReward", menuName = "Reward/Stat Reward")]
public class StatReward : RewardBase
{
    public StatType targetStat;
    public StatModType modType = StatModType.Flat;
    public float amount;

    public override void Apply(PlayerUnit player)
    {
        if (player == null) return;

        PlayerStats stats = player.GetStat<PlayerStats>();
        if (stats == null) return;

        StatModifier mod = new StatModifier(amount, modType);

        switch (targetStat)
        {
            case StatType.MaxHP:
                float oldMaxHp = stats.maxHp.GetValue();
                stats.maxHp.AddModifier(mod);
                
                // 증가 후 값 확인하여 차이만큼 현재 체력 회복
                float newMaxHp = stats.maxHp.GetValue();
                if (newMaxHp > oldMaxHp)
                    stats.hp += (newMaxHp - oldMaxHp);
                break;

            case StatType.AttackDamage:
                stats.attackDamage.AddModifier(mod);
                break;

            case StatType.Defense:
                stats.defense.AddModifier(mod);
                break;

            case StatType.AttackSpeed:
                stats.attackSpeed.AddModifier(mod);
                break;

            case StatType.Stamina:
                stats.maxStamina.AddModifier(mod);
                break;

            case StatType.StaminaRegen:
                stats.staminaRegen.AddModifier(mod);
                break;

            case StatType.MaxFp:
                float oldFp = stats.maxFp.GetValue();
                stats.maxFp.AddModifier(mod);

                float newFp = stats.maxFp.GetValue();
                if(newFp >  oldFp)
                    stats.fp += (newFp - oldFp);
                break;

            case StatType.SkillCoolTime:
                stats.skillCoolTime.AddModifier(mod);
                break;

            case StatType.DodgeCost:
                stats.dodgeCost.AddModifier(mod);
                break;

            case StatType.ParryCost:
                stats.parryCost.AddModifier(mod);
                break;
        }
        
        Debug.Log($"[StatReward] Applied {targetStat} Value:{amount} Type:{modType}");
    }
}
