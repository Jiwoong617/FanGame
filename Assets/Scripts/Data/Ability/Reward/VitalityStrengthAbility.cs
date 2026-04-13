using UnityEngine;

[System.Serializable]
public class VitalityStrengthAbility : RewardAbility
{
    [SerializeField, Range(0.01f, 1f)] private float hpRatio = 0.1f; // 최대 체력의 몇 %

    private StatModifier currentMod;

    public override void OnEvent(CombatEvent eventType, CombatEventContext ctx)
    {
        if (eventType == CombatEvent.OnBattleStart)
        {
            RemoveModifier();

            var stats = owner.GetStat<UnitStats>();
            if (stats == null) return;

            float bonusDamage = stats.maxHp.GetValue() * hpRatio;
            currentMod = new StatModifier(bonusDamage, StatModType.Flat);
            stats.attackDamage.AddModifier(currentMod);
        }
        else if (eventType == CombatEvent.OnBattleEnd)
        {
            RemoveModifier();
        }
    }

    public override void OnRemoved()
    {
        RemoveModifier();
    }

    private void RemoveModifier()
    {
        if (currentMod != null)
        {
            owner.GetStat<UnitStats>()?.attackDamage.RemoveModifier(currentMod);
            currentMod = null;
        }
    }
}
