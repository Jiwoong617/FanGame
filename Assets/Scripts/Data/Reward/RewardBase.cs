using UnityEngine;

public abstract class RewardBase : ScriptableObject
{
    public Sprite Icon;
    public string RewardName;
    [TextArea] public string Description;


    public abstract void Apply(PlayerUnit player);
}
