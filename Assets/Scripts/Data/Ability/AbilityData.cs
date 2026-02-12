using UnityEngine;

public abstract class AbilityData : ScriptableObject
{
    public string abilityName;
    [TextArea] public string description;
    public Sprite icon;

    public abstract Ability CreateAbility();
}
