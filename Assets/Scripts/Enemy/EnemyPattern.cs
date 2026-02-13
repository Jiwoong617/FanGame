using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public abstract class EnemyPattern
{
    public string patternName = "Pattern";
    public float cooldown = 5.0f;
    public Sprite patternSprite;

    [Range(0, 100)] public int triggerChance = 30;
    [HideInInspector] public float lastExecutionTime = -9999f;

    public virtual void OnEnter(EnemyUnit unit) { }
    
    // true 반환 시 패턴 종료
    public abstract bool OnUpdate(EnemyUnit unit, float delta);
    
    public virtual void OnExit(EnemyUnit unit) { }
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

        ComboStep step = comboSteps[currentStepIndex];
        float speed = stats.attackSpeed.GetValue();
        if (speed <= 0) speed = 0.001f; // 0 나누기 방지 및 진행 멈춤 방지

        
        currentTimer += delta * speed;
        float requiredGauge = step.delayBeforeAttack;
        float progress = (requiredGauge > 0) ? Mathf.Clamp01(currentTimer / requiredGauge) : 1f;
        unit.UpdatePatternUI(progress);

        if (currentTimer >= requiredGauge)
        {
            float finalDamage = stats.attackDamage.GetValue() * step.damagePercent;
            float isTargetHit = target.TakeDamage(unit, finalDamage);
            if (isTargetHit > 0)
            {
                unit.TriggerAbility(CombatEvent.OnAttack, finalDamage);
            }

            currentStepIndex++;
            currentTimer = 0f;

            if (currentStepIndex >= comboSteps.Count)
                return true;
        }

        return false;
    }
}