using UnityEngine;

[CreateAssetMenu(fileName = "PlayerData", menuName = "Scriptable Objects/PlayerData")]
public class PlayerData : UnitData
{
    [Header("Combat Resource")]
    public float stamina;
    public float staminaRegen;
    public float fp;

    public float dodgeCost;
    public float parryCost;
}

[System.Serializable]
public class PlayerStats : UnitStats
{
    public float maxStamina;
    public float stamina;
    public float staminaRegen;

    public float maxFp;
    public float fp;

    public float dodgeCost;
    public float parryCost;

    public PlayerStats(PlayerData data) : base(data)
    {
        maxStamina = data.stamina;
        stamina = data.stamina;
        staminaRegen = data.staminaRegen;
        maxFp = data.fp;
        fp = data.fp;
        dodgeCost = data.dodgeCost;
        parryCost = data.parryCost;
    }
}