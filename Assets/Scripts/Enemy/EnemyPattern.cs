using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public abstract class EnemyPattern
{
    public string patternName = "Pattern";
    public float cooldown = 5.0f;
    public float requiredChargeTime = 1.0f;
    public Sprite patternIconSprite;

    [Range(0, 100)] public int triggerChance = 30;
    [HideInInspector] public float lastExecutionTime = -9999f;

    public virtual void OnEnter(EnemyUnit unit) { }
    
    // true 반환 시 패턴 종료
    public abstract bool OnUpdate(EnemyUnit unit, float delta);
    
    public virtual void OnExit(EnemyUnit unit) { }

    public virtual bool CanExecute(EnemyUnit unit) { return true; }
}


[System.Serializable]
public class AttackPattern : EnemyPattern
{
    [Header("Attack Settings")]
    [Tooltip("공격력의 몇 %로 때릴지 (1.0 = 100%)")]
    public float damagePercent = 1.0f;

    [Tooltip("일반 데미지/고정 데미지")]
    public DamageType damageType = DamageType.Normal;

    [Tooltip("직접 가서 때릴지(True), 제자리 공격할지(False)")]
    public bool useMoveAnim = true;

    public override bool OnUpdate(EnemyUnit unit, float delta)
    {
        var target = unit.GetTarget();
        if (target == null || target.IsDead) return true;
        if (unit.IsAttacking) return false;

        var stats = unit.GetStat<UnitStats>();
        float damage = stats.attackDamage.GetValue() * damagePercent;

        unit.Attack(target, damage, true, true, null, useMoveAnim, damageType);

        return true;
    }
}


[System.Serializable]
public class SequentialAttackPattern : EnemyPattern
{
    [System.Serializable]
    public struct ComboStep
    {
        public float damagePercent;
        public float delayBeforeAttack;
    }

    public bool onHit = true;
    public bool onCritical = true;
    public DamageType damageType = DamageType.Normal;

    public List<ComboStep> comboSteps = new List<ComboStep>();

    private int currentStepIndex = 0;
    private float currentTimer = 0f;
    private UnitStats stats;

    public override void OnEnter(EnemyUnit unit)
    {
        currentStepIndex = 0;
        currentTimer = 0f;
        stats = unit.GetStat<UnitStats>();
    }

    public override bool OnUpdate(EnemyUnit unit, float delta)
    {
        if (currentStepIndex >= comboSteps.Count) return true;

        var target = unit.GetTarget();
        if (target == null || target.IsDead) return true;
        if (unit.IsAttacking) return false;

        ComboStep step = comboSteps[currentStepIndex];
        float speed = stats.attackSpeed.GetValue();
        if (speed <= 0)
            return false;

        
        currentTimer += delta * speed;
        float requiredGauge = step.delayBeforeAttack;
        float progress = (requiredGauge > 0) ? Mathf.Clamp01(currentTimer / requiredGauge) : 1f;
        unit.UpdatePatternUI(progress);

        if (currentTimer >= requiredGauge)
        {
            float damageAmount = stats.attackDamage.GetValue() * step.damagePercent;
            unit.Attack(target, damageAmount, onHit, onCritical, null, true, damageType);

            currentStepIndex++;
            currentTimer = 0f;

            if (currentStepIndex >= comboSteps.Count)
                return true;
        }

        return false;
    }
}