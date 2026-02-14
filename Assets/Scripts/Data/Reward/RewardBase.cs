using UnityEngine;

public abstract class RewardBase : ScriptableObject
{
    public Sprite Icon;
    public string RewardName;
    [TextArea] public string Description;

    [Header("Inventory Settings")]
    public bool isItem; //이거 true면 인벤토리 들어가게 할거임

    public abstract void Apply(PlayerUnit player);
}
