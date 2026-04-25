using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class DragonSleepPassive : PassiveAbility
{
    [Tooltip("잠에서 깰 때 플레이어에게 부여할 디버프 목록")]
    [SerializeReference, SerializeReferenceDropdown]
    public List<StatusEffect> wakeUpDebuffs = new List<StatusEffect>();

    public override void OnEvent(CombatEvent eventType, CombatEventContext ctx)
    {
        if (eventType == CombatEvent.OnTakeDamage)
        {
            DragonUnit dragon = owner as DragonUnit;
            if (dragon != null && dragon.IsSleeping)
            {
                var stats = dragon.GetStat<UnitStats>();
                float currentHp = stats.hp;
                float maxHp = stats.maxHp.GetValue();

                if (currentHp <= maxHp * 0.7f)
                {
                    dragon.WakeUp(wakeUpDebuffs);
                    passiveDescription = "이 드래곤은 깨어났습니다.";
                }
            }
        }
    }
}