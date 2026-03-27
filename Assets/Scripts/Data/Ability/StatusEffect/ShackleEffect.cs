using UnityEngine;

[System.Serializable]
public class ShackleEffect : StatusEffect
{
    public ShackleEffect()
    {
        effectType = EffectType.Shackle;
        this.stacks = 1;
    }

    public ShackleEffect(float duration, bool isPermanent) : this()
    {
        this.duration = duration;
        this.isPermanent = isPermanent;
    }
}