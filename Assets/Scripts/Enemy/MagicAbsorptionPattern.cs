using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class MagicAbsorptionPattern : EnemyPattern
{
    public int healAmount = 20;

    [Tooltip("시전하는 모션 유지 시간")]
    public float castDuration = 1.0f;

    public override bool OnUpdate(EnemyUnit unit, float delta)
    {
        if (unit.IsAttacking) return false;

        unit.PlayActionAnimation(actionSprite, castDuration, () =>
        {
            PlayerUnit player = GameManager.Instance.Player;
            if (player == null || player.IsDead) return;

            List<StatusEffect> playerBuffs = player.GetCurrentStatusEffects()
                .Where(effect => IsBuff(effect.effectType))
                .ToList();

            if (playerBuffs.Count > 0)
            {
                StatusEffect targetBuff = playerBuffs[Random.Range(0, playerBuffs.Count)];
                unit.AddAbility(targetBuff.Clone());

                targetBuff.MakeFinish();

                GameManager.VFX.ShowText(unit.transform, "마법 흡수", Color.magenta);
            }
            else
            {
                unit.Heal(healAmount);
            }
        });

        GameManager.Sound.PlaySFX(SFX.DragonMagicSteal);
        return true;
    }

    private bool IsBuff(EffectType type)
    {
        return type == EffectType.IronFortress ||
               type == EffectType.Vampire ||
               type == EffectType.Reflect ||
               type == EffectType.Inspire ||
               type == EffectType.AttackUp ||
               type == EffectType.DamageReduction;
    }
}