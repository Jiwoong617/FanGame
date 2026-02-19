using UnityEngine;

[System.Serializable]
public class IronFortressEffect : StatusEffect
{
    public IronFortressEffect()
    {
        effectType = EffectType.IronFortress;
        combatEvent = CombatEvent.OnTakeDamage;
    }

    public IronFortressEffect(int duration, int stack, bool isPermanent) 
    {
        effectType = EffectType.IronFortress;
        combatEvent = CombatEvent.OnTakeDamage;
        this.duration = duration;
        stacks = stack;
        this.isPermanent = isPermanent;
    }

    public override void OnEvent(CombatEvent eventType, CombatEventContext ctx)
    {
        //OnTakeDamage로 설정
        if (eventType == combatEvent && ctx.target == owner)
        {
            if (ctx.value > 0 && stacks > 0)
            {
                Debug.Log($"[철옹성] {owner.name} 데미지 무효화! (남은 횟수: {stacks - 1})");
                ctx.value = 0;
                stacks--;

                if (stacks <= 0)
                    IsFinished = true;
            }
        }
    }
}
