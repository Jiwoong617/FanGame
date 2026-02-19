public class ReflectEffect : StatusEffect
{
    public float reflectRatePerStack = 0.05f;

    public ReflectEffect()
    {
        effectType = EffectType.Reflect;
        combatEvent = CombatEvent.OnTakeDamage;
    }

    public ReflectEffect(float duration, int stack, bool isPermanent)
    {
        effectType = EffectType.Reflect;
        combatEvent = CombatEvent.OnTakeDamage;
        this.duration = duration;
        stacks = stack;
        this.isPermanent = isPermanent;
    }

    public override void OnEvent(CombatEvent eventType, CombatEventContext ctx)
    {
        //OnTakeDamage 이벤트
        if (eventType == combatEvent && ctx.target == owner)
        {
            // 데미지가 없거나, 이미 반사된 데미지(무한루프 방지)면 무시
            if (ctx.value <= 0 || ctx.isReflectDamage)
                return;

            float reflectAmount = ctx.value * (reflectRatePerStack * stacks);
            // 반사 데미지는 고정뎀/ 회피 패리 불가로
            CombatEventContext reflectCtx = new CombatEventContext(owner, ctx.source, reflectAmount, DamageType.Fixed, true);
            ctx.source.TakeDamage(reflectCtx);
        }
    }
}