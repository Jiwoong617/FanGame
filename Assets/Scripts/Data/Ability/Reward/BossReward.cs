using UnityEngine;
using DG.Tweening;

[System.Serializable]
public class Boss2 : RewardAbility
{
    public override void OnEvent(CombatEvent eventType, CombatEventContext ctx)
    {
        if (eventType == CombatEvent.OnBattleEnd)
        {
            var stats = owner.GetStat<UnitStats>();
            stats.maxHp.SetBaseValue(stats.maxHp.GetBaseValue() + 5f);
        }
    }
}

[System.Serializable]
public class Boss3 : RewardAbility
{
    public override void OnEvent(CombatEvent eventType, CombatEventContext ctx)
    {
        if (eventType == CombatEvent.OnBattleEnd)
        {
            var stats = owner.GetStat<UnitStats>();
            stats.attackDamage.SetBaseValue(stats.attackDamage.GetBaseValue() + 1f);
        }
    }
}

[System.Serializable]
public class Boss4 : RewardAbility
{
    public override void OnEvent(CombatEvent eventType, CombatEventContext ctx)
    {
        if (eventType == CombatEvent.OnBattleEnd)
        {
            var stats = owner.GetStat<UnitStats>();
            stats.attackSpeed.SetBaseValue(stats.attackSpeed.GetBaseValue() + 0.1f);
        }
    }
}

[System.Serializable]
public class Boss5 : RewardAbility
{
    public override void OnEvent(CombatEvent eventType, CombatEventContext ctx)
    {
        if (eventType == CombatEvent.OnBattleEnd)
        {
            var pStats = owner.GetStat<PlayerStats>();
            if (pStats != null)
            {
                pStats.maxStamina.SetBaseValue(pStats.maxStamina.GetBaseValue() + 10f);
            }
        }
    }
}

[System.Serializable]
public class Boss6 : RewardAbility
{
    private StatModifier damageMod;

    private UnitStats stats;

    protected override void OnAdded()
    {
        stats = owner.GetStat<UnitStats>();
    }

    public override void OnEvent(CombatEvent eventType, CombatEventContext ctx)
    {
        // 체력이 변할 수 있는 타이밍마다 갱신
        if (eventType == CombatEvent.OnBattleStart ||
            eventType == CombatEvent.OnTakeDamage ||
            (eventType == CombatEvent.OnAttack && ctx.source == owner) || // 흡혈
            eventType == CombatEvent.OnRest)
        {
            UpdateDamageBonus();
        }
    }

    public override void OnRemoved()
    {
        base.OnRemoved();
        if (stats != null && damageMod != null)
        {
            stats.attackDamage.RemoveModifier(damageMod);
            damageMod = null;
        }
    }

    private void UpdateDamageBonus()
    {
        if (stats == null) return;

        if (damageMod != null)
        {
            stats.attackDamage.RemoveModifier(damageMod);
            damageMod = null;
        }

        float maxHp = stats.maxHp.GetValue();
        float currentHp = stats.hp;
        if (maxHp <= 0) return;

        float lostRatio = 1f - (currentHp / maxHp);
        float bonusPercent = lostRatio * 0.05f;
        if (bonusPercent > 0)
        {
            damageMod = new StatModifier(bonusPercent, StatModType.PercentAdd);
            stats.attackDamage.AddModifier(damageMod);
        }
    }
}

[System.Serializable]
public class Boss9 : RewardAbility
{
    public override void OnEvent(CombatEvent eventType, CombatEventContext ctx)
    {
        if (eventType == CombatEvent.OnDodgeSuccess)
        {
            if (owner is PlayerUnit player)
            {
                player.UseFreeSkill();
            }
        }
    }
}


[System.Serializable]
public class Boss10 : RewardAbility
{
    public override void OnEvent(CombatEvent eventType, CombatEventContext ctx)
    {
        if (eventType == CombatEvent.OnTakeDamage)
        {
            var stats = owner.GetStat<UnitStats>();
            if (stats.hp <= 0 && !IsFinished)
            {
                MakeFinish();

                if (GameManager.Instance != null && GameManager.Inventory != null)
                    GameManager.Inventory.RemoveArtifact<Boss10>();

                float reviveDelay = 2f;
                DOVirtual.DelayedCall(reviveDelay, () =>
                {
                    if (GameManager.Instance != null)
                        GameManager.Instance.RevivePlayer();
                });
            }
        }
    }
}