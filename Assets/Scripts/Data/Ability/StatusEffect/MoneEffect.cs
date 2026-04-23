using DG.Tweening;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

[System.Serializable]
public class MoneEffect : StatusEffect
{
    private StatModifier critChanceMod;
    private StatModifier critDmgMod;

    public MoneEffect()
    {
        effectType = EffectType.Mone;
        combatEvent = CombatEvent.OnAttack;
    }

    public MoneEffect(float duration, bool isPermanent)
    {
        effectType = EffectType.Mone;
        combatEvent = CombatEvent.OnAttack;
        this.duration = duration;
        this.isPermanent = isPermanent;
    }

    public override void Init(CombatUnit owner)
    {
        base.Init(owner);
        isPermanent = false;
    }

    protected override void OnAdded()
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


        critChanceMod = new StatModifier(100f, StatModType.Flat);
        critDmgMod = new StatModifier(50f, StatModType.Flat);

        var stats = owner.GetStat<UnitStats>();
        stats.criticalChance.AddModifier(critChanceMod);
        stats.criticalDamage.AddModifier(critDmgMod);
    }

    public override void OnEvent(CombatEvent eventType, CombatEventContext ctx)
    {
        if (eventType == CombatEvent.OnAttack && ctx.source == owner)
        {
            owner.GetSpriteRenderer().DOFade(1f, 0.15f);
            IsFinished = true;
        }
    }

    public override void OnRemoved()
    {
        // 스프라이트 투명도 복원
        var sr = owner?.GetSpriteRenderer();
        if (sr != null)
            sr.DOFade(1f, 0.15f);

        // 버프가 사라질 때 스탯 보너스 회수
        var stats = owner.GetStat<UnitStats>();
        if (stats != null)
        {
            if (critChanceMod != null)
                stats.criticalChance.RemoveModifier(critChanceMod);
            if (critDmgMod != null)
                stats.criticalDamage.RemoveModifier(critDmgMod);
        }
    }
}