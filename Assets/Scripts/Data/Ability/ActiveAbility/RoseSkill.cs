using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

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

        GameManager.VFX.ShowText(player.transform, "FP 부족!", Color.gray);
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

        GameManager.VFX.PlayEffect(player.transform.position, target.transform.position, AttackVFXType.RoseSkill, 0f, Color.white);

        if (actualDamage > 0)
        {
            player.Heal(actualDamage * 0.5f, false);
        }

        VampireEffect vampireBuff = new VampireEffect(10f, 1, false, 0.1f);
        player.AddAbility(vampireBuff);

        GameManager.Sound.PlaySFX(SFX.RoseSkill);
    }
}