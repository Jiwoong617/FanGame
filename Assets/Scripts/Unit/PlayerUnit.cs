using UnityEngine;

public enum PlayerState
{
    Idle,
    Dodging,
    Parrying
}

public class PlayerUnit : CombatUnit
{
    private const float DODGE_DURATION = 0.7f;
    private const float PARRY_DURATION = 0.2f;


    protected CombatResources combatResources;

    protected PlayerState state = PlayerState.Idle;
    protected float stateTimer = 0f;


    public void Init(UnitData unitData, CombatResourceData combatResourceData)
    {
        stats = new UnitStats(unitData);
        combatResources = new CombatResources(combatResourceData);
    }


    public override void OnUpdate(float delta)
    {
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

        // TODO: 입력 매핑 수정
        if (Input.GetKeyDown(KeyCode.Space))
            TryDodge();
        else if (Input.GetKeyDown(KeyCode.LeftShift))
            TryParry();
    }

    private void HandleState(float delta)
    {
        if (state == PlayerState.Idle) return;

        stateTimer -= delta;
        if (stateTimer <= 0f)
            state = PlayerState.Idle;
    }

    private void TryDodge()
    {
        if (combatResources.stamina >= combatResources.dodgeCost)
        {
            combatResources.stamina -= combatResources.dodgeCost;
            state = PlayerState.Dodging;
            stateTimer = DODGE_DURATION;
            Debug.Log("Dodge");
        }
        else
        {
            Debug.Log("[Player] Not enough stamina to Dodge!");
        }
    }

    private void TryParry()
    {
        if (combatResources.stamina >= combatResources.parryCost)
        {
            combatResources.stamina -= combatResources.parryCost;
            state = PlayerState.Parrying;
            stateTimer = PARRY_DURATION;
        }
        else
        {
            Debug.Log("[Player] Not enough stamina to Parry!");
        }
    }

    public override void OnDead()
    {
        Debug.Log("Player Dead");
    }

    void RegenerateStamina(float delta)
    {
        combatResources.stamina = Mathf.Min(combatResources.maxStamina, combatResources.stamina + combatResources.staminaRegen * delta);
    }

    public override void Attack()
    {
        if (target != null)
        {
            Debug.Log($"[Player] Attacks {target.name} for {stats.attackDamage} damage!");
            target.TakeDamage(stats.attackDamage);
        }
    }

    public override void TakeDamage(float damage)
    {
        if (state == PlayerState.Dodging)
        {
            Debug.Log("Dodge Success");
            return;
        }

        if (state == PlayerState.Parrying)
        {
            Debug.Log("Parry Success! Stamina Refunded.");
            combatResources.stamina = Mathf.Min(combatResources.maxStamina, combatResources.stamina + combatResources.parryCost * 0.5f);

           // TODO : 뭐 카운터같은거 추가할거면 추가
           // TODO : 이펙트 등

            return;
        }

        float finalDamage = Mathf.Max(1, damage - stats.defense);
        stats.hp -= finalDamage;
        Debug.Log($"[Player] Took {finalDamage} damage. HP: {stats.hp}");
    }

    public CombatUnit GetTarget() => target;
}
