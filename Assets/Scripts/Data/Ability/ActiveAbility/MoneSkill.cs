using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

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

        GameManager.VFX.ShowText(player.transform, "FP 부족!", Color.gray);
        return false;
    }

    protected override float GetCooldown(PlayerUnit player)
    {
        return player.GetStat<PlayerStats>().skillCoolTime.GetValue();
    }

    protected override void ExecuteSkill(PlayerUnit player)
    {
        SpriteRenderer targetSR = owner.GetComponentInChildren<SpriteRenderer>();
        if (targetSR != null)
        {
            targetSR.DOFade(0.3f, 0.2f).SetEase(Ease.InOutSine).OnComplete(() =>
            {
                targetSR.color = new Color(1, 1, 1, 0.3f);
            });
        }
        GameManager.Sound.PlaySFX(SFX.MoneSkll);

        player.ChangeState(PlayerState.Skill, 0.2f);

        MoneEffect moneBuff = new MoneEffect();
        player.AddAbility(moneBuff);
    }
}
