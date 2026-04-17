using UnityEngine;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;

public class DragonUnit : EnemyUnit
{
    [Header("Dragon Sprites")]
    public Sprite sleepSprite;
    public Sprite fearSprite;
    public Sprite awakeSprite;

    public bool IsSleeping { get; private set; } = true;

    public override void Init(UnitData unitData)
    {
        base.Init(unitData);
        IsSleeping = true;
        if (sleepSprite != null)
            spriteRenderer.sprite = sleepSprite;
    }

    public override void OnUpdate(float delta)
    {
        if (IsSleeping)
        {
            TickAbilities(delta);
            return;
        }
        base.OnUpdate(delta);
    }

    public override void AddAbility(Ability newAbility)
    {
        if (newAbility is StatusEffect effect)
        {
            if (IsDebuff(effect.effectType))
                return; // 디버프 무시
        }

        base.AddAbility(newAbility);
    }

    public void WakeUp(List<StatusEffect> debuffsToPlayer)
    {
        if (!IsSleeping) return;
        IsSleeping = false;
        StartCoroutine(WakeUpRoutine(debuffsToPlayer));
    }

    private IEnumerator WakeUpRoutine(List<StatusEffect> debuffsToPlayer)
    {
        if (fearSprite != null) spriteRenderer.sprite = fearSprite;

        // 카메라 흔들림
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            Vector3 originalCamPos = mainCam.transform.position;
            mainCam.transform.DOShakePosition(0.8f, 0.4f, 20, 90f)
                .OnComplete(() => mainCam.transform.position = originalCamPos);
        }
        //transform.DOShakePosition(1f, 0.5f, 20, 90f);

        PlayerUnit player = GameManager.Instance.Player;
        if (player != null && debuffsToPlayer != null)
        {
            foreach (var debuff in debuffsToPlayer)
            {
                player.AddAbility(debuff.Clone());
            }
        }

        GameManager.Sound.PlaySFX(SFX.DragonRoar);

        yield return new WaitForSeconds(1f);
        ChangeToIdleSprite();
    }

    public override void ChangeToIdleSprite()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = IsSleeping ? sleepSprite : awakeSprite;
        }
    }

    private bool IsDebuff(EffectType type)
    {
        return type == EffectType.Shackle 
            || type == EffectType.Disarm 
            || type == EffectType.DamageAmplification
            || type == EffectType.AttackDown
            || type == EffectType.Slow
            || type == EffectType.Taunt;
    }
}