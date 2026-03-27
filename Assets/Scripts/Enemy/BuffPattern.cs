using UnityEngine;

public enum BuffTargetType
{
    Self,           // 나 자신
    Other,          // 다른 적 하나
    OtherEnemies,   // 나 제외 다른 적들
    AllEnemies,     // 모든 적들
    AllCharacter    // 모든 캐릭터
}


[System.Serializable]
public class BuffPattern : EnemyPattern
{
    [Header("Buff Settings")]
    public BuffTargetType targetType = BuffTargetType.Self;

    [SerializeReference, SerializeReferenceDropdown]
    public StatusEffect buffEffect;

    [Tooltip("버프를 시전하는 모션 유지 시간")]
    public float castDuration = 1.0f;

    public override bool OnUpdate(EnemyUnit unit, float delta)
    {
        if (unit.IsAttacking) return false;

        unit.PlayActionAnimation(actionSprite, castDuration, () => 
        { 
            var allies = GameManager.Battle.GetAliveEnemies();
            switch (targetType)
            {
                case BuffTargetType.Self:
                    if (buffEffect != null)
                        unit.AddAbility(buffEffect.Clone());
                    //TODO : 버프 이펙트 (unit 위치)
                    break;

                case BuffTargetType.Other:
                    var others = allies.FindAll(a => a != unit);
                    if (others.Count > 0)
                    {
                        var target = others[Random.Range(0, others.Count)];
                        if (buffEffect != null)
                            target.AddAbility(buffEffect.Clone());
                        //TODO : 버프 이펙트 (target 위치)
                    }
                    break;

                case BuffTargetType.OtherEnemies:
                    foreach (var ally in allies)
                    {
                        if (ally == unit) continue;

                        if (buffEffect != null)
                            ally.AddAbility(buffEffect.Clone());
                        //TODO : 버프 이펙트 (ally 위치)
                    }
                    break;

                case BuffTargetType.AllEnemies:
                    foreach (var ally in allies)
                    {
                        if (buffEffect != null)
                            ally.AddAbility(buffEffect.Clone());
                        //TODO : 버프 이펙트 (ally 위치)
                    }
                    break;

                case BuffTargetType.AllCharacter:
                    foreach (var character in allies)
                    {
                        if (character != null)
                            character.AddAbility(buffEffect.Clone());
                        //TODO : 버프 이펙트 (ally 위치)
                    }
                    GameManager.Instance.Player.AddAbility(buffEffect.Clone());
                    //TODO : 버프 이펙트 (플레이어 위치)

                    break;
            }
        });

        return true;
    }
}


[System.Serializable]
public class HealPattern : EnemyPattern
{
    [Header("Heal Settings")]
    public BuffTargetType targetType = BuffTargetType.Self;
    [Tooltip("체크하면 최대 체력 비례 퍼센트 회복 (30 = 30%)\n체크 해제하면 고정 수치 회복")]
    public bool isPercent = false;
    [Tooltip("회복량 (고정 수치 or 퍼센트)")]
    public float healAmount = 10f;

    [Tooltip("힐을 시전하는 모션 유지 시간")]
    public float castDuration = 1.0f;

    public override bool OnUpdate(EnemyUnit unit, float delta)
    {
        if (unit.IsAttacking) return false;

        unit.PlayActionAnimation(actionSprite, castDuration, () =>
        {
            var friends = GameManager.Battle.GetAliveEnemies();
            switch (targetType)
            {
                case BuffTargetType.Self:
                    ApplyHeal(unit);
                    break;

                case BuffTargetType.Other:
                    var others = friends.FindAll(f => f != unit);
                    if (others.Count > 0)
                    {
                        var target = others[Random.Range(0, others.Count)];
                        ApplyHeal(target);
                    }
                    break;

                case BuffTargetType.OtherEnemies:
                    foreach (var friend in friends)
                    {
                        if (friend == unit) continue;
                        ApplyHeal(friend);
                    }
                    break;

                case BuffTargetType.AllEnemies:
                    foreach (var friend in friends)
                    {
                        ApplyHeal(friend);
                    }
                    break;

                case BuffTargetType.AllCharacter:
                    foreach (var friend in friends)
                    {
                        ApplyHeal(friend);
                    }
                    ApplyHeal(GameManager.Instance.Player);
                    break;
            }
        });

        return true;
    }

    private void ApplyHeal(CombatUnit target)
    {
        float finalAmount = healAmount;
        if (isPercent)
        {
            var stats = target.GetStat<UnitStats>();
            if (stats != null)
                finalAmount = stats.maxHp.GetValue() * (healAmount/100);
        }

        target.Heal(finalAmount);
    }
}