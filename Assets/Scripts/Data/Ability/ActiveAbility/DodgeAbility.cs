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
        return player.GetStat<PlayerStats>().dodgeCoolTime;
    }

    protected override void ExecuteSkill(PlayerUnit player)
    {
        var stats = player.GetStat<PlayerStats>();
        player.ChangeState(PlayerState.Dodging, stats.dodgeDuration);
    }
}