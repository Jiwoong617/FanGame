using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DeathSpawnPassive : PassiveAbility
{
    [Header("Death Spawn Settings")]
    [Tooltip("죽을 때 소환할 적 데이터 리스트")]
    public List<UnitData> summonList = new List<UnitData>();

    [Tooltip("한 번에 몇마리 소환할 건지")]
    public int spawnCount = 2;
    [Tooltip("True: 풀에서 각각 다른 종류의 몬스터를 뽑아 소환\nFalse: 풀에서 랜덤으로 1종류만 골라 spawnCount만큼 소환")]
    public bool isRandom = false;

    public DeathSpawnPassive()
    {
        combatEvent = CombatEvent.OnBeforeDead;
    }

    public override void OnEvent(CombatEvent eventType, CombatEventContext context)
    {
        if (eventType == combatEvent && owner != null)
        {
            ExecuteDeathSpawn();
        }
    }

    private void ExecuteDeathSpawn()
    {
        if (summonList == null || summonList.Count == 0) return;

        if (!isRandom)
        {
            int pickIndex = Random.Range(0, summonList.Count);
            UnitData selectedData = summonList[pickIndex];

            for (int i = 0; i < spawnCount; i++)
                GameManager.Battle.SpawnEnemyMidBattle(selectedData, owner);
        }
        else
        {
            for (int i = 0; i < spawnCount; i++)
            {
                int pickIndex = Random.Range(0, summonList.Count);
                UnitData selectedData = summonList[pickIndex];

                GameManager.Battle.SpawnEnemyMidBattle(selectedData, owner);
            }
        }

        GameManager.Sound.PlaySFX(SFX.Summon);
    }
}