using UnityEngine;

[CreateAssetMenu(fileName = "CombatResourceData", menuName = "Scriptable Objects/CombatResourceData")]
public class CombatResourceData : ScriptableObject
{
    [Header("Combat Resource")]
    public float stamina;
    public float staminaRegen;
    public float fp;

    public float dodgeCost;
    public float parryCost;
}

[System.Serializable]
public class CombatResources
{
    public float maxStamina;
    public float stamina;
    public float staminaRegen;

    public float maxFp;
    public float fp;

    public float dodgeCost;
    public float parryCost;

    public CombatResources(CombatResourceData data)
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