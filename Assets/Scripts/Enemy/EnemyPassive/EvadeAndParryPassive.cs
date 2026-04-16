using DG.Tweening;
using UnityEngine;

[System.Serializable]
public class EvadeAndParryPassive : PassiveAbility
{
    [Header("Evade & Parry Settings")]
    [Tooltip("회피 확률 (0 ~ 100)")]
    public float dodgeChance = 50f;
    [Tooltip("패링 확률 (0 ~ 100)")]
    public float parryChance = 50f;
    [Tooltip("패링 성공 시 가하는 반격 데미지 비율 (0.2 = 본인 공격력의 20%)")]
    public float parryDamagePercent = 0.2f;

    [Header("Visual Effects")]
    [Tooltip("연출 및 게이지 정지 시간 (기본 0.1초)")]
    public float animDuration = 0.1f;
    [Tooltip("패링 시 보여줄 특수 스프라이트")]
    public Sprite parrySprite;

    public EvadeAndParryPassive()
    {
        combatEvent = CombatEvent.OnBeforeTakeDamage;
    }

    public override void OnEvent(CombatEvent eventType, CombatEventContext context)
    {
        if (eventType == combatEvent && context.target == owner && owner != null && !owner.IsDead)
        {
            if (context.isReflectDamage) return;

            //패링 먼저
            if (Random.Range(0f, 100f) < parryChance)
            {
                GameManager.VFX.ShowText(owner.transform, "패링!", Color.softYellow);
                GameManager.Sound.PlaySFX(SFX.Parry);
                ExecuteParry(context);
                return; 
            }

            if (Random.Range(0f, 100f) < dodgeChance)
            {
                GameManager.VFX.ShowText(owner.transform, "회피!", Color.cyan);
                GameManager.Sound.PlaySFX(SFX.Dodge);
                ExecuteDodge(context);
            }
        }
    }

    private void ExecuteDodge(CombatEventContext context)
    {
        context.value = 0;

        owner.SetIsAttacking(true);
        Transform t = owner.GetSpriteRenderer().transform;
        Vector3 origPos = t.position;
        t.DOKill(true);

        t.DOMoveX(origPos.x - 0.5f, animDuration / 2f)
         .SetEase(Ease.OutQuad)
         .SetLoops(2, LoopType.Yoyo)
         .OnComplete(() =>
         {
             t.position = origPos;
             if (!owner.IsDead) owner.SetIsAttacking(false);
         });
    }

    private void ExecuteParry(CombatEventContext context)
    {
        context.value = 0;

        owner.SetIsAttacking(true);
        var sr = owner.GetSpriteRenderer();
        if (sr != null && parrySprite != null)
        {
            Sprite prevSprite = sr.sprite;
            sr.sprite = parrySprite;

            DOVirtual.DelayedCall(animDuration, () =>
            {
                if (owner != null && !owner.IsDead)
                {
                    sr.sprite = prevSprite;
                    owner.SetIsAttacking(false);
                }
            });
        }
        else
        {
            DOVirtual.DelayedCall(animDuration, () => { if (owner != null && !owner.IsDead) owner.SetIsAttacking(false); });
        }

        HitFlash hitFlash = owner.GetComponent<HitFlash>();
        if (hitFlash != null) hitFlash.Flash(false);

        //반격
        if (context.source != null && !context.source.IsDead)
        {
            var stats = owner.GetStat<UnitStats>();
            float reflectValue = stats.attackDamage.GetValue() * parryDamagePercent;
            CombatEventContext reflectCtx = new CombatEventContext(
                owner,
                context.source,
                reflectValue,
                DamageType.Normal,
                true,
                false,
                null,
                AttackEvadeType.None
            );

            context.source.TakeDamage(reflectCtx);
        }


    }
}