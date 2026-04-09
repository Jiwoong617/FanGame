using UnityEngine;

[System.Serializable]
public class EndurePassive : PassiveAbility
{
    [Header("Endure Settings")]
    [Tooltip("최대 버티기 발동 횟수 (보통 1회)")]
    public int maxUses = 1;

    // 내부 카운터
    private int currentUses = 0;

    public EndurePassive()
    {
        combatEvent = CombatEvent.OnBeforeDead;
    }

    public override void OnEvent(CombatEvent eventType, CombatEventContext context)
    {
        // 내 트리거가 맞고, 아직 발동 횟수가 남아있다면
        if (eventType == combatEvent && owner != null)
        {
            if (currentUses < maxUses)
            {
                currentUses++;
                ExecuteEndure();
            }
        }
    }

    private void ExecuteEndure()
    {
        owner.CancelDeath();
        owner.GetStat<UnitStats>().hp = 1f;

        //TODO : 버티기 이펙트
    }
}