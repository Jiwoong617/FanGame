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
        {
            if (ability.TryUseSkill())
            {
                if (type == ActionType.Skill)
                {
                    CombatEventContext ctx = new CombatEventContext(this, target, 0);
                    TriggerAbility(CombatEvent.OnUseSkill, ctx);
                }
            }
        }
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
        transform.DOKill(true);
        spriteRenderer.transform.DOKill(true);

        spriteRenderer.sortingOrder = 30;

        Sequence deadSeq = DOTween.Sequence();

        deadSeq.Append(spriteRenderer.DOColor(Color.gray, 2f));
        deadSeq.InsertCallback(1.0f, () =>
        {
            if (unitData != null && unitData.unitDeadSprite != null)
            {
                spriteRenderer.sprite = unitData.unitDeadSprite;
            }
        });
        //spriteRenderer.transform.DORotate(new Vector3(0, 0, -90f), 2f).SetEase(Ease.OutBounce);
    }


    public override float TakeDamage(CombatEventContext ctx)
    {
        if(IsDead)
            return 0f;

        if (!ctx.isReflectDamage)
        {
            if (state == PlayerState.Dodging && (ctx.evadeType == AttackEvadeType.Both || ctx.evadeType == AttackEvadeType.DodgeOnly))
            {
                TriggerAbility(CombatEvent.OnDodgeSuccess, ctx);
                GameManager.VFX.ShowText(transform, "회피!", Color.cyan);
                return -1;
            }
            if (state == PlayerState.Parrying && (ctx.evadeType == AttackEvadeType.Both || ctx.evadeType == AttackEvadeType.ParryOnly))
            {
                ResetCooldown(ActionType.Parry);
                //playerStats.stamina = Mathf.Min(playerStats.maxStamina.GetValue(), playerStats.stamina + playerStats.parryCost.GetValue() * 0.5f);
                hitEffect?.Flash(false);

                TriggerAbility(CombatEvent.OnParrySuccess, ctx);
                GameManager.VFX.ShowText(transform, "패링!", Color.softYellow);
                return -1;
            }
        }

        //피격 전 이벤트
        TriggerAbility(CombatEvent.OnBeforeTakeDamage, ctx);

        //방어력 계산
        float finalDamage = ctx.value;
        if (ctx.damageType == DamageType.Normal && finalDamage > 0)
            finalDamage = Mathf.Max(1, finalDamage - stats.defense.GetValue());
        else if(finalDamage <= 0)
            finalDamage = 0;

        // 데미지, 디버프 적용
        stats.hp -= finalDamage;
        if (ctx.debuffs != null && ctx.debuffs.Count > 0)
        {
            foreach (var debuff in ctx.debuffs)
                AddAbility(debuff.Clone());
        }

        // 피격 후 이벤트
        ctx.value = finalDamage;
        TriggerAbility(CombatEvent.OnTakeDamage, ctx);

        //피격 이펙트
        hitEffect?.Flash(finalDamage > 0 ? true : false);
        RequestDamageText(ctx);

        if (stats.hp <= 0)
        {
            OnDead();
            return 0;
        }

        return finalDamage;
    }

    public void ChangeState(PlayerState newState, float duration)
    {
        state = newState;
        stateTimer = duration;

        if (newState == PlayerState.Dodging)
        {
            spriteRenderer.transform.DOKill(true);

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
            if (playerData.unitParrySprite != null)
            {
                spriteRenderer.sprite = playerData.unitParrySprite;
            }

            Sequence parrySeq = DOTween.Sequence();
            parrySeq.AppendInterval(duration);
            parrySeq.OnComplete(() =>
            {
                if (!IsDead)
                    ChangeToIdleSprite();
            });
        }
        else if (newState == PlayerState.Skill)
        {
            if (playerData.unitSkillSprite != null)
                spriteRenderer.sprite = playerData.unitSkillSprite;

            Sequence skillSeq = DOTween.Sequence();
            skillSeq.AppendInterval(duration);
            skillSeq.OnComplete(() =>
            {
                if (!IsDead)
                    ChangeToIdleSprite();
            });
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
        TauntEffect taunt = GetStatusEffect(EffectType.Taunt) as TauntEffect;
        CombatUnit finalTarget = inTarget;
        if (taunt != null && taunt.taunter != null && !taunt.taunter.IsDead)
        {
            finalTarget = taunt.taunter;
        }

        base.SetTarget(finalTarget);
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

    public override void ChangeToIdleSprite()
    {
        if (spriteRenderer != null && unitData.unitSprite != null)
        {
            spriteRenderer.sprite = playerData.unitBackSprite;
        }
    }

    public void UseFreeSkill()
    {
        if (actionMap.TryGetValue(ActionType.Skill, out ActiveAbility ability))
        {
            ability.ExecuteFree();
            CombatEventContext ctx = new CombatEventContext(this, target, 0);
            TriggerAbility(CombatEvent.OnUseSkill, ctx);
        }
    }
}
