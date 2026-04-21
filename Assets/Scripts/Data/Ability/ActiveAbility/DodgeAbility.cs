using UnityEngine;

[System.Serializable]
public class DodgeAbility : ActiveAbility
{
    protected override bool CheckAndConsumeCost(PlayerUnit player)
    {
        var stats = player.GetStat<PlayerStats>();
        float cost = stats.dodgeCost.GetValue();

        if (player.HasStatusEffect(EffectType.Shackle))
        {
            GameManager.VFX.ShowText(player.transform, "구속됨!", Color.gray);
            return false;
        }

        if (stats.stamina >= cost)
        {
            stats.stamina -= cost;
            player.TriggerAbility(CombatEvent.OnStaminaUsed, new CombatEventContext(player, player, cost));
            return true;
        }

        GameManager.VFX.ShowText(player.transform, "스태미나 부족!", Color.gray);
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