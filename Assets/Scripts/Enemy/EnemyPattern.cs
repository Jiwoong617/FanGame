using UnityEngine;
using System.Collections.Generic;

public enum PatternIcon
{
    Attack,
    AttackBuff,
    AttackDebuff,
    AttackHeal,
    Buff,
    Debuff,
    Heal,
    Combo2,
    Combo3,
    Combo4,
    Combo5,
    Combo6,
    Summon,
    Mimic,
}

[System.Serializable]
public abstract class EnemyPattern
{
    public string patternName = "Pattern";
    public float cooldown = 5.0f;
    public float requiredChargeTime = 1.0f;
    public PatternIcon patternIcon = PatternIcon.Attack;

    [Range(0, 100)] public int triggerChance = 30;
    [HideInInspector] public float lastExecutionTime = -9999f;

    [Header("Conditions")]
    [SerializeReference, SerializeReferenceDropdown]
    public List<PatternCondition> conditions = new List<PatternCondition>();

    [Header("Animation Settings")]
    [Tooltip("패턴 실행 시 덮어씌울 커스텀 스프라이트 (비워두면 기본 공격 스프라이트 사용)")]
    public Sprite actionSprite = null;

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