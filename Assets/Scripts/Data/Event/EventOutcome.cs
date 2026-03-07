using UnityEngine;
using System;

[Serializable]
public abstract class EventOutcome
{
    public abstract void Apply(PlayerUnit player);
}

[Serializable]
public class StatOutcome : EventOutcome
{
    public StatType targetStat;
    public StatModType modType = StatModType.Flat;
    public float amount;

    public override void Apply(PlayerUnit player)
    {
        var stats = player.GetStat<PlayerStats>();
        if (stats == null) return;

        StatModifier mod = new StatModifier(amount, modType);
        switch (targetStat)
        {
            case StatType.MaxHP: stats.maxHp.AddModifier(mod); break;
            case StatType.AttackDamage: stats.attackDamage.AddModifier(mod); break;
            case StatType.Defense: stats.defense.AddModifier(mod); break;
            case StatType.AttackSpeed: stats.attackSpeed.AddModifier(mod); break;
            case StatType.Stamina: stats.maxStamina.AddModifier(mod); break;
            case StatType.StaminaRegen: stats.staminaRegen.AddModifier(mod); break;
            case StatType.MaxFp: stats.maxFp.AddModifier(mod); break;
            case StatType.CriticalChance: stats.criticalChance.AddModifier(mod); break;
            case StatType.CriticalDamage: stats.criticalDamage.AddModifier(mod); break;
        }
    }
}

[Serializable]
public class RecoveryOutcome : EventOutcome
{
    public bool isHp = true;
    public float amount;
    public bool isPercentage = false;

    public override void Apply(PlayerUnit player)
    {
        var stats = player.GetStat<PlayerStats>();
        if (stats == null) return;

        float finalAmount = amount;
        float maxVal = 0f;

        if(isHp)
        {
            maxVal = stats.maxHp.GetValue();
            if (isPercentage)
                finalAmount = maxVal * (amount / 100f);
            player.Heal(finalAmount);
        }
        else
        {
            maxVal = stats.maxFp.GetValue();
            if (isPercentage)
                finalAmount = maxVal * (amount / 100f);
            stats.fp += finalAmount;
        }
    }
}

[Serializable]
public class ItemOutcome : EventOutcome
{
    public RewardData itemReward;

    public override void Apply(PlayerUnit player)
    {
        if (itemReward == null) return;

        itemReward.Apply(player);

        if (itemReward.isItem)
        {
            GameManager.Inventory.AddItem(itemReward);
        }
    }
}

[Serializable]
public class HpLoseOutCome : EventOutcome
{
    public float amount;
    public bool isPercentage = false;

    public override void Apply(PlayerUnit player)
    {
        var stats = player.GetStat<PlayerStats>();
        if (stats == null) return;

        float lossValue = amount;
        float currentHp = stats.hp;
        if (isPercentage)
            lossValue = currentHp * (amount / 100f);

        float nextHp = currentHp - lossValue;
        if (nextHp < 1f)
            nextHp = 1f;

        stats.hp = nextHp;
    }
}