using UnityEngine;

[System.Serializable]
public class SetHpAbility : RewardAbility
{
    public int value = 1;

    protected override void OnAdded()
    {
        if (owner == null) return;

        var stats = owner.GetStat<UnitStats>();
        if (stats != null)
        {
            stats.hp = value;
        }
    }
}