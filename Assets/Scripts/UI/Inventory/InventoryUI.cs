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

    protected override void Init()
    {
        Bind<GridLayoutGroup>(typeof(Grid));

        slotContainer = Get<GridLayoutGroup>(Grid.SlotContainer).transform;

        GameManager.Inventory.OnItemAdded += AddItemSlot;

        Hide();
    }

    private void OnDestroy()
    {
        if (GameManager.Inventory != null)
        {
            GameManager.Inventory.OnItemAdded -= AddItemSlot;
        }
    }

    private void AddItemSlot(RewardData item)
    {
        GameObject go = Instantiate(slotPrefab, slotContainer);
        InventorySlotUI slotUI = go.GetComponent<InventorySlotUI>();

        slotUI.Init(item);
    }
}