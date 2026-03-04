using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GenericDamageAbility : BaseTriggerAbility
{
    [Header("Damage Settings")]
    [SerializeField] private float damageAmount;
    [SerializeField] private DamageType damageType = DamageType.Normal;

    protected override void ExecuteEffect(CombatEventContext context)
    {
        List<CombatUnit> targets = GetTargets(context);

        foreach (var target in targets)
        {
            if (target != null && !target.IsDead)
            {
                CombatEventContext damageCtx = new CombatEventContext(
                    owner, target, damageAmount, damageType
                );
                target.TakeDamage(damageCtx);
            }
        }
    }
}