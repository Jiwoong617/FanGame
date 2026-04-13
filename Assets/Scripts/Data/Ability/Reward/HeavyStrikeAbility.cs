using UnityEngine;

[System.Serializable]
public class HeavyStrikeAbility : RewardAbility
{
    [SerializeField] private float damageThreshold = 30f;
    [SerializeField] private float amplifyValue = 0.05f; // 5%

    public override void OnEvent(CombatEvent eventType, CombatEventContext ctx)
    {
        if (eventType != CombatEvent.OnAttack) return;
        if (ctx.source != owner) return;
        if (ctx.value < damageThreshold) return;

        CombatUnit target = ctx.target;
        if (target == null || target.IsDead) return;

        var debuff = new DamageAmplificationEffect(-1, false, amplifyValue, 1);
        target.AddAbility(debuff);
    }
}
