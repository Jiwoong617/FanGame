using UnityEngine;

[System.Serializable]
public abstract class PatternCondition
{
    // 조건 검사 함수
    public abstract bool IsMet(EnemyUnit unit, EnemyPattern pattern);

    // 패턴이 실제 실행되었을 때 호출할 함수 (1회용 체크 등에 사용)
    public virtual void OnExecute(EnemyUnit unit, EnemyPattern pattern) { }

    // 맵이 넘어가거나 다시 전투할 때 초기화할 함수
    public virtual void ResetCondition() { }
}

// 체력 조건
[System.Serializable]
public class HpCondition : PatternCondition
{
    [Tooltip("체력 비율 (0.5 = 50%)")]
    [Range(0f, 1f)] public float thresholdRatio = 0.5f;
    [Tooltip("True: 이하일 때 발동 / False: 이상일 때 발동")]
    public bool belowThreshold = true;

    public override bool IsMet(EnemyUnit unit, EnemyPattern pattern)
    {
        var stats = unit.GetStat<UnitStats>();
        float currentHpRatio = stats.hp / stats.maxHp.GetValue();
        return belowThreshold ? (currentHpRatio <= thresholdRatio) : (currentHpRatio >= thresholdRatio);
    }
}

// 횟수 제한 조건
[System.Serializable]
public class ExecutionCountCondition : PatternCondition
{
    [Tooltip("최대 실행 가능 횟수")]
    public int maxExecutions = 1;
    private int currentExecutions = 0;

    public override bool IsMet(EnemyUnit unit, EnemyPattern pattern)
    {
        return currentExecutions < maxExecutions;
    }

    public override void OnExecute(EnemyUnit unit, EnemyPattern pattern)
    {
        currentExecutions++;
    }

    public override void ResetCondition()
    {
        currentExecutions = 0; // 전투 시작 시 초기화
    }
}

[System.Serializable]
public class AbsoluteHpCondition : PatternCondition
{
    [Tooltip("기준 체력 수치")]
    public float thresholdHp = 31f;
    [Tooltip("True: 이하일 때 발동 / False: 이상일 때 발동")]
    public bool belowThreshold = false;

    public override bool IsMet(EnemyUnit unit, EnemyPattern pattern)
    {
        var stats = unit.GetStat<UnitStats>();
        return belowThreshold ? (stats.hp <= thresholdHp) : (stats.hp >= thresholdHp);
    }
}

[System.Serializable]
public class AllyCountCondition : PatternCondition
{
    [Tooltip("발동 가능한 최대 아군 수 (본인 포함)")]
    public int maxCount = 3;

    public override bool IsMet(EnemyUnit unit, EnemyPattern pattern)
    {
        return GameManager.Battle.GetAliveEnemies().Count <= maxCount;
    }
}