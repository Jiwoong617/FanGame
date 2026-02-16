using System;
using System.Collections.Generic;
using UnityEngine;

public class CooldownUI : UI_Base
{
    [Header("UI Setup")]
    [SerializeField] private GameObject cooldownSlotPrefab;

    private Dictionary<ActionType, CooldownSlotUI> slotDictionary = new Dictionary<ActionType, CooldownSlotUI>();
    private PlayerUnit player;

    protected override void Init() { }

    private void Start()
    {
        player = GameManager.Instance.Player;
        if (player != null)
        {
            InitializeSlots();
            player.OnCooldownTriggered += HandleCooldownTriggered;
        }
    }

    private void OnDestroy()
    {
        if (player != null)
        {
            player.OnCooldownTriggered -= HandleCooldownTriggered;
        }
    }

    private void InitializeSlots()
    {
        foreach (Transform child in transform)
            Destroy(child.gameObject);

        slotDictionary.Clear();

        foreach (ActiveAbility ability in player.GetActiveAbilities())
        {
            GameObject slotObj = Instantiate(cooldownSlotPrefab, transform);
            CooldownSlotUI slotUI = slotObj.GetComponent<CooldownSlotUI>();

            slotUI.StartCooldown(ability.skillIcon, 0f);
            slotDictionary.Add(ability.actionType, slotUI);
        }
    }

    private void HandleCooldownTriggered(ActionType type, float duration)
    {
        if (slotDictionary.TryGetValue(type, out CooldownSlotUI slotUI))
        {
            slotUI.StartCooldown(null, duration);
        }
    }
}
