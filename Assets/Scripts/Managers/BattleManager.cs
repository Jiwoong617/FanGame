using System;
using System.Collections.Generic;
using UnityEngine;

public enum BattleState
{
    None,
    Processing,
    Finished
}

public class BattleManager
{
    public event Action OnPlayerDead;
    public event Action OnBattleWon;

    private PlayerUnit player;
    private List<EnemyUnit> enemies = new();

    private BattleState state = BattleState.None;

    public void SetupBattle(PlayerUnit p, List<EnemyUnit> eList)
    {
        player = p;
        enemies.Clear();
        if (eList != null) enemies.AddRange(eList);
        
        Debug.Log($"[BattleManager] Setup Complete. Player: {(player ? player.name : "null")}, Enemies: {enemies.Count}");
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
    }

    public void StartBattle()
    {
        Debug.Log("[BattleManager] Battle Started!");
        state = BattleState.Processing;

        if (player != null && enemies.Count > 0)
        {
            player.SetTarget(enemies[0]);
            foreach (var enemy in enemies)
            {
                enemy.SetTarget(player);
                enemy.OnUnitDead += HandleEnemyDead;
            }
        }
        else
        {
            Debug.LogWarning("[BattleManager] Units not found. Auto-win for testing?");
            // OnBattleWon?.Invoke(); // 테스트용
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

        // 승리 조건 체크
        if (enemies.Count == 0)
        {
            Debug.Log("[BattleManager] All enemies defeated!");
            state = BattleState.Finished;
            OnBattleWon?.Invoke();
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
        }
    }

    private void HandleTargeting()
    {
        if (player == null) return;

        // 수동 타겟 변경
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

            if (hit.collider != null)
            {
                EnemyUnit clickedEnemy = hit.collider.GetComponent<EnemyUnit>();
                if (clickedEnemy != null && !clickedEnemy.IsDead)
                {
                    player.SetTarget(clickedEnemy);
                    Debug.Log($"[Targeting] Player switched target to {clickedEnemy.name}");

                    // TODO: 타겟 변경 시각적 피드백 (화살표 UI 등)
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
                Debug.Log($"[Targeting] Auto-switched target to {newTarget.name}");
            }
        }
    }
}