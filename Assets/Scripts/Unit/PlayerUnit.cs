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
    Parrying
}

public class PlayerUnit : CombatUnit
{
    protected PlayerStats playerStats;
    protected float stateTimer = 0f;

    public PlayerState state { get; private set; } = PlayerState.Idle;

    public event Action<ActionType, float> OnCooldownTriggered;
    private Dictionary<ActionType, ActiveAbility> actionMap = new Dictionary<ActionType, ActiveAbility>();


    public override void Init(UnitData unitData)
    {
        base.Init(unitData);
        if (unitData is PlayerData playerData)
        {
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
        Debug.Log("Player Dead");
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
    }

    public void NotifyCooldownStarted(ActionType type, float duration)
    {
        OnCooldownTriggered?.Invoke(type, duration);
    }

    public override T GetStat<T>()
    {
        return playerStats as T;
    }

    protected override IEnumerator AttackAnimation()
   {
        if (spriteRenderer != null && unitData != null && unitData.unitBasicAttackSprite != null)
       {
           spriteRenderer.sprite = unitData.unitBasicAttackSprite;
       }

        yield return new WaitForSeconds(0.3f);

        if (spriteRenderer != null && unitData != null && unitData.unitBasicAttackSprite != null && unitData is PlayerData playerData)
        {
            spriteRenderer.sprite = playerData.unitBackSprite;
        }

        else if (spriteRenderer != null && unitData != null && unitData.unitSprite != null)
        {
            spriteRenderer.sprite = unitData.unitSprite;
        }
    }

    protected override IEnumerator AttackEffectCoroutine()
    {
        // 1. 필요한 데이터(타겟, 이펙트 스프라이트)가 없으면 실행하지 않음
        if (target == null || unitData == null || unitData.unitAttackEffectSprite == null)
        {
            yield break; // 코루틴 종료
        }
    
        // --- DOTween을 사용한 연출 ---
    
        // 2. 이펙트용 게임 오브젝트 생성 및 설정
        GameObject effectObject = new GameObject("PlayerAttackEffect");
        _activeEffects.Add(effectObject);
        SpriteRenderer effectRenderer = effectObject.AddComponent<SpriteRenderer>();
        effectRenderer.sprite = unitData.unitAttackEffectSprite;
        effectRenderer.sortingOrder = 10; // 다른 스프라이트보다 앞에 보이도록 설정
        effectRenderer.color = new Color(1, 1, 1, 0.8f); // 약간 투명하게 시작
    
        // 3. 연출 시작 위치와 각도 설정
        Vector3 direction = target.transform.position - transform.position; // 타겟을 향하는 방향
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg; // 방향을 각도로 변환
    
        effectObject.transform.position = target.transform.position + new Vector3(0.5f,1); //타겟 위치 대각선 시작
        effectObject.transform.rotation = Quaternion.Euler(0, 0, angle - 225f); // -90도 기울여서 시작
    
        // 4. DOTween 시퀀스를 사용하여 애니메이션 제작
        Sequence sequence = DOTween.Sequence();
    
        // 90도 회전
        sequence.Join(effectObject.transform.DORotate(new Vector3(0, 0, angle - 90f), 0.3f).SetEase(Ease.InExpo));

        // 0.3초에 걸쳐 서서히 사라지게 함
        sequence.Append(effectRenderer.DOFade(0, 0.3f));

        // 시퀀스가 모두 끝나면 이펙트 오브젝트를 파괴
        sequence.OnComplete(() =>
        {
            if (_activeEffects.Contains(effectObject))
            {
                _activeEffects.Remove(effectObject);
            }
            Destroy(effectObject);
        });
    
        yield break; // 코루틴의 역할은 시퀀스를 실행하는 것까지이므로 바로 종료
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
}
