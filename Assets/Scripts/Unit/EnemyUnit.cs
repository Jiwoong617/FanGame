using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyUnit : CombatUnit
{
    public event Action<PassiveAbility> OnPassiveAdded;

    [Header("Pattern Settings")]
    [SerializeReference, SerializeReferenceDropdown]
    public List<EnemyPattern> patterns = new List<EnemyPattern>();
    [SerializeField]
    protected bool isUsePatternFirst = false;
    
    private EnemyPattern currentRunningPattern = null;
    private EnemyPattern nextPattern = null;
    private EnemyPattern lastExecutedPattern = null;

    

    public override void Init(UnitData unitData)
    {
        base.Init(unitData);
        stats = new UnitStats(unitData);

        foreach (var pattern in patterns)
        {
            pattern.ResetPattern();
        }

        if (combatUI == null)
            combatUI = GetComponentInChildren<CombatUnitUI>();
        combatUI.SetOwner(this);

        attackTimer = 0f;

        InitializeAbilities(unitData);

        OnDamageTextRequested += GameManager.VFX.ShowDamageText;

        if(isUsePatternFirst)
            DecideNextAction();
    }

    public override void AddAbility(Ability newAbility)
    {
        base.AddAbility(newAbility);

        if (newAbility is PassiveAbility addedPassive)
        {
            OnPassiveAdded?.Invoke(addedPassive);
        }
    }

    public override void OnUpdate(float delta)
    {
        if (target == null || target.IsDead || IsDead) return;

        base.OnUpdate(delta);

        if (currentRunningPattern != null)
        {
            bool isFinished = currentRunningPattern.OnUpdate(this, delta);
            if (isFinished)
            {
                FinishPattern();
            }
            return;
        }

        if (currentRunningPattern == null)
        {
            ProcessAttackLoop(delta);
        }
    }

    public override void Attack(CombatUnit target, float damage, bool onHit, bool onCritical,
        List<StatusEffect> debuffs = null, bool useMoveAnim = true, DamageType damageType = DamageType.Normal, Action<float> onDamageDealt = null)
    {
        if (nextPattern != null) // 다음 패턴으로 변경
        {
            currentRunningPattern = nextPattern;
            lastExecutedPattern = nextPattern;
            currentRunningPattern.UpdateConditionOnExecute(this);

            nextPattern = null;
            currentRunningPattern.OnEnter(this);
        }
        else if (currentRunningPattern != null) // 현재 패턴 실행
        {
            // 커스텀 스프라이트 변경
            if (currentRunningPattern.actionSprite != null)
                SetActionSprite(currentRunningPattern.actionSprite);

            base.Attack(target, damage, onHit, onCritical, debuffs, useMoveAnim, damageType, onDamageDealt);
        }
        else // 둘다 null(기본 공격이면 기본 공격하고 패턴 확인)
        {
            base.Attack(target, damage, onHit, onCritical, debuffs, useMoveAnim, damageType, onDamageDealt);
            DecideNextAction();
        }
    }

    private void FinishPattern()
    {
        if (currentRunningPattern != null)
        {
            currentRunningPattern.lastExecutionTime = Time.time;
            currentRunningPattern.OnExit(this);
            currentRunningPattern = null;
        }
        
        // 패턴 종료 후 다음 행동 결정
        DecideNextAction();
    }

    private EnemyPattern GetAvailablePattern()
    {
        if (patterns == null || patterns.Count == 0) return null;

        List<EnemyPattern> validCandidates = new List<EnemyPattern>();
        int totalWeight = 0;
        float now = Time.time;

        foreach (var pattern in patterns)
        {
            // 쿨타임 체크
            if (now < pattern.lastExecutionTime + pattern.cooldown) continue;
            // 연속 사용 방지
            if (patterns.Count > 1 && pattern == lastExecutedPattern) continue;
            // 패턴 실행 조건
            if (!pattern.CanExecute(this)) continue;

            validCandidates.Add(pattern);
            totalWeight += pattern.triggerChance;
        }

        if (validCandidates.Count == 0)
            return null;

        int randomPoint = UnityEngine.Random.Range(0, totalWeight);
        int currentSum = 0;
        foreach (var pattern in validCandidates)
        {
            currentSum += pattern.triggerChance;
            if (randomPoint < currentSum)
                return pattern;
        }

        return null;
    }

    private void DecideNextAction()
    {
        nextPattern = GetAvailablePattern();

        if (nextPattern != null)
        {
            currentAttackThreshold = nextPattern.requiredChargeTime;
        }
        else
        {
            currentAttackThreshold = 1.0f;
        }

        if (combatUI != null)
        {
            Sprite intentSprite = (nextPattern != null) ? nextPattern.patternIconSprite : null;
            combatUI.SetIntentIcon(intentSprite);
        }
    }

    public void UpdatePatternUI(float progress)
    {
        RequestActionBarUpdate(progress);
    }

    public override void OnDead() 
    {
        currentRunningPattern = null;

        var col = GetComponent<Collider2D>();
        if (col != null)
            col.enabled = false;

        spriteRenderer.transform.DOKill(true);

        if (unitData.unitDeadSprite != null)
            spriteRenderer.sprite = unitData.unitDeadSprite;

        //아군 사망 이벤트
        var allies = GameManager.Battle.GetAliveEnemies();
        foreach (var ally in allies)
        {
            if (ally != this && !ally.IsDead)
            {
                CombatEventContext ctx = new CombatEventContext(this, ally, 0f);
                ally.TriggerAbility(CombatEvent.OnAllyDead, ctx);
            }
        }

        spriteRenderer.DOFade(0f, 1.0f).OnComplete(() =>
        {
            OnUnitDead?.Invoke(this);
        });
    }

    public override float TakeDamage(CombatEventContext ctx)
    {
        if (IsDead)
            return 0f;

        TriggerAbility(CombatEvent.OnBeforeTakeDamage, ctx);
        if (ctx.value <= 0)
            return 0;

        // 방어력 계산
        float finalDamage = ctx.value;
        if (ctx.damageType == DamageType.Normal)
            finalDamage = Mathf.Max(1, finalDamage - stats.defense.GetValue());

        // 데미지 적용
        stats.hp -= finalDamage;
        
        // 피격 후 이벤트
        ctx.value = finalDamage;
        TriggerAbility(CombatEvent.OnTakeDamage, ctx);

        //피격 이펙트
        hitEffect?.Flash();
        RequestDamageText(ctx);

        if (stats.hp <= 0)
        {
            stats.hp = 0;

            TriggerAbility(CombatEvent.OnBeforeDead, ctx);

            if (isDeathCanceled)
            {
                isDeathCanceled = false;
                return finalDamage;
            }

            OnDead();
            return finalDamage;
        }

        return finalDamage;
    }

    public override T GetStat<T>() => stats as T;

    protected override void PlayHitVFX(Vector3 targetPos)
    {
        if (unitData != null)
        {
            GameManager.VFX.ShowGenericEffect(
                targetPos,
                unitData.attackVFXType,
                0f,
                Color.white
            );
        }
    }

    public void SetAttackDelay(float delay)
    {
        attackTimer = -delay;
    }

    public void CancelCurrentAction()
    {
        currentRunningPattern = null;
        isAttacking = false;
        attackTimer = 0f;
        transform.DOKill(true);
        spriteRenderer.transform.DOKill(true);
    }
}
