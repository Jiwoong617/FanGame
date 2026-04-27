using System.Collections.Generic;
using UnityEngine;

// 회복 10마다 최대 체력 +1
[System.Serializable]
public class HealToMaxHpAbility : RewardAbility
{
    [SerializeField] private float healThreshold = 10f; // 최대체력 +1이 발동되는 누적 회복량

    private float accumulatedHeal = 0f;

    protected override void OnAdded()
    {
        accumulatedHeal = 0f;
    }

    public override void OnEvent(CombatEvent eventType, CombatEventContext ctx)
    {
        if (eventType != CombatEvent.OnHeal) return;
        if (ctx.source != owner) return;

        accumulatedHeal += ctx.value;

        int bonus = Mathf.FloorToInt(accumulatedHeal / healThreshold);
        if (bonus <= 0) return;

        accumulatedHeal -= bonus * healThreshold;

        var stats = owner.GetStat<UnitStats>();
        if (stats == null) return;

        stats.maxHp.SetBaseValue(stats.maxHp.GetBaseValue() + bonus);
    }
}

/// 전투 시작 후 1초마다 치명타 피해 +10%, 전투 종료 시 초기화
[System.Serializable]
public class CritDamageRampAbility : RewardAbility
{
    [SerializeField] private float interval = 1f;
    [SerializeField] private float bonusPerTick = 10f;

    private float timer = 0f;
    private bool isInBattle = false;
    private CritDamageEffect activeBuff;

    public override void OnEvent(CombatEvent eventType, CombatEventContext ctx)
    {
        if (eventType == CombatEvent.OnBattleStart)
        {
            isInBattle = true;
            timer = 0f;
        }
        else if (eventType == CombatEvent.OnBattleEnd)
        {
            isInBattle = false;
            timer = 0f;
            ResetBuff();
        }
    }

    public override void OnUpdate(float delta)
    {
        if (!isInBattle) return;

        timer += delta;
        if (timer < interval) return;

        timer -= interval;

        if (activeBuff == null || activeBuff.IsFinished)
        {
            activeBuff = new CritDamageEffect(-1, 1, false, bonusPerTick);
            owner.AddAbility(activeBuff);
        }
        else
        {
            activeBuff.AddStack(1, -1);
            owner.UpdateBuffUI(activeBuff);
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

/// 공격 시 10% 확률로 적에게 둔화 10% 적용
[System.Serializable]
public class OnAttackSlowAbility : RewardAbility
{
    [SerializeField] private float procChance = 0.1f;    // 발동 확률 (0~1)
    [SerializeField] private float slowValue = 0.1f;     // 둔화 비율 (0~1)
    [SerializeField] private float slowDuration = -1f;    // 둔화 지속 시간 (초)

    public override void OnEvent(CombatEvent eventType, CombatEventContext ctx)
    {
        if (eventType != CombatEvent.OnAttack) return;
        if (ctx.source != owner) return;
        if (ctx.target == null || ctx.target.IsDead) return;

        if (Random.value > procChance) return;  

        SlowEffect slow = new SlowEffect(slowDuration, 1, false, slowValue);
        ctx.target.AddAbility(slow);
    }
}

/// 전투 시작 시 공격력 BaseValue -1, 최대 체력 BaseValue +5 (공격력 1 이하면 스킵)
[System.Serializable]
public class TradeAttackForHpAbility : RewardAbility
{
    public override void OnEvent(CombatEvent eventType, CombatEventContext ctx)
    {
        if (eventType != CombatEvent.OnBattleStart) return;

        var stats = owner.GetStat<UnitStats>();
        if (stats == null) return;

        if (stats.attackDamage.GetBaseValue() <= 1f) return;

        stats.attackDamage.SetBaseValue(stats.attackDamage.GetBaseValue() - 1f);
        stats.maxHp.SetBaseValue(stats.maxHp.GetBaseValue() + 5f);
    }
}

/// 체력 50% 이상: 반사 20% 버프 / 50% 미만: 받는 피해 20% 감소 버프
/// HP가 변하는 모든 시점(전투시작, 피해, 회복, 휴식)마다 상태를 재평가
[System.Serializable]
public class HpThresholdBuffAbility : RewardAbility
{
    [SerializeField] private float threshold = 0.5f;      // 판정 비율 (기본 50%)
    [SerializeField] private float reflectValue = 0.2f;   // 반사 비율
    [SerializeField] private float reductionValue = 0.2f; // 피해 감소 비율

    private ReflectEffect reflectBuff;
    private DamageReductionEffect reductionBuff;

    private bool? currentState = null;

    public override void OnEvent(CombatEvent eventType, CombatEventContext ctx)
    {
        switch (eventType)
        {
            case CombatEvent.OnBattleStart:
            case CombatEvent.OnTakeDamage:
            case CombatEvent.OnHeal:
            case CombatEvent.OnRest:
                Evaluate();
                break;
            case CombatEvent.OnBattleEnd:
                ClearAll();
                currentState = null;
                break;
        }
    }

    public override void OnRemoved()
    {
        ClearAll();
    }

    private void Evaluate()
    {
        var stats = owner.GetStat<UnitStats>();
        if (stats == null) return;

        bool isHighHp = stats.hp / stats.maxHp.GetValue() >= threshold;

        if (currentState == isHighHp) return;

        currentState = isHighHp;
        ClearAll();

        if (isHighHp)
        {
            reflectBuff = new ReflectEffect(-1, 1, false, reflectValue);
            owner.AddAbility(reflectBuff);
        }
        else
        {
            reductionBuff = new DamageReductionEffect(-1, false, reductionValue, 1);
            owner.AddAbility(reductionBuff);
        }
    }

    private void ClearAll()
    {
        if (reflectBuff != null && !reflectBuff.IsFinished)
        {
            reflectBuff.MakeFinish();
        }
        reflectBuff = null;

        if (reductionBuff != null && !reductionBuff.IsFinished)
        {
            reductionBuff.MakeFinish();
        }
        reductionBuff = null;
    }
}

/// 피해 반사 시, 방어력의 50%를 추가 고정 데미지로 반사 대상에게 준다
[System.Serializable]
public class ReflectDefenseBonusAbility : RewardAbility
{
    [SerializeField] private float defenseRatio = 0.5f; // 방어력 중 추가 데미지로 줄 비율

    public override void OnEvent(CombatEvent eventType, CombatEventContext ctx)
    {
        if (eventType != CombatEvent.OnReflect) return;
        if (ctx.source != owner) return; // ctx.source = 반사한 유닛(나)
        if (ctx.target == null || ctx.target.IsDead) return;

        var stats = owner.GetStat<UnitStats>();
        if (stats == null) return;

        float bonusDamage = stats.defense.GetValue() * defenseRatio;
        if (bonusDamage <= 0) return;

        CombatEventContext bonusCtx = new CombatEventContext(owner, ctx.target, bonusDamage, DamageType.Fixed, true);
        ctx.target.TakeDamage(bonusCtx);
    }
}

/// 전투 시작 시 현재 체력 50% 상실, 흡혈 2스택 획득 (체력이 1 이하로 내려가면 1로 고정)
[System.Serializable]
public class BloodPactAbility : RewardAbility
{
    [SerializeField] private float hpLossRatio = 0.5f;       // 잃는 체력 비율
    [SerializeField] private float vampireValue = 0.05f;      // 흡혈 1스택당 비율
    [SerializeField] private int vampireStacks = 2;           // 획득 스택 수

    public override void OnEvent(CombatEvent eventType, CombatEventContext ctx)
    {
        if (eventType != CombatEvent.OnBattleStart) return;

        var stats = owner.GetStat<UnitStats>();
        if (stats == null) return;

        float loss = stats.hp * hpLossRatio;
        stats.hp = Mathf.Max(1f, stats.hp - loss);

        owner.AddAbility(new VampireEffect(-1, vampireStacks, false, vampireValue));
    }
}

/// 잃은 체력 5%마다 모든 회복량 3% 증가
[System.Serializable]
public class MissingHpHealBonusAbility : RewardAbility
{
    [SerializeField] private float missingHpStep = 0.05f;    // 몇 % 잃을 때마다 (기본 5%)
    [SerializeField] private float healBonusPerStep = 0.03f; // 스텝당 회복량 증가 비율 (기본 3%)

    public override void OnEvent(CombatEvent eventType, CombatEventContext ctx)
    {
        if (eventType != CombatEvent.OnBeforeHeal) return;
        if (ctx.source != owner) return;

        var stats = owner.GetStat<UnitStats>();
        if (stats == null) return;

        float maxHp = stats.maxHp.GetValue();
        if (maxHp <= 0) return;

        float missingRatio = 1f - (stats.hp / maxHp);
        int steps = Mathf.FloorToInt(missingRatio / missingHpStep);
        if (steps <= 0) return;

        ctx.value *= 1f + (steps * healBonusPerStep);
    }
}

/// 스태미나 누적 50 이상 소모 시마다 스태미나 재생 BaseValue +0.1
[System.Serializable]
public class StaminaRegenBonusAbility : RewardAbility
{
    [SerializeField] private float staminaThreshold = 50f; // 재생 증가가 발동되는 누적 소모량
    [SerializeField] private float regenBonus = 0.1f;      // 발동 시 증가량

    private float accumulatedStamina = 0f;

    protected override void OnAdded()
    {
        accumulatedStamina = 0f;
    }

    public override void OnEvent(CombatEvent eventType, CombatEventContext ctx)
    {
        if (eventType != CombatEvent.OnStaminaUsed) return;
        if (ctx.source != owner) return;

        accumulatedStamina += ctx.value;

        int bonus = Mathf.FloorToInt(accumulatedStamina / staminaThreshold);
        if (bonus <= 0) return;

        accumulatedStamina -= bonus * staminaThreshold;

        var pStats = owner.GetStat<PlayerStats>();
        if (pStats == null) return;

        pStats.staminaRegen.SetBaseValue(pStats.staminaRegen.GetBaseValue() + regenBonus * bonus);
    }
}

/// 공격 시 대상에게 둔화가 있으면 추가 고정 데미지
[System.Serializable]
public class SlowBonusDamageAbility : RewardAbility
{
    [SerializeField] private float bonusDamage = 3f;

    public override void OnEvent(CombatEvent eventType, CombatEventContext ctx)
    {
        if (eventType != CombatEvent.OnAttack) return;
        if (ctx.source != owner) return;
        if (ctx.target == null || ctx.target.IsDead) return;

        if (!ctx.target.HasStatusEffect(EffectType.Slow)) return;

        CombatEventContext bonusCtx = new CombatEventContext(owner, ctx.target, bonusDamage, DamageType.Fixed);
        ctx.target.TakeDamage(bonusCtx);
    }
}

/// LastStandAbility 보유 시 발동 - 발사된 바바
[System.Serializable]
public class LastStandSynergyAbility : RewardAbility
{
    [SerializeField] private float damage = 20f;
    [SerializeField] private float slowValue = 0.1f;
    [SerializeField] private float slowDuration = -1f;

    private float timer = 0f;
    private bool isActive = false;
    private bool hasSynergy = false; // null = 미검사, 이후 캐싱

    public override void OnEvent(CombatEvent eventType, CombatEventContext ctx)
    {
        if (eventType == CombatEvent.OnBattleStart)
        {
            isActive = false;
            if(hasSynergy == false)
                hasSynergy = GameManager.Inventory.HasAbility<LastStandAbility>();
            
            if (hasSynergy == true && Random.value < 0.7f)
            {
                timer = Random.Range(5f, 8f);
                isActive = true;
            }
            return;
        }

        if (eventType == CombatEvent.OnBattleEnd)
        {
            isActive = false;
        }
    }

    public override void OnUpdate(float delta)
    {
        if (!isActive) return;

        timer -= delta;
        if (timer > 0f) return;

        isActive = false;
        AbilityFunc();
    }

    private void AbilityFunc()
    {
        GameManager.Sound.PlaySFX(SFX.HasiyoSelect);

        DG.Tweening.DOVirtual.DelayedCall(1f, () =>
        {
            Vector3 startPos = new Vector3(-11f, UnityEngine.Random.Range(5f, 8f));
            Vector3 endPos = new Vector3(UnityEngine.Random.Range(2.5f, 4f), UnityEngine.Random.Range(-1f, -0.5f));
            GameManager.VFX.PlayEffect(startPos, endPos, AttackVFXType.FireBaba, 0f, Color.white, () =>
            {
                GameManager.VFX.PlayEffect(endPos, endPos, AttackVFXType.BabaExplode, 0f, Color.white);
                foreach (var enemy in GameManager.Battle.GetAliveEnemies())
                {
                    SlowEffect slow = new SlowEffect(slowDuration, 1, false, slowValue);
                    enemy.AddAbility(slow);

                    CombatEventContext dmgCtx = new CombatEventContext(owner, enemy, damage, DamageType.Normal);
                    enemy.TakeDamage(dmgCtx);

                    GameManager.Sound.PlaySFX(SFX.BabaExplode, Random.Range(0.9f, 1.1f));
                }
            });

            GameManager.Sound.PlaySFX(SFX.BabaFlying);
        });
    }
}

/// 피격으로 체력이 1이 될 경우 모든 적에게 고정 데미지 100 - 바바 발사
[System.Serializable]
public class LastStandAbility : RewardAbility
{
    [SerializeField] private float damage = 100f;

    private bool triggered = false;

    public override void OnEvent(CombatEvent eventType, CombatEventContext ctx)
    {
        if (eventType == CombatEvent.OnBattleStart)
        {
            triggered = false;
            return;
        }

        if (eventType != CombatEvent.OnTakeDamage) return;
        if (ctx.target != owner) return;
        if (triggered) return;

        var stats = owner.GetStat<UnitStats>();
        if (stats == null) return;

        if (Mathf.FloorToInt(stats.hp) != 1) return;

        triggered = true;

        foreach (var enemy in GameManager.Battle.GetAliveEnemies())
        {
            CombatEventContext dmgCtx = new CombatEventContext(owner, enemy, damage, DamageType.Fixed);
            enemy.TakeDamage(dmgCtx);
        }
    }
}
