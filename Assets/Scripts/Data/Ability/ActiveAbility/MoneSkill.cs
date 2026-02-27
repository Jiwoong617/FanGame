using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MoneSkill : ActiveAbility
{
    public MoneSkill()
    {
        actionType = ActionType.Skill;
    }

    protected override bool CheckAndConsumeCost(PlayerUnit player)
    {
        var stats = player.GetStat<PlayerStats>();
        float cost = 1f;

        if (stats.fp >= cost)
        {
            stats.fp -= cost;
            return true;
        }

        return false;
    }

    protected override float GetCooldown(PlayerUnit player)
    {
        return player.GetStat<PlayerStats>().skillCoolTime.GetValue();
    }

    protected override void ExecuteSkill(PlayerUnit player)
    {
        player.ChangeState(PlayerState.Skill, 0.2f);

        MoneEffect moneBuff = new MoneEffect();
        player.AddAbility(moneBuff);
    }
}
