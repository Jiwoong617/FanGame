using System.Collections.Generic;
using UnityEngine;

public enum AbilityTargetType
{
    Self,
    CurrentTarget,
    AllEnemies,
    AllUnits
}

[System.Serializable]
public abstract class RewardAbility : Ability {}


[System.Serializable]
public abstract class BaseTriggerAbility : RewardAbility
{
    [Header("Stack Settings")]
    [SerializeField] protected bool useStack = false;
    [SerializeField] protected int requiredStack = 1;
    [SerializeField] protected bool resetStackOnTrigger = true; // 발동 시 스택 초기화 여부
    [SerializeField] protected bool resetStackOnBattleEnd = true; // 전투 종료 시 초기화 여부
    [SerializeField] protected AbilityTargetType targetType = AbilityTargetType.Self;


    protected int currentStack = 0;

    public override void Init(CombatUnit owner)
    {
        base.Init(owner);
        currentStack = 0;
    }

    public override void OnEvent(CombatEvent eventType, CombatEventContext context)
    {
        //전투 종료 시 스택 초기화
        if (eventType == CombatEvent.OnBattleEnd && resetStackOnBattleEnd)
            currentStack = 0;

        // 설정된 이벤트와 다르면 무시
        if (eventType != combatEvent) return;

        // 스택 체크
        if (useStack)
        {
            currentStack++;
            if (currentStack < requiredStack)
                return;
        }

        ExecuteEffect(context);

        if (useStack && resetStackOnTrigger)
            currentStack = 0;
    }

    protected abstract void ExecuteEffect(CombatEventContext context);
    protected List<CombatUnit> GetTargets(CombatEventContext context)
    {
        List<CombatUnit> targets = new List<CombatUnit>();

        switch (targetType)
        {
            case AbilityTargetType.Self:
                targets.Add(owner);
                break;

            case AbilityTargetType.CurrentTarget:
                if (owner.GetTarget() != null)
                    targets.Add(owner.GetTarget());
                else
                    targets.Add(context.target);
                break;

            case AbilityTargetType.AllEnemies:
                if (owner is PlayerUnit)
                    targets.AddRange(GameManager.Battle.GetAliveEnemies());
                else
                    targets.Add(GameManager.Instance.Player);
                break;

            case AbilityTargetType.AllUnits:
                targets.Add(GameManager.Instance.Player);
                targets.AddRange(GameManager.Battle.GetAliveEnemies());
                break;
        }

        return targets;
    }
}