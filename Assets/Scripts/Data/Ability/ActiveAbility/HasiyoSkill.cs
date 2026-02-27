using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class HasiyoSkill : ActiveAbility
{
    public HasiyoSkill()
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
        List<EnemyUnit> enemies = GameManager.Battle.GetAliveEnemies();
        if (enemies == null || enemies.Count == 0)
            return;

        //TODO : 이거 일단 0.3초로 해놓음
        player.ChangeState(PlayerState.Skill, 0.3f);

        // 0 불, 1 얼음, 2 대지
        int magicType = Random.Range(0, 3);

        var stats = player.GetStat<PlayerStats>();
        float baseDamage = stats.attackDamage.GetValue();

        switch (magicType)
        {
            case 0:
                foreach (var enemy in enemies)
                {
                    CombatEventContext ctx = new CombatEventContext(player, enemy, baseDamage, DamageType.Fixed, false);
                    float actualDamage = enemy.TakeDamage(ctx);

                    //이거 온힛은 일단 주석처리
                    //if (actualDamage > 0 && !player.IsDead)
                    //{
                    //    ctx.value = actualDamage;
                    //    player.TriggerAbility(CombatEvent.OnAttack, ctx);
                    //}
                }
                break;

            case 1:
                foreach (var enemy in enemies)
                {
                    SlowEffect slow = new SlowEffect(5f, 1, false, 0.5f);
                    enemy.AddAbility(slow);
                }
                break;

            case 2:
                foreach (var enemy in enemies)
                {
                    DisarmEffect disarm = new DisarmEffect(2f, 1, false);
                    enemy.AddAbility(disarm);
                }
                break;
        }
    }
}