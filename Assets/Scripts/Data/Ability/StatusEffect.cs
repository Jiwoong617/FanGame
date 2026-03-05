using UnityEngine;

public enum EffectType
{
    None,
    IronFortress, // 철옹성
    Vampire,      // 흡혈
    Reflect,      // 반사
    Disarm,       // 무장해제
    Slow,         // 둔화
    Inspire,      // 격려
    Mone,         // 모네 스킬
    AttackUp,     // 공격력증가
    AttackDown,   // 공격력 감소
}


[System.Serializable]
public abstract class StatusEffect : Ability
{
    public EffectType effectType;
    public float duration = 0;
    public int stacks = 1;
    public float effectValue = 0f;

    //영구 지속 버프인지
    public bool isPermanent = true;


    public override void OnUpdate(float delta)
    {
        base.OnUpdate(delta);

        if (!isPermanent && duration > 0)
        {
            duration -= delta;
            if (duration <= 0)
            {
                IsFinished = true;
            }
        }
    }

    // 중첩 시 호출될 함수
    public virtual void AddStack(int addCount, float newDuration)
    {
        //시간 제한 버프면 시간만 갱신하게 할거임
        if (!isPermanent && duration > 0)
        {
            duration = Mathf.Max(duration, newDuration);
        }
        else
        {
            stacks += addCount;
            if (newDuration == -1)
                duration = -1;
        }

        OnStackUpdated();
    }

    protected virtual void OnStackUpdated() { }
}