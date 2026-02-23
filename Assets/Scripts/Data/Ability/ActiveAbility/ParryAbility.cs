using UnityEngine;

[System.Serializable]
public class ParryAbility : ActiveAbility
{
    public override void OnAdded()
    {
        actionType = ActionType.Parry;
    }

    protected override bool CheckAndConsumeCost(PlayerUnit player)
    {
        var stats = player.GetStat<PlayerStats>();
        float cost = stats.parryCost.GetValue(); // Stat 패시브 자동 적용됨!

        if (stats.stamina >= cost)
        {
            stats.stamina -= cost;
            return true;
        }
        return false;
    }

    protected override float GetCooldown(PlayerUnit player)
    {
        return player.GetStat<PlayerStats>().parrayCoolTime;
    }

    protected override void ExecuteSkill(PlayerUnit player)
    {
        var stats = player.GetStat<PlayerStats>();
        player.ChangeState(PlayerState.Parrying, stats.parryDuration);
        Debug.Log("패링 발동!");
    }
}