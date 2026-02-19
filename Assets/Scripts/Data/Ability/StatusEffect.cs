using UnityEngine;

public enum EffectType
{
    None,
    IronFortress, // 철옹성
    Vampire,      // 흡혈
    Reflect,      // 반사
    Disarm,       // 무장해제
    Slow,         // 둔화
    Inspire       // 격려
}


[System.Serializable]
public abstract class StatusEffect : Ability
{
    public EffectType effectType;
    public float duration = 0;
    public int stacks = 1;

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
        stacks += addCount;
        duration = Mathf.Max(duration, newDuration);
        OnStackUpdated();
    }

    protected virtual void OnStackUpdated() { }
}