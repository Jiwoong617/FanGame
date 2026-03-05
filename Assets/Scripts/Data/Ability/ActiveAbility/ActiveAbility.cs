using UnityEngine;

public enum ActionType
{
    None,
    Dodge,
    Parry,
    Skill,
}

[System.Serializable]
public abstract class ActiveAbility : Ability
{
    [Header("Action Setting")]
    public ActionType actionType;
    public Sprite skillIcon;

    protected float currentCooldown = 0f;

    public override void OnUpdate(float delta)
    {
        if (currentCooldown > 0)
        {
            currentCooldown -= delta;
        }
    }

    public bool TryUseSkill()
    {
        PlayerUnit player = owner as PlayerUnit;
        if (player == null) return false;

        if (currentCooldown > 0) return false;
        if (!CheckAndConsumeCost(player)) return false;

        ExecuteSkill(player);

        currentCooldown = GetCooldown(player);
        player.NotifyCooldownStarted(actionType, currentCooldown);

        return true;
    }

    public bool IsOnCooldown() => currentCooldown > 0;
    public void ResetCooldown() => currentCooldown = 0f;

    protected abstract bool CheckAndConsumeCost(PlayerUnit player);
    protected abstract float GetCooldown(PlayerUnit player);
    protected abstract void ExecuteSkill(PlayerUnit player);

    public void ExecuteFree()
    {
        PlayerUnit player = owner as PlayerUnit;
        if (player != null)
            ExecuteSkill(player);
    }
}