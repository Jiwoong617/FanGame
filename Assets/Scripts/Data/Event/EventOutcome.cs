using UnityEngine;
using System;

[Serializable]
public abstract class EventOutcome
{
    public abstract string Apply(PlayerUnit player);
}

[Serializable]
public class StatOutcome : EventOutcome
{
    public StatType targetStat;
    public StatModType modType = StatModType.Flat;
    public float amount;

    public override string Apply(PlayerUnit player)
    {
        var stats = player.GetStat<PlayerStats>();
        if (stats == null) return "";

        Stat targetStatObj = null;
        string statName = "";
        switch (targetStat)
        {
            case StatType.MaxHP: targetStatObj = stats.maxHp; statName = "최대 체력"; break;
            case StatType.AttackDamage: targetStatObj = stats.attackDamage; statName = "공격력"; break;
            case StatType.Defense: targetStatObj = stats.defense; statName = "방어력"; break;
            case StatType.AttackSpeed: targetStatObj = stats.attackSpeed; statName = "공격 속도"; break;
            case StatType.Stamina: targetStatObj = stats.maxStamina; statName = "최대 스태미나"; break;
            case StatType.StaminaRegen: targetStatObj = stats.staminaRegen; statName = "스태미나 회복"; break;
            case StatType.MaxFp: targetStatObj = stats.maxFp; statName = "최대 FP"; break;
            case StatType.CriticalChance: targetStatObj = stats.criticalChance; statName = "치명타 확률"; break;
            case StatType.CriticalDamage: targetStatObj = stats.criticalDamage; statName = "치명타 피해"; break;
        }

        if (targetStatObj == null) return "";

        float beforeVal = targetStatObj.GetValue();
        targetStatObj.AddModifier(new StatModifier(amount, modType));
        float afterVal = targetStatObj.GetValue();
        float diff = afterVal - beforeVal;

        // 3. 증감에 따라 색상을 다르게 하여 텍스트 반환
        if (diff > 0)
            return $"<color=#009900>({statName} {diff:F1} 증가)</color>";
        else if (diff < 0)
            return $"<color=#FF0000>({statName} {Mathf.Abs(diff):F1} 감소)</color>";
        else
            return "";
    }
}

[Serializable]
public class RecoveryOutcome : EventOutcome
{
    public bool isHp = true;
    public float amount;
    public bool isPercentage = false;

    public override string Apply(PlayerUnit player)
    {
        var stats = player.GetStat<PlayerStats>();
        if (stats == null) return "";

        float finalAmount = amount;
        float maxVal = isHp ? stats.maxHp.GetValue() : stats.maxFp.GetValue();
        if (isPercentage)
            finalAmount = maxVal * (amount / 100f);

        if (isHp)
        {
            player.Heal(finalAmount);
            return $"<color=#009900>(체력 {finalAmount:F0} 회복)</color>";
        }
        else
        {
            stats.fp = Mathf.Min(stats.fp + finalAmount, maxVal);
            return $"<color=#0000FF>(FP {finalAmount:F0} 회복)</color>";
        }
    }
}

[Serializable]
public class ItemOutcome : EventOutcome
{
    public RewardData itemReward;

    public override string Apply(PlayerUnit player)
    {
        if (itemReward == null) return "";

        itemReward.Apply(player);

        if (itemReward.isItem)
        {
            GameManager.Inventory.AddItem(itemReward);
        }

        return $"<color=#009900>({itemReward.RewardName} 획득)</color>";
    }
}

[Serializable]
public class HpLoseOutCome : EventOutcome
{
    public float amount;
    public bool isPercentage = false;

    public override string Apply(PlayerUnit player)
    {
        var stats = player.GetStat<PlayerStats>();
        if (stats == null) return "";

        float lossValue = amount;
        float currentHp = stats.hp;
        if (isPercentage)
            lossValue = currentHp * (amount / 100f);

        float nextHp = currentHp - lossValue;
        if (nextHp < 1f)
            nextHp = 1f;

        stats.hp = nextHp;

        return $"<color=#FF0000>(체력 {lossValue:F0} 감소)</color>";
    }
}