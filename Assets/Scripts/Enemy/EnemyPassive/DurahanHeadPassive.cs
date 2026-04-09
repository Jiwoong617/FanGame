using UnityEngine;

[System.Serializable]
public class DurahanHeadPassive : PassiveAbility
{
    [Header("Curse Settings")]
    [Tooltip("해제할 본체의 무적 버프 타입")]
    public EffectType buffToRemove = EffectType.DamageReduction;

    [Tooltip("머리 파괴 시 본체에 줄 고정 피해량")]
    public float damageToBoss = 30f;

    public DurahanHeadPassive()
    {
        combatEvent = CombatEvent.OnBeforeDead;
    }

    public override void OnEvent(CombatEvent eventType, CombatEventContext context)
    {
        if (eventType == combatEvent && owner != null)
        {
            ExecuteCurse();
        }
    }

    private void ExecuteCurse()
    {
        var enemies = GameManager.Battle.GetAliveEnemies();

        foreach (var enemy in enemies)
        {
            if (enemy == owner) continue;

            StatusEffect targetBuff = enemy.GetStatusEffect(buffToRemove);

            if (targetBuff != null)
            {
                targetBuff.MakeFinish();
                CombatEventContext damageCtx = new CombatEventContext(
                    owner,
                    enemy,
                    damageToBoss,
                    DamageType.Fixed,
                    true,
                    false,
                    null
                );

                enemy.TakeDamage(damageCtx);
                break;
            }
        }
    }
}