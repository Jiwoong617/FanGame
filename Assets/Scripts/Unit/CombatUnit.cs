using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CombatUnit : MonoBehaviour
{
    [SerializeField] protected CombatUnitUI combatUI;
    protected SpriteRenderer spriteRenderer;
    protected UnitData unitData;
    protected List<GameObject> _activeEffects = new List<GameObject>(); // 이펙트 관리

    protected const float ATTACK_THRESHOLD = 1f;

    public Action<CombatUnit> OnUnitDead;
    public event Action<CombatEventContext> OnDamageTextRequested;
    public event Action<float> OnHealTextRequested;

    //이건 ui 띄울것들임
    public event Action<float> OnActionBarUpdated;
    public event Action<StatusEffect> OnBuffAdded;
    public event Action<StatusEffect> OnBuffRemoved;
    public event Action<StatusEffect> OnBuffUpdated;

    protected UnitStats stats;
    protected CombatUnit target;
    protected float attackTimer = 0f;
    
    // 런타임 능력 리스트
    protected List<Ability> abilities = new List<Ability>();

    public bool IsDead => stats.hp <= 0;


    protected HitFlash hitEffect;


    protected virtual void Start()
    {
        hitEffect = GetComponent<HitFlash>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>(); // 임시 코드라 좀 더 정확하게 짜야함
    }

    protected void ProcessAttackLoop(float delta)
    {
        if (IsDead || target == null || target.IsDead) return;

        attackTimer += (delta * stats.attackSpeed.GetValue());
        OnActionBarUpdated?.Invoke(Mathf.Clamp01(attackTimer / ATTACK_THRESHOLD));

        if (attackTimer >= ATTACK_THRESHOLD)
        {
            Attack();
            attackTimer = 0f;
            OnActionBarUpdated?.Invoke(0f);
        }
    }

    public virtual void SetTarget(CombatUnit inTarget)
    {
        target = inTarget;
    }

    public CombatUnit GetTarget()
    {
        return target;
    }
    protected virtual void OnDestroy()
    {
        // 리스트에 남아있는 모든 이펙트 게임 오브젝트를 파괴합니다.
        foreach (var effect in _activeEffects)
        {
            if (effect != null)
            {
                // DOTween 애니메이션도 함께 중단하고 파괴합니다.
                effect.transform.DOKill();
                Destroy(effect);
            }
        }
        // 리스트를 비웁니다.
        _activeEffects.Clear();
    }
    public virtual void OnBattleStart()
    {
        TriggerAbility(CombatEvent.OnBattleStart, new CombatEventContext(this, target, 0));
    }

    public virtual void OnBattleEnd()
    {
        TriggerAbility(CombatEvent.OnBattleEnd, new CombatEventContext(this, target, 0));

        for (int i = abilities.Count - 1; i >= 0; i--)
        {
            if (abilities[i] is StatusEffect status)
            {
                if (!status.isPermanent || status.IsFinished)
                {
                    status.OnRemoved();
                    OnBuffRemoved?.Invoke(status);
                    abilities.RemoveAt(i);
                }
            }
        }
    }

    public virtual void OnUpdate(float delta)
    {
        if (IsDead) return;

        for (int i = abilities.Count - 1; i >= 0; i--)
        {
            var ability = abilities[i];
            ability.OnUpdate(delta);
            
            if (ability.IsFinished)
            {
                ability.OnRemoved();
                if (ability is StatusEffect status)
                    OnBuffRemoved?.Invoke(status);

                abilities.RemoveAt(i);
            }
        }
    }
    //
    public virtual void Init(UnitData data)
    {
        this.unitData = data;
    
        // 초기 스프라이트 설정
        if (spriteRenderer != null && unitData != null && unitData.unitSprite != null)
        {
            spriteRenderer.sprite = unitData.unitSprite;
        }
    }
    public abstract void OnDead();
    public abstract float TakeDamage(CombatEventContext info);
    public abstract T GetStat<T>() where T : UnitStats;

    public virtual void Attack()
    {
        StopCoroutine(nameof(AttackAnimation));
        StartCoroutine(AttackAnimation());

        StartCoroutine(AttackEffectCoroutine());
        if (target == null || IsDead) return;

        float damage = stats.attackDamage.GetValue();
        bool isCrit = false;

        if (UnityEngine.Random.Range(0f, 100f) < stats.criticalChance.GetValue())
        {
            isCrit = true;
            damage *= (stats.criticalDamage.GetValue() / 100f);
        }

        CombatEventContext attackCtx = new CombatEventContext(this, target, damage, DamageType.Normal, false, isCrit);
        float actualDamage = target.TakeDamage(attackCtx);

        if (actualDamage > 0 && !IsDead)
        {
            //이거 방어력 깎인 최종 데미지로 교체
            attackCtx.value = actualDamage;
            TriggerAbility(CombatEvent.OnAttack, attackCtx);
        }
    }
    // 스프라이트 변경 코루틴 코드
    protected virtual IEnumerator AttackAnimation()
    {
        if (spriteRenderer != null && unitData != null && unitData.unitBasicAttackSprite != null)
        {
            spriteRenderer.sprite = unitData.unitBasicAttackSprite;
        }
        yield return new WaitForSeconds(0.3f);

        if (spriteRenderer != null && unitData != null && unitData.unitSprite != null)
        {
            spriteRenderer.sprite = unitData.unitSprite;
        }
    }

    protected virtual IEnumerator AttackEffectCoroutine()
    {
        // 1. 필요한 데이터(타겟, 이펙트 스프라이트)가 없으면 실행하지 않음
        if (target == null || unitData == null || unitData.unitAttackEffectSprite == null)
        {
            yield break; // 코루틴 종료
        }

        // 2. 이펙트를 표시할 빈 게임 오브젝트를 생성
        GameObject effectObject = new GameObject("AttackEffect");
        _activeEffects.Add(effectObject);

        // 3. 이펙트 오브젝트의 위치를 타겟의 위치로 설정
        // (Z값을 살짝 조정하여 다른 스프라이트보다 앞에 보이게 할 수 있습니다)
        effectObject.transform.position = target.transform.position + new Vector3(0, 0, -0.1f);

        // 4. SpriteRenderer 컴포넌트를 추가하고 이펙트 스프라이트를 할당
        SpriteRenderer effectRenderer = effectObject.AddComponent<SpriteRenderer>();
        effectRenderer.sprite = unitData.unitAttackEffectSprite;

        // (선택 사항) 다른 스프라이트와 겹치지 않도록 Sorting Order를 높게 설정
        effectRenderer.sortingOrder = 10;

        // 5. 이펙트를 보여줄 시간만큼 대기
        yield return new WaitForSeconds(0.5f); // 0.5초 동안 보여줌 (시간 조절 가능)
        if (_activeEffects.Contains(effectObject))
        {
            _activeEffects.Remove(effectObject);
        }

        // 6. 이펙트 오브젝트를 파괴하여 화면에서 제거
        Destroy(effectObject);
    }

    protected void InitializeAbilities(UnitData data)
    {
        abilities.Clear();
        if (data.startingAbilities != null)
        {
            foreach (var ability in data.startingAbilities)
            {
                if (ability != null)
                {
                    AddAbility(ability.Clone());
                }
            }
        }
    }

    public virtual void AddAbility(Ability newAbility)
    {
        if (newAbility == null) return;

        if (newAbility is StatusEffect newStatus)
        {
            foreach (var ability in abilities)
            {
                if (ability is StatusEffect existingStatus &&
                    existingStatus.effectType == newStatus.effectType &&
                    existingStatus.isPermanent == newStatus.isPermanent &&
                    Mathf.Approximately(existingStatus.effectValue, newStatus.effectValue))
                {
                    existingStatus.AddStack(newStatus.stacks, newStatus.duration);
                    OnBuffUpdated?.Invoke(existingStatus);
                    return;
                }
            }
        }

        newAbility.Init(this);
        abilities.Add(newAbility);

        if (newAbility is StatusEffect addedStatus)
        {
            OnBuffAdded?.Invoke(addedStatus);
        }
    }

    public void TriggerAbility(CombatEvent type, CombatEventContext cec)
    {
        for (int i = 0; i < abilities.Count; i++)
            abilities[i].OnEvent(type, cec);
    }

    public void Heal(float amount)
    {
        if (IsDead || amount <= 0) return;

        stats.hp += amount;

        OnHealTextRequested?.Invoke(amount);
    }

    protected void RequestDamageText(CombatEventContext ctx)
    {
        OnDamageTextRequested?.Invoke(ctx);
    }

    protected void RequestActionBarUpdate(float value)
    {
        OnActionBarUpdated?.Invoke(value);
    }
}
