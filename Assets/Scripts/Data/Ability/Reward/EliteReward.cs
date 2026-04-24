using UnityEngine;

[System.Serializable]
public class Elite8 : RewardAbility
{
    [Tooltip("곱연산 배율 (예: 1.2 = 공격력 1.2배)")]
    [SerializeField] private float multValue = 1.2f;

    private StatModifier activeMod;

    public override void OnEvent(CombatEvent eventType, CombatEventContext ctx)
    {
        if (eventType == CombatEvent.OnBattleStart)
        {
            RemoveActiveMod();
            AddActiveMod();
        }
        else if (eventType == CombatEvent.OnTakeDamage && ctx.value > 0)
        {
            RemoveActiveMod();
        }
        else if (eventType == CombatEvent.OnBattleEnd)
        {
            RemoveActiveMod();
        }
    }

    private void AddActiveMod()
    {
        var stats = owner.GetStat<UnitStats>();
        if (stats != null)
        {
            activeMod = new StatModifier(multValue, StatModType.PercentMult);
            stats.attackDamage.AddModifier(activeMod);
        }
    }

    private void RemoveActiveMod()
    {
        if (activeMod != null)
        {
            var stats = owner.GetStat<UnitStats>();
            stats?.attackDamage.RemoveModifier(activeMod);
            activeMod = null;
        }
    }
}


[System.Serializable]
public class Elite9 : RewardAbility
{
    [Header("Buff Settings")]
    [SerializeField] private AttackDamageEffect strongTemplate; // 2배 설정
    [SerializeField] private AttackDamageEffect weakTemplate;   // 0.7배 설정

    private AttackDamageEffect activeBuff;
    private bool isBroken = false;

    public override void OnEvent(CombatEvent eventType, CombatEventContext ctx)
    {
        if (strongTemplate == null || weakTemplate == null) return;

        if (eventType == CombatEvent.OnBattleStart)
        {
            isBroken = false;
            ChangeBuff(strongTemplate);
        }
        else if (eventType == CombatEvent.OnTakeDamage && ctx.value > 0)
        {
            if (!isBroken)
            {
                isBroken = true;
                ChangeBuff(weakTemplate);
            }
        }
        else if (eventType == CombatEvent.OnBattleEnd)
        {
            RemoveActiveBuff();
        }
    }

    private void ChangeBuff(AttackDamageEffect template)
    {
        RemoveActiveBuff();

        activeBuff = template.Clone() as AttackDamageEffect;
        owner.AddAbility(activeBuff);
    }

    private void RemoveActiveBuff()
    {
        if (activeBuff != null)
        {
            activeBuff.MakeFinish();
            activeBuff = null;
        }
    }
}


[System.Serializable]
public class Elite10 : RewardAbility
{
    [Header("Buff Setting")]
    [SerializeField] private AttackDamageEffect buffTemplate;

    private AttackDamageEffect activeBuff;

    public override void OnEvent(CombatEvent eventType, CombatEventContext ctx)
    {
        if (buffTemplate == null) return;

        if (eventType == CombatEvent.OnBattleStart)
        {
            activeBuff = null;
        }
        else if (eventType == CombatEvent.OnParrySuccess)
        {
            if (activeBuff == null || activeBuff.IsFinished)
            {
                activeBuff = buffTemplate.Clone() as AttackDamageEffect;
                owner.AddAbility(activeBuff);
            }
            else
            {
                var t = buffTemplate.Clone() as AttackDamageEffect;
                owner.AddAbility(t);
            }
        }
        else if ((eventType == CombatEvent.OnTakeDamage && ctx.value > 0) || eventType == CombatEvent.OnBattleEnd)
        {
            if (activeBuff != null)
            {
                activeBuff.MakeFinish();
                activeBuff = null;
            }
        }
    }
}

[System.Serializable]
public class Elite14 : RewardAbility
{
    public override void OnEvent(CombatEvent eventType, CombatEventContext ctx)
    {
        if (eventType == CombatEvent.OnBattleStart)
        {
            if (owner is PlayerUnit playerUnit)
            {
                if(GameManager.Instance.CurrentBattleType == NodeType.Elite ||
                    GameManager.Instance.CurrentBattleType == NodeType.Boss)
                    playerUnit.UseFreeSkill();
            }
        }
    }
}