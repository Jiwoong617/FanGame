using UnityEngine;

[System.Serializable]
public class IronFortressEffect : StatusEffect
{
    public IronFortressEffect()
    {
        effectType = EffectType.IronFortress;
        combatEvent = CombatEvent.OnBeforeTakeDamage;
    }

    public IronFortressEffect(int duration, int stack, bool isPermanent) 
    {
        effectType = EffectType.IronFortress;
        combatEvent = CombatEvent.OnBeforeTakeDamage;
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
                GameManager.VFX.PlayEffect(ctx.source.transform.position, owner.transform.position, AttackVFXType.IronFortress, 0f, Color.white);

                ctx.value = 0;
                if (ctx.debuffs != null && ctx.debuffs.Count > 0)
                    ctx.debuffs = null;

                stacks--;
                owner.UpdateBuffUI(this);

                if (stacks <= 0)
                    IsFinished = true;

                GameManager.Sound.PlaySFX(SFX.IronFortress);
            }
        }
    }
}
