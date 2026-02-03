using System;
using System.Collections.Generic;
using UnityEngine;

public class StageManager
{
    public event Action<string> OnStageSelected; // string은 임시 (StageData 타입 등)

    private Transform playerAnchor;
    private Transform enemyAnchor;
    
    private float spawnSpacing = 5f; 


    public void SetAnchors(Transform pAnchor, Transform eCenterAnchor)
    {
        playerAnchor = pAnchor;
        enemyAnchor = eCenterAnchor;
    }

    public PlayerUnit SpawnPlayer(UnitData data, CombatResourceData resourceData)
    {
        if (data == null || playerAnchor == null || data.prefab == null)
        {
            Debug.LogError("Something wrong");
            return null;
        }

        GameObject go = UnityEngine.Object.Instantiate(data.prefab, playerAnchor.position, playerAnchor.rotation);
        PlayerUnit CurrentPlayer = go.GetComponent<PlayerUnit>();
        CurrentPlayer.Init(data, resourceData);
        
        return CurrentPlayer;
    }

    public List<EnemyUnit> SpawnEnemies(List<UnitData> enemiesData)
    {
        if (enemyAnchor == null)
        {
            Debug.LogError("[StageManager] Enemy Center Anchor is missing!");
            return new List<EnemyUnit>();
        }

        List<EnemyUnit> spawnedEnemies = new List<EnemyUnit>();
        int count = enemiesData.Count;
        
        if (count == 0) return spawnedEnemies;
        
        float startX = -(count - 1) * 0.5f * spawnSpacing;

        for (int i = 0; i < count; i++)
        {
            float xOffset = startX + (i * spawnSpacing);
            Vector3 spawnPos = enemyAnchor.position + (enemyAnchor.right * xOffset);

            GameObject go = UnityEngine.Object.Instantiate(enemiesData[i].prefab, spawnPos, enemyAnchor.rotation);
            EnemyUnit enemy = go.GetComponent<EnemyUnit>();
            if (enemy != null)
            {
                enemy.Init(enemiesData[i]); 
                spawnedEnemies.Add(enemy);
            }
        }
        
        return spawnedEnemies;
    }
}
