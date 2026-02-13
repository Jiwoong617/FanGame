using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyUnit : CombatUnit
{
    [SerializeField] private EnemyUI enemyUI;
    [SerializeField] private Sprite basicAttackSprite;

    [Header("Pattern Settings")]
    [SerializeReference, SerializeReferenceDropdown]
    public List<EnemyPattern> patterns = new List<EnemyPattern>();
    
    private EnemyPattern currentRunningPattern = null;
    private EnemyPattern nextPattern = null;
    private EnemyPattern lastExecutedPattern = null;

    public override void Init(UnitData unitData)
    {
        stats = new UnitStats(unitData);

        foreach (var pattern in patterns)
            pattern.lastExecutionTime = -9999f;

        stats.OnHpChanged += enemyUI.UpdateHp;
        enemyUI.UpdateHp(stats.hp, stats.maxHp.GetValue());

        attackTimer = 0f;
        // 초기 랜덤 딜레이 (선택 사항)
        // attackTimer = -Random.Range(0f, 0.5f);

        InitializeAbilities(unitData);

        //이거 주석 풀면 시작 시 기본 공격말고 패턴 선택함
        //DecideNextAction();
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

        attackTimer += delta * stats.attackSpeed.GetValue();
        if (enemyUI != null)
        {
            float progress = Mathf.Clamp01(attackTimer / ATTACK_THRESHOLD);
            enemyUI.UpdateActionBar(progress);
        }

        if (attackTimer >= ATTACK_THRESHOLD)
        {
            attackTimer = 0f;
            Attack();
        }
    }

    public override void Attack()
    {
        if (nextPattern != null)
        {
            currentRunningPattern = nextPattern;
            lastExecutedPattern = nextPattern;
            
            currentRunningPattern.OnEnter(this);
        }
        else
        {
            base.Attack();
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
            if (now < pattern.lastExecutionTime + pattern.cooldown) continue;

            if (patterns.Count > 1 && pattern == lastExecutedPattern) continue;

            validCandidates.Add(pattern);
            totalWeight += pattern.triggerChance;
        }

        if (validCandidates.Count == 0)
            return null;

        int randomPoint = Random.Range(0, totalWeight);
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

        if (enemyUI != null)
        {
            Sprite intentSprite = (nextPattern != null) ? nextPattern.patternSprite : basicAttackSprite;
            enemyUI.SetIntentIcon(intentSprite);
        }
    }

    public void UpdatePatternUI(float progress)
    {
        if (enemyUI != null)
            enemyUI.UpdateActionBar(progress);
    }

    public override void OnDead() 
    { 
        currentRunningPattern = null; 
    }

    public override float TakeDamage(CombatUnit attacker, float damage)
    {
        float finalDamage = Mathf.Max(1, damage - stats.defense.GetValue());
        stats.hp -= finalDamage;

        if (stats.hp <= 0)
        {
            stats.hp = 0;
            OnUnitDead?.Invoke(this);
            OnDead();
        }

        TriggerAbility(CombatEvent.OnTakeDamage, damage);
        return damage;
    }

    public override T GetStat<T>() => stats as T;
}
