using UnityEngine;

[System.Serializable]
public class DodgeAbility : ActiveAbility
{
    public override void OnAdded()
    {
        actionType = ActionType.Dodge;
    }

    protected override bool CheckAndConsumeCost(PlayerUnit player)
    {
        var stats = player.GetStat<PlayerStats>();
        float cost = stats.dodgeCost.GetValue();

        if (stats.stamina >= cost)
        {
            stats.stamina -= cost;
            return true;
        }

        return false;
    }

    protected override float GetCooldown(PlayerUnit player)
    {
        return player.GetStat<PlayerStats>().dodgeCoolTime;
    }

    protected override void ExecuteSkill(PlayerUnit player)
    {
        var stats = player.GetStat<PlayerStats>();
        player.ChangeState(PlayerState.Dodging, stats.dodgeDuration);
    }
}