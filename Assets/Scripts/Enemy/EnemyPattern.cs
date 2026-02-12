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

    public abstract IEnumerator Execute(EnemyUnit unit);
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

    public override IEnumerator Execute(EnemyUnit unit)
    {
        var target = unit.GetTarget();
        var stats = unit.GetStat<UnitStats>();

        foreach (var step in comboSteps)
        {
            if (target == null || target.IsDead) break;

            if (step.delayBeforeAttack > 0)
            {
                float duration = step.delayBeforeAttack;
                yield return unit.StartCoroutine(unit.WaitAndUpdateUI(duration));
            }

            float finalDamage = stats.attackDamage.GetValue() * step.damagePercent;
            target.TakeDamage(unit, finalDamage);
        }
    }
}