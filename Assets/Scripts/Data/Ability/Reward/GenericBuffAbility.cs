using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GenericBuffAbility : BaseTriggerAbility
{
    [Header("Buff Settings")]
    [SerializeReference, SerializeReferenceDropdown]
    private StatusEffect buffToApply;

    protected override void ExecuteEffect(CombatEventContext context)
    {
        if (buffToApply == null) return;

        List<CombatUnit> targets = GetTargets(context);

        foreach (var target in targets)
        {
            if (target != null && !target.IsDead)
            {
                Ability newBuff = buffToApply.Clone();
                target.AddAbility(newBuff);
            }
        }
    }
}