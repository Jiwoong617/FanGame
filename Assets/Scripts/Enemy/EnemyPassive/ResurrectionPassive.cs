using UnityEngine;
using DG.Tweening;

[System.Serializable]
public class ResurrectionPassive : PassiveAbility
{
    [Header("Resurrection Settings")]
    [Tooltip("최대 부활 가능 횟수")]
    public int maxResurrections = 1;

    [Tooltip("쓰러진 뒤 부활할 때까지의 대기 시간 (초)")]
    public float delayTime = 2.0f;

    [Tooltip("부활 시 회복할 최대 체력 비율 (예: 50 = 50%)")]
    public float reviveHpPercent = 50f;
    private int currentCount = 0;

    public ResurrectionPassive()
    {
        combatEvent = CombatEvent.OnBeforeDead;
    }

    public override void OnEvent(CombatEvent eventType, CombatEventContext context)
    {
        if (eventType == combatEvent && owner != null)
        {
            if (currentCount < maxResurrections)
            {
                currentCount++;
                ExecuteResurrectionVisualSequence();
            }
        }
    }

    private void ExecuteResurrectionVisualSequence()
    {
        EnemyUnit enemy = owner as EnemyUnit;
        if (enemy == null) return;

        enemy.CancelDeath();
        enemy.CancelCurrentAction();
        var col = enemy.GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        var spriteRenderer = enemy.GetSpriteRenderer();
        var unitData = enemy.unitData;

        Sequence resSeq = DOTween.Sequence();

        resSeq.AppendCallback(() =>
        {
            if (unitData != null && unitData.unitDeadSprite != null)
                spriteRenderer.sprite = unitData.unitDeadSprite;
        });
        resSeq.AppendInterval(delayTime);
        resSeq.AppendCallback(() =>
        {
            if (enemy == null) return; // 씬 넘어가서 삭제 방어

            spriteRenderer.color = Color.white;
            enemy.ChangeToIdleSprite();
        }
        );

        resSeq.OnComplete(() =>
        {
            if (enemy == null) return;

            if (col != null) col.enabled = true;
            enemy.GetStat<UnitStats>().hp = 0.01f;
            float healAmount = enemy.GetStat<UnitStats>().maxHp.GetValue() * (reviveHpPercent / 100f);
            enemy.Heal(healAmount);
        });
    }
}