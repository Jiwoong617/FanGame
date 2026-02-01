using UnityEngine;

/// <summary>
/// Sciptable Objects
/// </summary>

[CreateAssetMenu(fileName = "UnitData", menuName = "Scriptable Objects/UnitData")]
public class UnitData : ScriptableObject
{
    [Header("Basic Info")]
    public string unitName;
    public GameObject prefab;
    public Sprite unitSprite;

    [Header("Combat Stats")]
    public float hp;
    public float defense;
    public float attackDamage;
    public float attackInterval;

    [Header("Description"), TextArea]
    public string unitDescription;
}


/// <summary>
/// 복사용 클래스
/// </summary>

[System.Serializable]
public class UnitStats
{
    public float maxHp;
    public float hp;
    public float defense;
    public float attackDamage;
    public float attackInterval;

    public UnitStats(UnitData data)
    {
        maxHp = data.hp;
        hp = data.hp;
        defense = data.defense;
        attackDamage = data.attackDamage;
        attackInterval = data.attackInterval;
    }
}