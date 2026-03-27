using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SummonPattern : EnemyPattern
{
    [Tooltip("몬스터 풀")]
    public List<UnitData> summonList = new List<UnitData>();

    [Tooltip("한 번에 몇마리 소환할 건지")]
    [Range(1, 2)] public int spawnCount = 1;

    [Tooltip("True: 풀에서 각각 다른 종류의 몬스터를 뽑아 소환\nFalse: 풀에서 랜덤으로 1종류만 골라 spawnCount만큼 소환")]
    public bool isRandom = false;

    [Tooltip("소환 시전하는 모션 유지 시간")]
    public float castDuration = 1.0f;

    public override bool CanExecute(EnemyUnit unit)
    {
        if(base.CanExecute(unit))
        {
            return GameManager.Battle.GetAliveEnemies().Count < 3;
        }

        return false;
    }

    public override bool OnUpdate(EnemyUnit unit, float delta)
    {
        if (unit.IsAttacking) return false;

        unit.PlayActionAnimation(actionSprite, castDuration, () =>
        {
            if (summonList != null && summonList.Count > 0)
            {
                if (!isRandom)
                {
                    int pickIndex = Random.Range(0, summonList.Count);
                    UnitData selectedData = summonList[pickIndex];

                    for (int i = 0; i < spawnCount; i++)
                        GameManager.Battle.SpawnEnemyMidBattle(selectedData, unit);
                }
                else
                {
                    for (int i = 0; i < spawnCount; i++)
                    {
                        int pickIndex = Random.Range(0, summonList.Count);
                        UnitData selectedData = summonList[pickIndex];

                        GameManager.Battle.SpawnEnemyMidBattle(selectedData, unit);
                    }
                }
            }
        });


        return true;
    }
}