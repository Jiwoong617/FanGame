using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TriggerEventPassive : PassiveAbility
{
    [Header("Heal Settings")]
    public bool useHeal = true;
    [Tooltip("체크하면 최대 체력 비례 퍼센트 회복 (예: 10 = 10%)\n체크 해제하면 고정 수치 회복")]
    public bool isPercentHeal = false;
    public float healAmount = 5f;

    [Header("Buff/Debuff Settings")]
    [Tooltip("발동 시 자신에게 부여할 상태이상(버프) 리스트")]
    [SerializeReference, SerializeReferenceDropdown]
    public List<StatusEffect> grantEffects = new List<StatusEffect>();

    public override void OnEvent(CombatEvent eventType, CombatEventContext context)
    {
        if (combatEvent == eventType && owner != null && !owner.IsDead)
        {
            ApplyHeal();
            ApplyEffects();
        }
    }

    private void ApplyHeal()
    {
        if (!useHeal || healAmount <= 0) return;

        float finalHeal = healAmount;

        if (isPercentHeal)
        {
            var stats = owner.GetStat<UnitStats>();
            if (stats != null)
                finalHeal = stats.maxHp.GetValue() * (healAmount / 100f);
        }

        owner.Heal(finalHeal);
    }

    private void ApplyEffects()
    {
        if (grantEffects == null || grantEffects.Count == 0) return;

        foreach (var effectTemplate in grantEffects)
        {
            if (effectTemplate != null)
            {
                owner.AddAbility(effectTemplate.Clone());
            }
        }
    }
}