using UnityEngine;
using UnityEngine.InputSystem;

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

    protected PlayerStats playerStats;

    protected PlayerState state = PlayerState.Idle;
    protected float stateTimer = 0f;


    public override void Init(UnitData unitData)
    {
        if(unitData is PlayerData playerData)
        {
            playerStats = new PlayerStats(playerData);
            base.stats = playerStats;
        }
        else
        {
            Debug.LogError("PlayerData 형식 아님");
        }
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
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
            TryDodge();
        else if (Keyboard.current.qKey.wasPressedThisFrame)
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
        if (playerStats.stamina >= playerStats.dodgeCost)
        {
            playerStats.stamina -= playerStats.dodgeCost;
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
        if (playerStats.stamina >= playerStats.parryCost)
        {
            playerStats.stamina -= playerStats.parryCost;
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
        playerStats.stamina = Mathf.Min(playerStats.maxStamina, playerStats.stamina + playerStats.staminaRegen * delta);
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
            playerStats.stamina = Mathf.Min(playerStats.maxStamina, playerStats.stamina + playerStats.parryCost * 0.5f);

           // TODO : 뭐 카운터같은거 추가할거면 추가
           // TODO : 이펙트 등

            return;
        }

        float finalDamage = Mathf.Max(1, damage - stats.defense);
        stats.hp -= finalDamage;
        Debug.Log($"[Player] Took {finalDamage} damage. HP: {stats.hp}");
    }

    public CombatUnit GetTarget() => target;

    public override T GetStat<T>()
    {
        return playerStats as T;
    }
}
