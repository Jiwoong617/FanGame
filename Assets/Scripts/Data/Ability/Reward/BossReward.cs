using UnityEngine;

[System.Serializable]
public class Boss2 : Ability
{
    public override void OnEvent(CombatEvent eventType, CombatEventContext ctx)
    {
        if (eventType == CombatEvent.OnBattleEnd)
        {
            var stats = owner.GetStat<UnitStats>();
            stats.maxHp.SetBaseValue(stats.maxHp.GetValue() + 5f);
        }
    }
}

[System.Serializable]
public class Boss3 : Ability
{
    public override void OnEvent(CombatEvent eventType, CombatEventContext ctx)
    {
        if (eventType == CombatEvent.OnBattleEnd)
        {
            var stats = owner.GetStat<UnitStats>();
            stats.attackDamage.SetBaseValue(stats.attackDamage.GetValue() + 1f);
        }
    }
}

[System.Serializable]
public class Boss4 : Ability
{
    public override void OnEvent(CombatEvent eventType, CombatEventContext ctx)
    {
        if (eventType == CombatEvent.OnBattleEnd)
        {
            var stats = owner.GetStat<UnitStats>();
            stats.attackSpeed.SetBaseValue(stats.attackSpeed.GetValue() + 0.1f);
        }
    }
}

[System.Serializable]
public class Boss5 : Ability
{
    public override void OnEvent(CombatEvent eventType, CombatEventContext ctx)
    {
        if (eventType == CombatEvent.OnBattleEnd)
        {
            var pStats = owner.GetStat<PlayerStats>();
            if (pStats != null)
            {
                pStats.maxStamina.SetBaseValue(pStats.maxStamina.GetValue() + 10f);
            }
        }
    }
}

[System.Serializable]
public class Boss6 : Ability
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
public class Boss9 : Ability
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
public class Boss10 : Ability
{
    public override void OnEvent(CombatEvent eventType, CombatEventContext ctx)
    {
        if (eventType == CombatEvent.OnTakeDamage)
        {
            var stats = owner.GetStat<UnitStats>();
            if (stats.hp <= 0 && !IsFinished)
            {
                //TODO : 부활 사운드나 이펙트

                float maxHp = stats.maxHp.GetValue();
                stats.hp = maxHp;

                GameManager.VFX.ShowHealText(owner.transform, maxHp);
                MakeFinish();

                if (GameManager.Instance != null && GameManager.Inventory != null)
                    GameManager.Inventory.RemoveArtifact<Boss10>();
            }
        }
    }
}