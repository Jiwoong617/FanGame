using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : UI_Base
{
    enum Grid
    {
        SlotContainer
    }

    [SerializeField] private GameObject slotPrefab;

    private Transform slotContainer;
    private List<InventorySlotUI> activeSlots = new List<InventorySlotUI>();

    protected override void Init()
    {
        Bind<GridLayoutGroup>(typeof(Grid));

        slotContainer = Get<GridLayoutGroup>(Grid.SlotContainer).transform;

        if (GameManager.Inventory != null)
        {
            GameManager.Inventory.OnItemAdded -= AddItemSlot;
            GameManager.Inventory.OnItemAdded += AddItemSlot;
            GameManager.Inventory.OnItemRemoved -= RemoveItemSlot;
            GameManager.Inventory.OnItemRemoved += RemoveItemSlot;
        }

        Hide();
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null && GameManager.Inventory != null)
        {
            GameManager.Inventory.OnItemAdded -= AddItemSlot;
            GameManager.Inventory.OnItemRemoved -= RemoveItemSlot;
        }
    }

    private void AddItemSlot(RewardData item)
    {
        GameObject go = Instantiate(slotPrefab, slotContainer);
        InventorySlotUI slotUI = go.GetComponent<InventorySlotUI>();

        slotUI.Init(item);
        activeSlots.Add(slotUI);
    }

    private void RemoveItemSlot(RewardData item)
    {
        InventorySlotUI targetSlot = null;

        foreach (var slot in activeSlots)
        {
            if (slot.ItemData == item)
            {
                targetSlot = slot;
                break;
            }
        }

        if (targetSlot != null)
        {
            activeSlots.Remove(targetSlot);
            Destroy(targetSlot.gameObject);
        }
    }
}