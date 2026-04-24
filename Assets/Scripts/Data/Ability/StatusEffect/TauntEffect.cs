using UnityEngine;

[System.Serializable]
public class TauntEffect : StatusEffect
{
    public CombatUnit taunter { get; private set; } // 나를 도발한 대상

    public TauntEffect()
    {
        effectType = EffectType.Taunt;
        isPermanent = false;
        stacks = 1;
    }

    public void SetTaunter(CombatUnit taunter)
    {
        this.taunter = taunter;
    }

    public override void OnUpdate(float delta)
    {
        base.OnUpdate(delta);

        if (taunter == null || taunter.IsDead)
        {
            IsFinished = true;
        }
    }

    protected override void OnAdded()
    {
        base.OnAdded();

        if (taunter != null && !taunter.IsDead)
        {
            owner.SetTarget(taunter);
            GameManager.VFX.PlayEffect(owner.transform.position, owner.transform.position, AttackVFXType.Taunt, 0f, Color.white);
        }
    }
}