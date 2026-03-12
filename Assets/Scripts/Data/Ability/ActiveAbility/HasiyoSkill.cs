using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HasiyoSkill : ActiveAbility
{
    [SerializeField] private Sprite fireballSprite;
    [SerializeField] private Sprite iceSprite;
    [SerializeField] private Sprite vineSprite;

    public HasiyoSkill()
    {
        actionType = ActionType.Skill;
    }

    protected override bool CheckAndConsumeCost(PlayerUnit player)
    {
        var stats = player.GetStat<PlayerStats>();
        float cost = 1f;

        if (stats.fp >= cost)
        {
            stats.fp -= cost;
            return true;
        }

        return false;
    }

    protected override float GetCooldown(PlayerUnit player)
    {
        return player.GetStat<PlayerStats>().skillCoolTime.GetValue();
    }

    protected override void ExecuteSkill(PlayerUnit player)
    {
        List<EnemyUnit> enemies = GameManager.Battle.GetAliveEnemies();
        if (enemies == null || enemies.Count == 0)
            return;

        //TODO : 이거 일단 0.3초로 해놓음
        player.ChangeState(PlayerState.Skill, 0.3f);

        // 0 불, 1 얼음, 2 대지
        int magicType = Random.Range(0, 3);

        switch (magicType)
        {
            case 0:
                foreach (var enemy in enemies)
                    Fireball(player,enemy);
                break;
            case 1:
                foreach (var enemy in enemies)
                    Slow(enemy);
                break;
            case 2:
                foreach (var enemy in enemies)
                    Disarm(enemy);
                break;
        }
    }

    private void Fireball(PlayerUnit player, EnemyUnit enemy)
    {
        Vector3 targetPos = enemy.transform.position;
        Vector3 startPos = targetPos + Vector3.up * 4f;
        float dropDuration = 0.5f;

        GameManager.VFX.ShowCustomEffect(startPos, fireballSprite, (t, sr, onComplete) =>
        {
            t.localScale = Vector3.one;

            t.DOMove(targetPos, dropDuration).SetEase(Ease.InExpo).OnComplete(() =>
            {
                if (enemy != null && !enemy.IsDead)
                {
                    var stats = player.GetStat<PlayerStats>();
                    float baseDamage = stats.attackDamage.GetValue();
                    CombatEventContext ctx = new CombatEventContext(player, enemy, baseDamage, DamageType.Fixed, false);
                    enemy.TakeDamage(ctx);
                }

                // 메테오가 바닥에 박히며 찌그러지는 느낌
                t.DOScale(new Vector3(1.3f, 0.2f, 1f), 0.15f).SetEase(Ease.OutQuad);
                t.DOMoveY(targetPos.y - 0.2f, 0.15f);
                sr.DOFade(0f, 0.15f).OnComplete(() =>
                {
                    onComplete?.Invoke();
                });
            });
        });
    }

    private void Slow(EnemyUnit enemy)
    {
        float slowDuration = 5f;
        SlowEffect slow = new SlowEffect(slowDuration, 1, false, 0.5f);
        enemy.AddAbility(slow);

        Vector3 centerPos = enemy.transform.position;
        Vector3 feetPos = centerPos + Vector3.down * 0.5f;

        GameManager.VFX.ShowCustomEffect(feetPos, iceSprite, (t, sr, onComplete) =>
        {
            sr.color = new Color(1f, 1f, 1f, 0f);
            t.localScale = new Vector3(1f, 0f, 1f);
            t.position = feetPos;

            float encaseTime = 1f;

            t.DOScaleY(1f, encaseTime).SetEase(Ease.OutCubic);
            sr.DOFade(0.7f, encaseTime);
            t.DOMoveY(centerPos.y, encaseTime).SetEase(Ease.OutCubic).OnComplete(() =>
            {
                sr.DOFade(0f, 0.3f).SetDelay(encaseTime*2).OnComplete(() =>
                {
                    onComplete?.Invoke();
                });
            });
        });
    }

    private void Disarm(EnemyUnit enemy)
    {
        float disarmDuration = 2f;
        DisarmEffect disarm = new DisarmEffect(disarmDuration, 1, false);
        enemy.AddAbility(disarm);

        Vector3 feetPos = enemy.transform.position + Vector3.down * 0.2f;

        GameManager.VFX.ShowCustomEffect(feetPos, vineSprite, (t, sr, onComplete) =>
        {
            sr.color = new Color(1f, 1f, 1f, 0f);
            t.localScale = new Vector3(1.2f, 0.2f, 1f);
            t.position = feetPos;

            float growTime = 0.5f;

            sr.DOFade(1f, growTime);

            t.DOScaleY(1.2f, growTime).SetEase(Ease.OutBack).OnComplete(() =>
            {
                sr.DOFade(0f, 0.3f).SetDelay(disarmDuration - growTime * 2f).OnComplete(() =>
                {
                    onComplete?.Invoke();
                });
            });
        });
    }
}