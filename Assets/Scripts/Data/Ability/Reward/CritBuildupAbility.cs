using UnityEngine;


[System.Serializable]
public class CritBuildupAbility : RewardAbility
{
    [SerializeField] private float critChancePerStack = 5f; // 미적중 1회당 증가량

    private CritChanceEffect activeBuff;

    public override void OnEvent(CombatEvent eventType, CombatEventContext ctx)
    {
        if (eventType == CombatEvent.OnBattleEnd)
        {
            ResetBuff();
            return;
        }

        if (ctx.source != owner) return;

        if (eventType == CombatEvent.OnAttack && !ctx.isCritical)
        {
            if (activeBuff == null || activeBuff.IsFinished)
            {
                activeBuff = new CritChanceEffect(-1, 1, false, critChancePerStack);
                owner.AddAbility(activeBuff);
            }
            else
            {
                activeBuff.AddStack(1, -1);
                owner.UpdateBuffUI(activeBuff);
            }
        }
        else if (eventType == CombatEvent.OnCritical)
        {
            ResetBuff();
        }
    }

    public override void OnRemoved()
    {
        ResetBuff();
    }

    private void ResetBuff()
    {
        if (activeBuff != null && !activeBuff.IsFinished)
        {
            activeBuff.MakeFinish();
        }
        activeBuff = null;
    }
}
