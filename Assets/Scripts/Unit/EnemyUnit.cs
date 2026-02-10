using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyUnit : CombatUnit
{
    private float targetTime = 1f;
    private bool isActing = false;

    [SerializeField] private EnemyUI enemyUI;
    [SerializeField] private Sprite basicAttackSprite;

    [Header("Pattern Settings")]
    [SerializeReference, SerializeReferenceDropdown]
    public List<EnemyPattern> patterns = new List<EnemyPattern>();
    private EnemyPattern nextPattern = null;
    private EnemyPattern lastExecutedPattern = null;

    public override void Init(UnitData unitData)
    {
        stats = new UnitStats(unitData);

        foreach (var pattern in patterns)
            pattern.lastExecutionTime = -9999f;

        stats.OnHpChanged += enemyUI.UpdateHp;
        enemyUI.UpdateHp(stats.hp, stats.maxHp);

        attackTimer = 0f;
        targetTime = stats.attackSpeed > 0 ? 1f / stats.attackSpeed : 1f;
        //같은몹 여러마리일 때 초기 딜레이 줄거면 이거 주석 해제
        //attackTimer = -Random.Range(0, targetTime * 0.5f);
    }

    public override void OnUpdate(float delta)
    {
        if (target == null || target.IsDead || IsDead) return;
        if (isActing) return;

        if (enemyUI != null)
        {
            float progress = GetActionProgress();
            enemyUI.UpdateActionBar(progress);
        }

        attackTimer += delta;
        if (attackTimer >= targetTime)
        {
            attackTimer = targetTime;
            Attack();
        }
    }

    public override void Attack()
    {
        if (nextPattern != null)
            StartCoroutine(ExecutePatternRoutine(nextPattern));
        else
            PerformBasicAttack();
    }

    private IEnumerator ExecutePatternRoutine(EnemyPattern pattern)
    {
        isActing = true;
        lastExecutedPattern = pattern;

        yield return StartCoroutine(pattern.Execute(this));

        pattern.lastExecutionTime = Time.time;

        isActing = false;
        ResetTimer();
    }

    private void PerformBasicAttack()
    {
        if (target != null)
            target.TakeDamage(stats.attackDamage);

        ResetTimer();
    }

    private void ResetTimer()
    {
        attackTimer = 0f;
        targetTime = stats.attackSpeed > 0 ? 1f / stats.attackSpeed : 1f;

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

    public IEnumerator WaitAndUpdateUI(float duration)
    {
        if (duration <= 0) yield break;
        attackTimer = 0f;
        targetTime = duration;
        while (attackTimer < targetTime)
        {
            attackTimer += Time.deltaTime;
            if (enemyUI != null)
                enemyUI.UpdateActionBar(GetActionProgress());

            yield return null;
        }

        attackTimer = targetTime;
        if (enemyUI != null)
            enemyUI.UpdateActionBar(1f);
    }


    public override void OnDead() { StopAllCoroutines(); isActing = false; }
    public override void TakeDamage(float damage)
    {
        stats.hp -= Mathf.Max(1, damage - stats.defense);
        if (stats.hp <= 0)
        {
            stats.hp = 0;
            OnUnitDead?.Invoke(this);
            OnDead();
        }
    }
    public override T GetStat<T>() => stats as T;

    public float GetActionProgress()
    {
        if (targetTime <= 0) return 0;
        return Mathf.Clamp01(attackTimer / targetTime);
    }
}