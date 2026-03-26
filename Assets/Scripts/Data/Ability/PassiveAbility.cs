using UnityEngine;

[System.Serializable]
public abstract class PassiveAbility : Ability
{
    [Header("Passive UI")]
    [Tooltip("UI 버프창에 띄울 패시브 아이콘")]
    public Sprite passiveIcon;

    public PassiveAbility()
    {
    }
}