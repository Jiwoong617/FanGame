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

    protected override void OnAdded()
    {
        GameManager.VFX.PlayEffect(owner.transform.position, owner.transform.position, AttackVFXType.Shackle, 0f, Color.white);
    }
}