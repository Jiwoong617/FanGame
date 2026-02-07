using UnityEngine;

public enum StatType
{
    MaxHP,
    AttackDamage,
    Defense,
    AttackSpeed
}

[CreateAssetMenu(fileName = "NewStatReward", menuName = "Reward/Stat Reward")]
public class StatReward : RewardBase
{
    public StatType targetStat;
    public float amount;

    public override void Apply(PlayerUnit player)
    {
        if (player == null) return;

        UnitStats stats = player.GetStat<UnitStats>();
        if (stats == null) return;

        switch (targetStat)
        {
            case StatType.MaxHP:
                stats.maxHp += amount;
                stats.hp += amount;
                break;
            case StatType.AttackDamage:
                stats.attackDamage += amount;
                break;
            case StatType.Defense:
                stats.defense += amount;
                break;
            case StatType.AttackSpeed:
                stats.attackSpeed += amount;
                break;
        }
        
        Debug.Log($"[StatReward] Applied {targetStat} {((amount >= 0) ? "+" : "")}{amount}");
    }
}
