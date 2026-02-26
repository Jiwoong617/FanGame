using System.Collections.Generic;
using UnityEngine;
public enum StatType
{
    MaxHP,
    Defense,
    AttackDamage,
    AttackSpeed,
    Stamina,
    StaminaRegen,
    MaxFp,
    SkillCoolTime,
    DodgeCost,
    ParryCost
}

[System.Serializable]
public class StatChange
{
    public StatType targetStat;
    public StatModType modType;
    public float amount;
}

[CreateAssetMenu(fileName = "Reward", menuName = "Scriptable Objects/Reward")]
public class RewardData : ScriptableObject
{
    public Sprite Icon;
    public string RewardName;
    [TextArea] public string Description;
    [TextArea] public string FlavorText;

    [Header("Inventory Settings")]
    public bool isItem; //이거 true면 인벤토리 들어가게 할거임

    public List<StatChange> statChanges = new List<StatChange>();
    [SerializeReference, SerializeReferenceDropdown]
    public List<Ability> abilities = new List<Ability>();

    public void Apply(PlayerUnit player)
    {
        if (player == null) return;

        PlayerStats stats = player.GetStat<PlayerStats>();
        foreach (var stat in statChanges)
        {
            if (stats == null) return;

            StatModifier mod = new StatModifier(stat.amount, stat.modType);
            switch (stat.targetStat)
            {
                case StatType.MaxHP:
                    float oldMaxHp = stats.maxHp.GetValue();
                    stats.maxHp.AddModifier(mod);

                    // 증가 후 값 확인하여 차이만큼 현재 체력 회복
                    float newMaxHp = stats.maxHp.GetValue();
                    if (newMaxHp > oldMaxHp)
                        stats.hp += (newMaxHp - oldMaxHp);
                    break;

                case StatType.AttackDamage:
                    stats.attackDamage.AddModifier(mod);
                    break;

                case StatType.Defense:
                    stats.defense.AddModifier(mod);
                    break;

                case StatType.AttackSpeed:
                    stats.attackSpeed.AddModifier(mod);
                    break;

                case StatType.Stamina:
                    stats.maxStamina.AddModifier(mod);
                    break;

                case StatType.StaminaRegen:
                    stats.staminaRegen.AddModifier(mod);
                    break;

                case StatType.MaxFp:
                    float oldFp = stats.maxFp.GetValue();
                    stats.maxFp.AddModifier(mod);

                    float newFp = stats.maxFp.GetValue();
                    if (newFp > oldFp)
                        stats.fp += (newFp - oldFp);
                    break;

                case StatType.SkillCoolTime:
                    stats.skillCoolTime.AddModifier(mod);
                    break;

                case StatType.DodgeCost:
                    stats.dodgeCost.AddModifier(mod);
                    break;

                case StatType.ParryCost:
                    stats.parryCost.AddModifier(mod);
                    break;
            }
        }

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
