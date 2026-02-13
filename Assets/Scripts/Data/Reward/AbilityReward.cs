using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewAbilityReward", menuName = "Reward/Ability Reward")]
public class AbilityReward : RewardBase
{
    [Header("Ability Settings")]
    [SerializeReference, SerializeReferenceDropdown]
    public List<Ability> abilities = new List<Ability>();

    public override void Apply(PlayerUnit player)
    {
        if (player == null) return;
        if (abilities == null || abilities.Count == 0) return;

        foreach (var ability in abilities)
        {
            if (ability != null)
            {
                Ability newAbility = ability.Clone();
                player.AddAbility(newAbility);
            }
        }
    }
}
