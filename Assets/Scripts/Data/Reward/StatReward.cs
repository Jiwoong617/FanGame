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

    }
}
