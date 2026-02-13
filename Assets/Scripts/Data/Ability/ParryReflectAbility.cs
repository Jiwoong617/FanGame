using UnityEngine;

[System.Serializable]
public class ParryReflectAbility : Ability
{
    [Header("Reflect Settings")]
    [Tooltip("반사할 데미지의 배율 (1.0 = 100% 반사)")]
    [SerializeField] protected float reflectMultiplier = 1.0f;

    public override void OnEvent(CombatEvent eventType, CombatEventContext ctx)
    {
        if (combatEvent == eventType && ctx.target != null)
        {
            float damageToReturn = ctx.value * reflectMultiplier;

            Debug.Log($"[ParryReflect] {ctx.target.name}에게 {damageToReturn}의 데미지 반사!");

            ctx.target.TakeDamage(owner, damageToReturn);
        }
    }
}
