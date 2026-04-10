using UnityEngine;

[System.Serializable]
public class PassiveAbility : Ability
{
    [Header("Passive UI")]
    [Tooltip("UI 버프창에 띄울 패시브 아이콘")]
    public Sprite passiveIcon;

    [Tooltip("툴팁에 표시될 패시브 설명")]
    [TextArea(2, 4)]
    public string passiveName = "패시브 능력";

    [Tooltip("툴팁에 표시될 패시브 설명")]
    [TextArea(2, 4)]
    public string passiveDescription = "패시브 설명";

    public PassiveAbility()
    {
    }
}