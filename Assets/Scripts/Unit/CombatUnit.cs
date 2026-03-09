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
    protected UnitData unitData;

    public Action<CombatUnit> OnUnitDead;
    public event Action<CombatEventContext> OnDamageTextRequested;
    public event Action<float> OnHealTextRequested;

    //이건 ui 띄울것들임
    public event Action<float> OnActionBarUpdated;
    public event Action<StatusEffect> OnBuffAdded;
    public event Action<StatusEffect> OnBuffRemoved;
    public event Action<StatusEffect> OnBuffUpdated;

    protected UnitStats stats;
    protected CombatUnit target;
    protected float attackTimer = 0f;
    protected bool isAttacking = false;
    public bool IsAttacking => isAttacking;

    // 런타임 능력 리스트
    protected List<Ability> abilities = new List<Ability>();

    public bool IsDead => stats.hp <= 0;


    protected HitFlash hitEffect;


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
        target = inTarget;
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
        ,List<StatusEffect> debuffs = null, bool useMoveAnim = true)
    {
        if (target == null || IsDead) return;

        isAttacking = true;

        if (spriteRenderer != null && unitData.unitBasicAttackSprite != null)
        {
            spriteRenderer.sprite = unitData.unitBasicAttackSprite;
        }

        if (useMoveAnim) // 움직이는 공격
            PerformPhysicalAttack(target, damage, onHit, onCritical, debuffs);
        else // 제자리 공격
            PerformStationaryAttack(target, damage, onHit, onCritical, debuffs);
    }

    private void PerformPhysicalAttack(CombatUnit target, float damage, bool onHit, bool onCritical, List<StatusEffect> debuffs)
    {
        Vector3 originalPos = transform.position;
        Vector3 dir = (target.transform.position - transform.position).normalized;
        Vector3 attackPos = originalPos + dir * 1f;

        float attackInterval = currentAttackThreshold / stats.attackSpeed.GetValue();
        float maxAnimTime = Mathf.Min(0.3f, attackInterval * 0.8f);

        transform.DOKill(true);

        Sequence attackSeq = DOTween.Sequence();
        attackSeq.Append(transform.DOMove(attackPos, maxAnimTime * 0.2f).SetEase(Ease.OutExpo));
        attackSeq.AppendCallback(() => ExecuteHit(target, damage, onHit, onCritical, debuffs));
        attackSeq.AppendInterval(maxAnimTime * 0.2f);
        attackSeq.Append(transform.DOMove(originalPos, maxAnimTime * 0.6f).SetEase(Ease.OutCirc));

        attackSeq.OnComplete(() =>
        {
            isAttacking = false;
            ChangeToIdleSprite();
        });
    }

    private void PerformStationaryAttack(CombatUnit target, float damage, bool onHit, bool onCritical, List<StatusEffect> debuffs)
    {
        float castDelay = 0.2f;

        Sequence attackSeq = DOTween.Sequence();
        attackSeq.AppendInterval(castDelay);
        attackSeq.AppendCallback(() => ExecuteHit(target, damage, onHit, onCritical, debuffs, false));
        attackSeq.OnComplete(() =>
        {
            isAttacking = false;
            ChangeToIdleSprite();
        });
    }

    private void ExecuteHit(CombatUnit target, float damage, bool onHit, bool onCritical, List<StatusEffect> debuffs, bool attackVFX = true)
    {
        if (target == null || target.IsDead || IsDead) return;
        bool isCrit = false;

        if (UnityEngine.Random.Range(0f, 100f) < stats.criticalChance.GetValue())
        {
            isCrit = true;
            damage *= (stats.criticalDamage.GetValue() / 100f);
        }

        CombatEventContext attackCtx = new CombatEventContext(
            this, target, damage, DamageType.Normal, false, isCrit, debuffs
        );

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
                    Mathf.Approximately(existingStatus.effectValue, newStatus.effectValue))
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

        OnHealTextRequested?.Invoke(finalHeal);
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

    protected virtual void ChangeToIdleSprite()
    {
        if (spriteRenderer != null && unitData.unitSprite != null)
        {
            spriteRenderer.sprite = unitData.unitSprite;
        }
    }
}
