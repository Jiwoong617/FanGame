using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PopoSkill : ActiveAbility
{
    public PopoSkill()
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

        var stats = player.GetStat<PlayerStats>();

        float attackSpeed = stats.attackSpeed.GetValue();
        int hitCount = Mathf.FloorToInt(attackSpeed * 2);

        // 일단 공속이 매우 낮으면 1타는 보장
        if (hitCount < 1)
            hitCount = 1;

        float baseDmg = stats.attackDamage.GetValue();
        float damagePerHit = Mathf.Floor(Mathf.Max(1f, baseDmg * 0.25f));

        float interval = 0.05f; //이거 연타 속도임
        float totalDuration = hitCount * interval;
        player.ChangeState(PlayerState.Skill, totalDuration + 0.1f);

        // 연타
        Sequence skillSeq = DOTween.Sequence();
        for (int i = 0; i < hitCount; i++)
        {
            skillSeq.AppendCallback(() =>
            {
                if (target == null || target.IsDead) return;

                bool isCrit = false;
                float finalDmg = damagePerHit;
                if (Random.Range(0f, 100f) < stats.criticalChance.GetValue())
                {
                    isCrit = true;
                    finalDmg *= (stats.criticalDamage.GetValue() / 100f);
                }

                CombatEventContext ctx = new CombatEventContext(player, target, finalDmg, DamageType.Normal, false, isCrit);
                float actualDamage = target.TakeDamage(ctx);

                GameManager.VFX.PlayEffect(player.transform.position, target.transform.position, AttackVFXType.Popo, 0f,
                    Color.white);

                if (actualDamage > 0)
                {
                    ctx.value = actualDamage;
                    player.TriggerAbility(CombatEvent.OnAttack, ctx);
                }
            });

            skillSeq.AppendInterval(interval);
        }
    }
}
