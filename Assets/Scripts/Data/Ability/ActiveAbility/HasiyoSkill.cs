using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

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

        GameManager.VFX.ShowText(player.transform, "FP 부족!", Color.gray);
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

        switch (magicType)
        {
            case 0:
                foreach (var enemy in enemies)
                    Fireball(player,enemy);
                break;
            case 1:
                foreach (var enemy in enemies)
                    Slow(player, enemy);
                break;
            case 2:
                foreach (var enemy in enemies)
                    Disarm(player, enemy);
                break;
        }
    }

    private void Fireball(PlayerUnit player, EnemyUnit enemy)
    {
        GameManager.VFX.PlayEffect(
            player.transform.position,
            enemy.transform.position,
            AttackVFXType.HasiyoMeteo,
            0f,
            Color.white,
            onHit: () =>
            {
                if (enemy != null && !enemy.IsDead)
                {
                    var stats = player.GetStat<PlayerStats>();
                    float baseDamage = stats.attackDamage.GetValue();
                    CombatEventContext ctx = new CombatEventContext(player, enemy, baseDamage, DamageType.Fixed, false);
                    enemy.TakeDamage(ctx);
                }
            });
    }

    private void Slow(PlayerUnit player, EnemyUnit enemy)
    {
        GameManager.VFX.PlayEffect(
            player.transform.position,
            enemy.transform.position,
            AttackVFXType.HasiyoIce,
            0f,
            Color.white,
            onHit: () =>
            {
                if (enemy != null && !enemy.IsDead)
                {
                    float slowDuration = 5f;
                    SlowEffect slow = new SlowEffect(slowDuration, 1, false, 0.5f);
                    enemy.AddAbility(slow);
                }
            });

        GameManager.Sound.PlaySFX(SFX.HasiyoIce);
    }

    private void Disarm(PlayerUnit player, EnemyUnit enemy)
    {
        GameManager.VFX.PlayEffect(
            player.transform.position,
            enemy.transform.position,
            AttackVFXType.HasiyoPlant,
            0f,
            Color.white,
            onHit: () =>
            {
                if (enemy != null && !enemy.IsDead)
                {
                    float disarmDuration = 2f;
                    DisarmEffect disarm = new DisarmEffect(disarmDuration, 1, false);
                    enemy.AddAbility(disarm);
                }
            }
        );

        GameManager.Sound.PlaySFX(SFX.HasiyoWood);
    }
}