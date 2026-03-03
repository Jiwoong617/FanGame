using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StageData", menuName = "Scriptable Objects/StageData")]
public class StageData : ScriptableObject
{
    [Header("Enemies")]
    public List<EnemyGroup> normalEnemyPool;
    public List<EnemyGroup> eliteEnemyPool;
    public List<EnemyGroup> bossPool;
}

[System.Serializable]
public class EnemyGroup
{
    public List<UnitData> enemies;
}