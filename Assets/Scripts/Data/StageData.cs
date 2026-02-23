using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StageData", menuName = "Scriptable Objects/StageData")]
public class StageData : ScriptableObject
{
    [Header("Enemies")]
    public List<EnemyGroup> normalEnemyPool;
    public List<UnitData> eliteEnemyPool;
    public List<UnitData> bossPool;

    [Header("Rewards")]
    public List<RewardData> rewards;
}

[System.Serializable]
public class EnemyGroup
{
    public List<UnitData> enemies;
}