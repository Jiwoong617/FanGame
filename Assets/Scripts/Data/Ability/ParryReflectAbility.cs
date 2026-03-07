using UnityEngine;

[System.Serializable]
public class ParryReflectAbility : Ability
{
    public override void OnEvent(CombatEvent eventType, CombatEventContext ctx)
    {
        if (eventType == CombatEvent.OnParrySuccess)
        {
            CombatUnit attacker = ctx.source;
            if (attacker == null || attacker.IsDead)
                return;

            var stats = owner.GetStat<UnitStats>();
            if (stats == null)
                return;

            float dmg = stats.attackDamage.GetValue();
            bool isCritical = false;
            if (Random.Range(0f, 100f) < stats.criticalChance.GetValue())
            {
                isCritical = true;
                dmg *= (stats.criticalDamage.GetValue() / 100f);
            }

            CombatEventContext reflectCtx = new CombatEventContext(
                owner,
                attacker,
                dmg,
                DamageType.Normal,
                isReflectDamage: true,
                isCritical: isCritical
            );

            float actualDealt = attacker.TakeDamage(reflectCtx);
            if (actualDealt > 0)
            {
                reflectCtx.value = actualDealt;
                owner.TriggerAbility(CombatEvent.OnAttack, reflectCtx);

                if (isCritical)
                    owner.TriggerAbility(CombatEvent.OnCritical, reflectCtx);
            }
        }
    }
}
