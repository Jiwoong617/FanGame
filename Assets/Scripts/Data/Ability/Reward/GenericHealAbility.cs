using UnityEngine;

[System.Serializable]
public class GenericHealAbility : BaseTriggerAbility
{
    [Header("Heal Settings")]
    [SerializeField] private float healHpAmount = 0f;
    [SerializeField] private float recoverStaminaAmount = 0f;
    [SerializeField] private float recoverFpAmount = 0f;

    protected override void ExecuteEffect(CombatEventContext context)
    {
        if (healHpAmount > 0)
        {
            owner.Heal(healHpAmount);
        }

        if (owner is PlayerUnit player)
        {
            var stats = player.GetStat<PlayerStats>();
            if (stats != null)
            {
                if (recoverStaminaAmount > 0) stats.stamina += recoverStaminaAmount;
                if (recoverFpAmount > 0) stats.fp += recoverFpAmount;
            }
        }
    }
}