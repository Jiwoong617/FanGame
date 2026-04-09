using UnityEngine;

[System.Serializable]
public class AttackTypePassive : PassiveAbility
{
    [Header("Attack Type Override")]
    [Tooltip("이 패시브를 가진 유닛의 모든 공격 타입을 강제로 변경합니다.")]
    public AttackEvadeType overrideEvadeType = AttackEvadeType.Both;

    public AttackTypePassive()
    {
        combatEvent = CombatEvent.OnBeforeAttack;
    }

    public override void OnEvent(CombatEvent eventType, CombatEventContext context)
    {
        if (eventType == combatEvent && context.source == owner && owner != null && !owner.IsDead)
        {
            context.evadeType = overrideEvadeType;

            // TODO : 공격타입에 따라 패리/회피 불가라고 세키로 처럼 이펙트 띄우기 같은거
        }
    }
}