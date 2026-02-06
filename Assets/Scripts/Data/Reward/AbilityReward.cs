using UnityEngine;

[CreateAssetMenu(fileName = "NewAbilityReward", menuName = "Reward/Ability Reward")]
public class AbilityReward : RewardBase
{
    public float value; 

    public override void Apply(PlayerUnit player)
    {
        if (player == null) return;

        // TODO: 플레이어의 버프/패시브 시스템에 능력 등록
    }
}
