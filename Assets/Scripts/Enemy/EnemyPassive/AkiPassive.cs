using UnityEngine;

[System.Serializable]
public class AkiPassive : PassiveAbility
{
    [Header("Death Match Settings")]
    [Tooltip("서로에게 적용할 디버프 (예: DamageAmplificationEffect)")]
    [SerializeReference, SerializeReferenceDropdown]
    public StatusEffect damageDebuff;

    [Tooltip("디버프 추가 부여 주기 (초)")]
    public float applyInterval = 10f;
    private float currentTimer = 0f;

    public AkiPassive()
    {
        combatEvent = CombatEvent.OnBattleStart;
    }

    public override void OnEvent(CombatEvent eventType, CombatEventContext context)
    {
        if (eventType == combatEvent && owner != null && !owner.IsDead)
        {
            currentTimer = 0f;
            ApplyDebuff();
        }
    }

    public override void OnUpdate(float delta)
    {
        base.OnUpdate(delta);

        if (owner == null || owner.IsDead) return;

        currentTimer += delta;
        if (currentTimer >= applyInterval)
        {
            currentTimer -= applyInterval;
            ApplyDebuff();
        }
    }

    private void ApplyDebuff()
    {
        if (damageDebuff == null) return;

        owner.AddAbility(damageDebuff.Clone());
        var player = GameManager.Instance.Player;
        if (player != null && !player.IsDead)
        {
            player.AddAbility(damageDebuff.Clone());
        }
    }
}