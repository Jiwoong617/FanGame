using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public abstract class EnemyPattern
{
    public string patternName = "Pattern";
    public float cooldown = 5.0f;
    public Sprite patternIconSprite;

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

    public bool onHit = true;
    public bool onCritical = true;
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
        if (speed <= 0)
            return false;

        
        currentTimer += delta * speed;
        float requiredGauge = step.delayBeforeAttack;
        float progress = (requiredGauge > 0) ? Mathf.Clamp01(currentTimer / requiredGauge) : 1f;
        unit.UpdatePatternUI(progress);

        if (currentTimer >= requiredGauge)
        {
            float damageAmount = stats.attackDamage.GetValue() * step.damagePercent;
            bool isCrit = false;

            if (UnityEngine.Random.Range(0f, 100f) < stats.criticalChance.GetValue())
            {
                isCrit = true;
                damageAmount *= (stats.criticalDamage.GetValue() / 100f);
            }

            CombatEventContext ctx = new CombatEventContext(unit, target, damageAmount, DamageType.Normal, false, isCrit);
            float actualDamage = target.TakeDamage(ctx);

            if (actualDamage > 0 && !unit.IsDead)
            {
                ctx.value = actualDamage;
                unit.TriggerAbility(CombatEvent.OnAttack, ctx);
                if(isCrit && onCritical)
                    unit.TriggerAbility(CombatEvent.OnCritical, ctx);
            }

            currentStepIndex++;
            currentTimer = 0f;

            if (currentStepIndex >= comboSteps.Count)
                return true;
        }

        return false;
    }
}