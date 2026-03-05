using UnityEngine;

[System.Serializable]
public class Elite8 : Ability
{
    [SerializeField] private AttackDamageEffect buffTemplate;

    private AttackDamageEffect activeBuff;

    public override void OnEvent(CombatEvent eventType, CombatEventContext ctx)
    {
        if (buffTemplate == null) return;

        if (eventType == CombatEvent.OnBattleStart)
        {
            RemoveActiveBuff();

            activeBuff = buffTemplate.Clone() as AttackDamageEffect;
            owner.AddAbility(activeBuff);
        }
        else if (eventType == CombatEvent.OnTakeDamage && ctx.value > 0)
        {
            if (activeBuff != null && !activeBuff.IsFinished)
            {
                RemoveActiveBuff();
            }
        }
        else if (eventType == CombatEvent.OnBattleEnd)
        {
            RemoveActiveBuff();
        }
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
public class Elite9 : Ability
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
public class Elite10 : Ability
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
            if (activeBuff == null)
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
public class Elite14 : Ability
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