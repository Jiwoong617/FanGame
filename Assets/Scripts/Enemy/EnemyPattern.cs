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

    [Header("Conditions")]
    [SerializeReference, SerializeReferenceDropdown]
    public List<PatternCondition> conditions = new List<PatternCondition>();

    public void ResetPattern()
    {
        lastExecutionTime = -9999f;
        if (conditions != null && conditions.Count > 0)
        {
            foreach (var cond in conditions)
                cond.ResetCondition();
        }
    }

    public void UpdateConditionOnExecute(EnemyUnit unit)
    {
        if (conditions != null)
        {
            foreach (var cond in conditions)
                cond.OnExecute(unit, this);
        }
    }

    public virtual void OnEnter(EnemyUnit unit) { }
    
    // true 반환 시 패턴 종료
    public abstract bool OnUpdate(EnemyUnit unit, float delta);
    
    public virtual void OnExit(EnemyUnit unit) { }

    public virtual bool CanExecute(EnemyUnit unit)
    {
        if (conditions != null)
        {
            foreach (var condition in conditions)
            {
                if (!condition.IsMet(unit, this))
                    return false;
            }
        }
        return true;
    }
}