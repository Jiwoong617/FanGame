using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public enum PlayerState
{
    Idle,
    Dodging,
    Parrying,
    Skill
}

public class PlayerUnit : CombatUnit
{
    protected PlayerData playerData;

    protected PlayerStats playerStats;
    protected float stateTimer = 0f;

    public PlayerState state { get; private set; } = PlayerState.Idle;

    public event Action<ActionType, float> OnCooldownTriggered;
    private Dictionary<ActionType, ActiveAbility> actionMap = new Dictionary<ActionType, ActiveAbility>();


    public override void Init(UnitData unitData)
    {
        this.unitData = unitData;

        if (unitData is PlayerData playerData)
        {
            spriteRenderer.sprite = playerData.unitBackSprite;
            this.playerData = playerData;

            playerStats = new PlayerStats(playerData);
            base.stats = playerStats;

            if (combatUI == null)
                combatUI = GetComponentInChildren<CombatUnitUI>();
            combatUI.SetOwner(this);
            combatUI.Hide();

            InitializeAbilities(unitData);

            OnDamageTextRequested += GameManager.VFX.ShowDamageText;
            OnHealTextRequested += (amount) => GameManager.VFX.ShowHealText(transform, amount);
        }
        else
        {
            Debug.LogError("PlayerData 가 아님");
        }
    }

    public override void AddAbility(Ability newAbility)
    {
        base.AddAbility(newAbility);
        if (newAbility is ActiveAbility active && active.actionType != ActionType.None)
        {
            actionMap[active.actionType] = active;
        }
    }

    public override void OnBattleStart()
    {
        base.OnBattleStart();
        combatUI?.Show();

        if (playerStats != null)
            playerStats.stamina = playerStats.maxStamina.GetValue();

        foreach (var a in actionMap)
            if (a.Value != null)
                a.Value.ResetCooldown();

        state = PlayerState.Idle;
        stateTimer = 0f;
    }

    public override void OnBattleEnd()
    {
        base.OnBattleEnd();
        combatUI?.Hide();

        if (playerStats != null)
            playerStats.stamina = playerStats.maxStamina.GetValue();

        foreach (var a in actionMap)
            if (a.Value != null)
                a.Value.ResetCooldown();

        state = PlayerState.Idle;
        stateTimer = 0f;
    }

    public override void OnUpdate(float delta)
    {
        base.OnUpdate(delta);

        HandleInput();
        HandleState(delta);

        if (state == PlayerState.Idle)
        {
            ProcessAttackLoop(delta);
            RegenerateStamina(delta);
        }
    }

    private void HandleInput()
    {
        if (state != PlayerState.Idle) return;

        // TODO: 입력 매니저 연동
        if (Keyboard.current != null)
        {
            if (Keyboard.current.spaceKey.wasPressedThisFrame)
                UseAbility(ActionType.Dodge);
            else if (Keyboard.current.qKey.wasPressedThisFrame)
                UseAbility(ActionType.Parry);
            else if (Keyboard.current.fKey.wasPressedThisFrame)
                UseAbility(ActionType.Skill);
        }
    }

    private void UseAbility(ActionType type)
    {
        if (actionMap.TryGetValue(type, out ActiveAbility ability))
            ability.TryUseSkill();
    }

    private void HandleState(float delta)
    {
        if (state == PlayerState.Idle) return;

        stateTimer -= delta;
        if (stateTimer <= 0f)
            state = PlayerState.Idle;
    }


    public void ResetCooldown(ActionType type)
    {
        if (actionMap.TryGetValue(type, out ActiveAbility ability))
        {
            ability.ResetCooldown();
            NotifyCooldownStarted(type, 0f);
        }
    }

    public bool CheckAbilityCooldown(ActionType type)
    {
        if (actionMap.TryGetValue(type, out ActiveAbility ability))
        {
            return ability.IsOnCooldown();
        }
        return false;
    }


    public override void OnDead()
    {
        transform.DOKill();
        spriteRenderer.transform.DOKill();

        spriteRenderer.sortingOrder = 40;

        spriteRenderer.transform.DORotate(new Vector3(0, 0, -90f), 2f).SetEase(Ease.OutBounce);
    }


    public override float TakeDamage(CombatEventContext ctx)
    {
        if(IsDead)
            return 0f;

        if (!ctx.isReflectDamage)
        {
            if (state == PlayerState.Dodging)
                return 0;
            if (state == PlayerState.Parrying)
            {
                ResetCooldown(ActionType.Parry);
                playerStats.stamina = Mathf.Min(playerStats.maxStamina.GetValue(), playerStats.stamina + playerStats.parryCost.GetValue() * 0.5f);

                TriggerAbility(CombatEvent.OnParrySuccess, ctx);
                return 0;
            }
        }

        //피격 전 이벤트
        TriggerAbility(CombatEvent.OnBeforeTakeDamage, ctx);
        if (ctx.value <= 0)
            return 0;

        //방어력 계산
        float finalDamage = ctx.value;
        if (ctx.damageType == DamageType.Normal)
            finalDamage = Mathf.Max(1, finalDamage - stats.defense.GetValue());

        // 데미지 적용
        stats.hp -= finalDamage;
        Debug.Log($"[Player] Took {finalDamage} damage. HP: {stats.hp}");

        // 피격 후 이벤트
        ctx.value = finalDamage;
        TriggerAbility(CombatEvent.OnTakeDamage, ctx);

        if (stats.hp <= 0)
        {
            OnDead();
            return 0;
        }

        //피격 이펙트
        hitEffect?.Flash();
        RequestDamageText(ctx);

        return finalDamage;
    }

    public void ChangeState(PlayerState newState, float duration)
    {
        state = newState;
        stateTimer = duration;

        if (newState == PlayerState.Dodging)
        {
            spriteRenderer.transform.DOKill();

            float originalX = spriteRenderer.transform.position.x;
            float dodgeDistance = 1.5f;

            Sequence dodgeSeq = DOTween.Sequence();

            //일단 구르기 0.3, 제자리 0.2, 복귀 0.5로 했음
            dodgeSeq.Append(spriteRenderer.transform.DOMoveX(originalX - dodgeDistance, duration * 0.3f).SetEase(Ease.OutCubic));
            dodgeSeq.AppendInterval(duration * 0.2f);
            dodgeSeq.Append(spriteRenderer.transform.DOMoveX(originalX, duration * 0.5f).SetEase(Ease.InOutSine));
        }
        else if (newState == PlayerState.Parrying)
        {
            //TODO : 패링 연출
        }
        else if (newState == PlayerState.Skill)
        {
            //TODO : 스킬 연출
        }
    }

    public void NotifyCooldownStarted(ActionType type, float duration)
    {
        OnCooldownTriggered?.Invoke(type, duration);
    }

    public override T GetStat<T>()
    {
        return playerStats as T;
    }

    public override void SetTarget(CombatUnit inTarget)
    {
        base.SetTarget(inTarget);
        attackTimer = 0f;
    }

    public IEnumerable<ActiveAbility> GetActiveAbilities()
    {
        return actionMap.Values;
    }

    private void RegenerateStamina(float delta)
    {
        playerStats.stamina = Mathf.Min(playerStats.maxStamina.GetValue(), playerStats.stamina + playerStats.staminaRegen.GetValue() * delta);
    }

    // 스테이지 클리어 시 호출할 회복 함수
    public void FullyRestore()
    {
        var pStats = GetStat<PlayerStats>();

        pStats.hp = pStats.maxHp.GetValue();
        pStats.stamina = pStats.maxStamina.GetValue();
        pStats.fp = pStats.maxFp.GetValue();
    }

    protected override void PlayAttackVFX(Vector3 targetPos, float hitDelay)
    {
        if (unitData != null)
        {
            GameManager.VFX.PlayerAttackEffect(transform.position, target.transform.position,
                unitData.attackVFXType, playerData.attackVFXSprite, hitDelay);
        }
    }

    protected override void ChangeToIdleSprite()
    {
        if (spriteRenderer != null && unitData.unitSprite != null)
        {
            spriteRenderer.sprite = playerData.unitBackSprite;
        }
    }
}
