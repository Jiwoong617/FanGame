using DG.Tweening;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

[System.Serializable]
public class RoseSkill : ActiveAbility
{
    [SerializeField] private Sprite effectSprite;

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
        HitEffect(target);
        if (actualDamage > 0)
        {
            player.Heal(actualDamage * 0.5f);
        }

        VampireEffect vampireBuff = new VampireEffect(10f, 1, false, 0.1f);
        player.AddAbility(vampireBuff);
    }


    private void HitEffect(CombatUnit enemy)
    {
        Vector3 randPos = new Vector3(Random.Range(-0.2f, 0.2f), Random.Range(-0.2f, 0.2f), 0);
        GameManager.VFX.ShowCustomEffect(enemy.transform.position + randPos, effectSprite, (t, sr, onComplete) =>
        {
            float angle = Random.Range(0, 360f);
            t.rotation = Quaternion.Euler(0, 0, angle);
            t.localScale = new Vector3(0.2f, 0.1f, 1f);

            Sequence seq = DOTween.Sequence();
            seq.Append(t.DOScale(new Vector3(1f, 0.8f, 1f), 0.05f).SetEase(Ease.OutBack));
            seq.Append(sr.DOFade(0f, 0.25f).SetEase(Ease.InQuad));

            seq.OnComplete(() => onComplete.Invoke());
        });
    }

}