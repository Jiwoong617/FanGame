using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum BattleState
{
    None,
    Intro,
    Processing,
    Finished
}

public class BattleManager
{
    public event Action OnPlayerDead;
    public event Action OnBattleWon;

    private PlayerUnit player;
    private List<EnemyUnit> enemies = new();
    private Transform enemyAnchor;
    private TargetMarker targetMarker;

    private BattleState state = BattleState.None;
    private float spawnSpacing = 4f;

    // 배틀 도중 소환을 위한 소환 몹 기록용 변수
    private EnemyUnit leftSummon;
    private EnemyUnit rightSummon;

    public void SetupBattle(PlayerUnit p, List<UnitData> enemyDataList)
    {
        player = p;
        enemies.Clear();

        if (enemyAnchor == null)
        {
            Debug.LogError("[BattleManager] Enemy Anchor is null!");
            return;
        }

        if (enemyDataList != null && enemyDataList.Count > 0)
        {
            SpawnEnemies(enemyDataList);
        }
        
        Debug.Log($"[BattleManager] Setup Complete. Player: {(player ? player.name : "null")}, Enemies: {enemies.Count}");
    }

    private void SpawnEnemies(List<UnitData> dataList)
    {
        int count = dataList.Count;
        float startX = -(count - 1) * 0.5f * spawnSpacing;

        for (int i = 0; i < count; i++)
        {
            if (dataList[i] == null || dataList[i].prefab == null) continue;

            float xOffset = startX + (i * spawnSpacing);
            Vector3 spawnPos = enemyAnchor.position + (enemyAnchor.right * xOffset);

            // 적 생성
            GameObject go = UnityEngine.Object.Instantiate(dataList[i].prefab, spawnPos, enemyAnchor.rotation);
            EnemyUnit enemy = go.GetComponent<EnemyUnit>();
            
            if (enemy != null)
            {
                enemy.Init(dataList[i]);
                enemies.Add(enemy);
            }
        }
    }

    public void CleanupBattle()
    {
        foreach (var enemy in enemies)
        {
            if (enemy != null && enemy.gameObject != null)
            {
                UnityEngine.Object.Destroy(enemy.gameObject);
            }
        }
        enemies.Clear();
        state = BattleState.None;

        leftSummon = null;
        rightSummon = null;
    }

    public void StartBattle()
    {
        state = BattleState.Intro;
    }

    public void StartProcessing()
    {
        Debug.Log("[BattleManager] Battle Started!");
        state = BattleState.Processing;

        if (player != null && enemies.Count > 0)
        {
            player.OnTargetChanged -= UpdateTargetMarker;
            player.OnTargetChanged += UpdateTargetMarker;
            player.SetTarget(enemies[0]);
            player.OnBattleStart();

            foreach (var enemy in enemies)
            {
                enemy.SetTarget(player);
                enemy.OnUnitDead -= HandleEnemyDead;
                enemy.OnUnitDead += HandleEnemyDead;
                enemy.OnBattleStart();
            }
        }
        else
        {
            Debug.LogWarning("[BattleManager] Units not found. Auto-win for testing?");
        }
    }

    public void OnUpdate()
    {
        if (state != BattleState.Processing) return;

        HandleTargeting();

        // 플레이어 업데이트
        if (player != null)
        {
            player.OnUpdate(Time.deltaTime);
            if (player.IsDead)
            {
                state = BattleState.Finished;
                OnPlayerDead?.Invoke();
                return;
            }
        }

        // 적 업데이트
        // 리스트 변경(삭제)이 일어날 수 있으므로 역순 순회 유지
        for (int i = enemies.Count - 1; i >= 0; i--)
        {
            if (enemies[i] != null)
            {
                enemies[i].OnUpdate(Time.deltaTime);
            }
        }
    }

    private void HandleEnemyDead(CombatUnit unit)
    {
        EnemyUnit enemy = unit as EnemyUnit;
        if (enemy != null)
        {
            Debug.Log($"[BattleManager] Event Received: {enemy.name} died.");
            enemy.OnUnitDead -= HandleEnemyDead;
            
            enemies.Remove(enemy);
            GameObject.Destroy(enemy.gameObject);

            // 승리 조건 체크
            if (enemies.Count == 0)
            {
                Debug.Log("[BattleManager] All enemies defeated!");
                state = BattleState.Finished;

                if (player != null)
                    player.OnBattleEnd();

                OnBattleWon?.Invoke();
            }
        }
    }

    private void HandleTargeting()
    {
        if (player == null) return;

        // 수동 타겟 변경 (New Input System)
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

            if (hit.collider != null)
            {
                EnemyUnit clickedEnemy = hit.collider.GetComponent<EnemyUnit>();
                if (clickedEnemy != null && !clickedEnemy.IsDead)
                {
                    player.SetTarget(clickedEnemy);
                }
            }
        }

        // 자동 타겟 변경 (현재 타겟이 없거나 죽었을 때)
        if (player.GetTarget() == null || player.GetTarget().IsDead)
        {
            EnemyUnit newTarget = enemies.Find(e => !e.IsDead);
            if (newTarget != null)
            {
                player.SetTarget(newTarget);
            }
        }
    }

    private void UpdateTargetMarker(CombatUnit newTarget)
    {
        if (newTarget != null && !newTarget.IsDead)
        {
            targetMarker?.PlaySnapEffect(newTarget.transform);
        }
    }

    public void SpawnEnemyMidBattle(UnitData data, EnemyUnit summoner)
    {
        if (data == null || data.prefab == null) return;
        if (enemies.Count >= 3)
            return;

        Vector3 spawnPos = summoner.transform.position;
        bool spawnLeft = false;

        // 왼/오 순서대로 소환
        if (leftSummon == null || leftSummon.IsDead)
        {
            spawnLeft = true;
            spawnPos -= summoner.transform.right * spawnSpacing;
        }
        else if (rightSummon == null || rightSummon.IsDead)
            spawnPos += summoner.transform.right * spawnSpacing;
        else
            return;

        GameObject go = UnityEngine.Object.Instantiate(data.prefab, spawnPos, summoner.transform.rotation);
        EnemyUnit enemy = go.GetComponent<EnemyUnit>();
        if (enemy != null)
        {
            enemy.Init(data);
            enemy.SetAttackDelay(UnityEngine.Random.Range(0f, 0.5f));

            if (spawnLeft) leftSummon = enemy;
            else rightSummon = enemy;

            enemy.SetTarget(player);
            enemy.OnUnitDead += HandleEnemyDead;
            enemy.OnBattleStart();

            enemies.Add(enemy);
        }
    }

    public List<EnemyUnit> GetAliveEnemies()
    {
        return enemies.FindAll(e => e != null && !e.IsDead);
    }

    public void SetEnemyAnchor()
    {
        GameObject eAnchorObj = GameObject.Find("EnemyAnchor");
        enemyAnchor = eAnchorObj.transform;
    }

    public void SetTargetMarker(TargetMarker targetMarker) => this.targetMarker = targetMarker;
}