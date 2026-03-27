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
            // TODO: 화면에 "구속됨!" 같은 플로팅 텍스트
            return false;
        }

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