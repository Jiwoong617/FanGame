using UnityEngine;

[System.Serializable]
public class ParryReflectAbility : Ability
{
    [Header("Reflect Settings")]
    [Tooltip("반사할 데미지의 배율 (1.0 = 100% 반사)")]
    [SerializeField] protected float reflectMultiplier = 1.0f;

    public override void OnEvent(CombatEvent eventType, CombatEventContext ctx)
    {
        //OnParrySuccess
        if (combatEvent == eventType && ctx.target != null)
        {
            float damageToReturn = ctx.value * reflectMultiplier;

            Debug.Log($"[ParryReflect] {ctx.target.name}에게 {damageToReturn}의 데미지 반사!");

            CombatEventContext reflectCtx = new CombatEventContext(owner, ctx.target, damageToReturn, DamageType.Fixed, true);
            ctx.target.TakeDamage(reflectCtx);
            
            // TODO : 여기서 뭐 온힛 관련 어빌리티 호출 할건지
        }
    }
}
