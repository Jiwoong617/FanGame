using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class RoseSkill : ActiveAbility
{
    public RoseSkill()
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
        CombatUnit target = player.GetTarget();
        if (target == null || target.IsDead) return;

        player.ChangeState(PlayerState.Skill, 0.3f);
        var stats = player.GetStat<PlayerStats>();

        float baseDmg = stats.attackDamage.GetValue();
        float hpBonusDmg = stats.maxHp.GetValue() * 0.5f;
        float totalDmg = baseDmg + hpBonusDmg;

        CombatEventContext ctx = new CombatEventContext(player, target, totalDmg, DamageType.Normal, false);
        float actualDamage = target.TakeDamage(ctx);
        if (actualDamage > 0)
        {
            player.Heal(actualDamage * 0.5f);
        }

        VampireEffect vampireBuff = new VampireEffect(10f, 1, false, 0.1f);
        player.AddAbility(vampireBuff);
    }
}