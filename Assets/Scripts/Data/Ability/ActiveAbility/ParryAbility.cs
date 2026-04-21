using UnityEngine;

[System.Serializable]
public class ParryAbility : ActiveAbility
{
    protected override bool CheckAndConsumeCost(PlayerUnit player)
    {
        var stats = player.GetStat<PlayerStats>();
        float cost = stats.parryCost.GetValue();

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
        return player.GetStat<PlayerStats>().parrayCoolTime;
    }

    protected override void ExecuteSkill(PlayerUnit player)
    {
        var stats = player.GetStat<PlayerStats>();
        player.ChangeState(PlayerState.Parrying, stats.parryDuration);
        Debug.Log("패링 발동!");
    }

    public override void OnEvent(CombatEvent eventType, CombatEventContext ctx)
    {
        // OnParrySuccess
        if (eventType == CombatEvent.OnParrySuccess && ctx.target == owner)
        {
            DisarmEffect disarm = new DisarmEffect(0.5f, 1, false);
            ctx.source.AddAbility(disarm);
        }
    }
}