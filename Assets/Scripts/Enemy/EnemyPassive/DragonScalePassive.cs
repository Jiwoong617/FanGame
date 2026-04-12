using UnityEngine;

[System.Serializable]
public class DragonScalePassive : PassiveAbility
{
    private int hitCount = 0;

    [Tooltip("5회 피격 시 부여할 무효화(보호막) 어빌리티")]
    [SerializeReference, SerializeReferenceDropdown]
    public StatusEffect buff;

    public override void OnEvent(CombatEvent eventType, CombatEventContext ctx)
    {
        if (eventType == CombatEvent.OnTakeDamage)
        {
            if (ctx.value > 0)
            {
                hitCount++;
                if (hitCount >= 5)
                {
                    hitCount = 0;
                    if (buff != null)
                        owner.AddAbility(buff.Clone());
                }
            }
        }
    }
}