using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CombatUnit : MonoBehaviour
{
    protected float currentAttackThreshold = 1f;

    [SerializeField] protected CombatUnitUI combatUI;

    protected SpriteRenderer spriteRenderer;
    public UnitData unitData { get; protected set; }

    public Action<CombatUnit> OnUnitDead;
    public event Action<CombatEventContext> OnDamageTextRequested;
    public event Action<CombatUnit> OnTargetChanged;

    //이건 ui 띄울것들임
    public event Action<float> OnActionBarUpdated;
    public event Action<StatusEffect> OnBuffAdded;
    public event Action<StatusEffect> OnBuffRemoved;
    public event Action<StatusEffect> OnBuffUpdated;

    protected UnitStats stats;
    protected CombatUnit target;
    protected float attackTimer = 0f;
    protected bool isAttacking = false;
    protected bool isDeathCanceled = false;

    public bool IsAttacking => isAttacking;
    public bool IsDead => stats.hp <= 0;


    // 런타임 능력 리스트
    protected List<Ability> abilities = new List<Ability>();


    protected HitFlash hitEffect;
    protected Sprite overrideActionSprite = null;


    protected virtual void Awake()
    {
        hitEffect = GetComponent<HitFlash>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>(); // 임시 코드라 좀 더 정확하게 짜야함
    }

    protected void ProcessAttackLoop(float delta)
    {
        if (IsDead || target == null || target.IsDead) return;

        if (isAttacking) return;

        attackTimer += (delta * stats.attackSpeed.GetValue());
        OnActionBarUpdated?.Invoke(Mathf.Clamp01(attackTimer / currentAttackThreshold));

        if (attackTimer >= currentAttackThreshold)
        {
            attackTimer = 0f;
            OnActionBarUpdated?.Invoke(0f);
            Attack(target, stats.attackDamage.GetValue(), true, true);
        }
    }

    public virtual void SetTarget(CombatUnit inTarget)
    {
        if (target != inTarget)
        {
            target = inTarget;
            OnTargetChanged?.Invoke(target);
        }
    }

    public CombatUnit GetTarget()
    {
        return target;
    }

    public virtual void OnBattleStart()
    {
        TriggerAbility(CombatEvent.OnBattleStart, new CombatEventContext(this, target, 0));
    }

    public virtual void OnBattleEnd()
    {
        TriggerAbility(CombatEvent.OnBattleEnd, new CombatEventContext(this, target, 0));

        for (int i = abilities.Count - 1; i >= 0; i--)
        {
            Ability ability = abilities[i];

            if (ability.IsFinished)
            {
                ability.OnRemoved();
                if (ability is StatusEffect finishedStatus)
                    OnBuffRemoved?.Invoke(finishedStatus);

                abilities.RemoveAt(i);
                continue;
            }

            if (ability is StatusEffect status)
            {
                if (!status.isPermanent || status.IsFinished)
                {
                    status.OnRemoved();
                    OnBuffRemoved?.Invoke(status);
                    abilities.RemoveAt(i);
                }
            }
        }
    }

    public virtual void OnUpdate(float delta)
    {
        if (IsDead) return;

        for (int i = abilities.Count - 1; i >= 0; i--)
        {
            var ability = abilities[i];
            ability.OnUpdate(delta);
            
            if (ability.IsFinished)
            {
                ability.OnRemoved();
                if (ability is StatusEffect status)
                    OnBuffRemoved?.Invoke(status);

                abilities.RemoveAt(i);
            }
        }
    }
    
    public virtual void Init(UnitData data)
    {
        this.unitData = data;
    
        // 초기 스프라이트 설정
        if (spriteRenderer != null && unitData != null && unitData.unitSprite != null)
        {
            spriteRenderer.sprite = unitData.unitSprite;
        }
    }
    public abstract void OnDead();
    public abstract float TakeDamage(CombatEventContext info);
    public abstract T GetStat<T>() where T : UnitStats;

    public virtual void Attack(CombatUnit target, float damage, bool onHit, bool onCritical
        ,List<StatusEffect> debuffs = null, bool useMoveAnim = true, DamageType damageType = DamageType.Normal, Action<float> onDamageDealt = null)
    {
        if (target == null || IsDead) return;

        isAttacking = true;


        Sprite spriteToUse = overrideActionSprite != null ? overrideActionSprite : (unitData != null ? unitData.unitBasicAttackSprite : null);
        if (spriteRenderer != null && spriteToUse != null)
            spriteRenderer.sprite = spriteToUse;

        overrideActionSprite = null;

        if (useMoveAnim) // 움직이는 공격
            PerformPhysicalAttack(target, damage, onHit, onCritical, debuffs, damageType, onDamageDealt);
        else // 제자리 공격
            PerformStationaryAttack(target, damage, onHit, onCritical, debuffs, damageType, onDamageDealt);
    }

    private void PerformPhysicalAttack(CombatUnit target, float damage, bool onHit, bool onCritical,
        List<StatusEffect> debuffs, DamageType damageType, Action<float> onDamageDealt)
    {
        Vector3 originalPos = transform.position;
        Vector3 dir = (target.transform.position - transform.position).normalized;
        Vector3 attackPos = originalPos + dir * 1f;

        float attackInterval = currentAttackThreshold / stats.attackSpeed.GetValue();
        float maxAnimTime = Mathf.Min(0.3f, attackInterval * 0.8f);
        float forwardTime = maxAnimTime * 0.2f;
        float pauseTime = maxAnimTime * 0.2f;
        float returnTime = maxAnimTime * 0.6f;

        transform.DOKill(true);
        PlayAttackVFX(target.transform.position, forwardTime);

        Sequence attackSeq = DOTween.Sequence();
        attackSeq.Append(transform.DOMove(attackPos, forwardTime).SetEase(Ease.OutExpo));
        attackSeq.AppendCallback(() => ExecuteHit(target, damage, onHit, onCritical, debuffs, damageType, true, onDamageDealt));
        attackSeq.AppendInterval(pauseTime);
        attackSeq.Append(transform.DOMove(originalPos, returnTime).SetEase(Ease.OutCirc));
        attackSeq.OnComplete(() =>
        {
            isAttacking = false;
            ChangeToIdleSprite();
        });
    }

    private void PerformStationaryAttack(CombatUnit target, float damage, bool onHit, bool onCritical,
        List<StatusEffect> debuffs, DamageType damageType, Action<float> onDamageDealt)
    {
        float castDelay = 0.2f;

        Sequence attackSeq = DOTween.Sequence();
        attackSeq.AppendInterval(castDelay);
        attackSeq.AppendCallback(() => ExecuteHit(target, damage, onHit, onCritical, debuffs, damageType, false, onDamageDealt));
        attackSeq.OnComplete(() =>
        {
            isAttacking = false;
            ChangeToIdleSprite();
        });
    }

    private void ExecuteHit(CombatUnit target, float damage, bool onHit, bool onCritical,
        List<StatusEffect> debuffs, DamageType damageType, bool attackVFX, Action<float> onDamageDealt)
    {
        if (target == null || target.IsDead || IsDead) return;
        bool isCrit = false;

        if (UnityEngine.Random.Range(0f, 100f) < stats.criticalChance.GetValue())
        {
            isCrit = true;
            damage *= (stats.criticalDamage.GetValue() / 100f);
        }

        CombatEventContext attackCtx = new CombatEventContext(
            this, target, damage, damageType, false, isCrit, debuffs
        );

        TriggerAbility(CombatEvent.OnBeforeAttack, attackCtx);

        float actualDamage = target.TakeDamage(attackCtx);
        if (actualDamage >= 0)
        {
            if(attackVFX)
                PlayHitVFX(target.transform.position);
            else
            {
                //TODO : 디버프 이펙트
            }

            attackCtx.value = actualDamage;
            if (onHit) TriggerAbility(CombatEvent.OnAttack, attackCtx);
            if (isCrit && onCritical) TriggerAbility(CombatEvent.OnCritical, attackCtx);

            onDamageDealt?.Invoke(actualDamage);
        }
    }

    protected void InitializeAbilities(UnitData data)
    {
        abilities.Clear();
        if (data.startingAbilities != null)
        {
            foreach (var ability in data.startingAbilities)
            {
                if (ability != null)
                {
                    AddAbility(ability.Clone());
                }
            }
        }
    }

    public virtual void AddAbility(Ability newAbility)
    {
        if (newAbility == null) return;

        if (newAbility is StatusEffect newStatus)
        {
            foreach (var ability in abilities)
            {
                if (ability is StatusEffect existingStatus &&
                    existingStatus.effectType == newStatus.effectType &&
                    existingStatus.isPermanent == newStatus.isPermanent &&
                    Mathf.Approximately(existingStatus.effectValue, newStatus.effectValue) &&
                    !existingStatus.IsFinished)
                {
                    existingStatus.AddStack(newStatus.stacks, newStatus.duration);
                    OnBuffUpdated?.Invoke(existingStatus);
                    return;
                }
            }
        }

        newAbility.Init(this);
        abilities.Add(newAbility);

        if (newAbility is StatusEffect addedStatus)
        {
            OnBuffAdded?.Invoke(addedStatus);
        }
    }

    public void TriggerAbility(CombatEvent type, CombatEventContext cec)
    {
        for (int i = 0; i < abilities.Count; i++)
            abilities[i].OnEvent(type, cec);
    }

    public void Heal(float amount)
    {
        if (IsDead || amount <= 0) return;

        float finalHeal = Mathf.Max(1f, amount);
        stats.hp += finalHeal;

        GameManager.VFX.ShowHealText(transform, finalHeal);
    }

    protected void RequestDamageText(CombatEventContext ctx)
    {
        OnDamageTextRequested?.Invoke(ctx);
    }

    protected void RequestActionBarUpdate(float value)
    {
        OnActionBarUpdated?.Invoke(value);
    }

    protected virtual void PlayAttackVFX(Vector3 targetPos, float hitDelay) { }
    protected virtual void PlayHitVFX(Vector3 targetPos) { }

    public virtual void ChangeToIdleSprite()
    {
        if (spriteRenderer != null && unitData.unitSprite != null)
        {
            spriteRenderer.sprite = unitData.unitSprite;
        }
    }

    public void UpdateBuffUI(StatusEffect effect)
    {
        OnBuffUpdated?.Invoke(effect);
    }

    public SpriteRenderer GetSpriteRenderer() => spriteRenderer;

    public bool HasStatusEffect(EffectType type)
    {
        for (int i = 0; i < abilities.Count; i++)
        {
            if (abilities[i] is StatusEffect status && status.effectType == type && !status.IsFinished)
                return true;
        }
        return false;
    }

    public StatusEffect GetStatusEffect(EffectType type)
    {
        for (int i = 0; i < abilities.Count; i++)
        {
            if (abilities[i] is StatusEffect status && status.effectType == type && !status.IsFinished)
                return status;
        }
        return null;
    }

    public void SetActionSprite(Sprite customSprite)
    {
        overrideActionSprite = customSprite;
    }

    public virtual void PlayActionAnimation(Sprite customSprite, float duration, Action onActionExecute)
    {
        if (IsDead) return;

        isAttacking = true;

        if (spriteRenderer != null && customSprite != null)
            spriteRenderer.sprite = customSprite;

        Sequence actionSeq = DOTween.Sequence();

        // 절반 대기 후, 실제 효과 발동
        actionSeq.AppendInterval(duration * 0.5f);
        actionSeq.AppendCallback(() =>
        {
            if (!IsDead)
                onActionExecute?.Invoke();
        });
        actionSeq.AppendInterval(duration * 0.5f);
        actionSeq.OnComplete(() =>
        {
            isAttacking = false;
            if (!IsDead)
                ChangeToIdleSprite();
        });
    }

    public void CancelDeath()
    {
        isDeathCanceled = true;
    }

    public void SetIsAttacking(bool state)
    {
        isAttacking = state;
    }
}
