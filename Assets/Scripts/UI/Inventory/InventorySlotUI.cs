using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventorySlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private RewardData itemData;
    private Image icon;
    public RewardData ItemData => itemData;

    public void Init(RewardData item)
    {
        if (icon == null)
            icon = GetComponent<Image>();

        itemData = item;

        icon.sprite = item.Icon;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (itemData != null && TooltipUI.Instance != null)
        {
            TooltipUI.Instance.ShowTooltip(itemData.RewardName, itemData.Description, itemData.FlavorText,
                itemData.Icon, transform.position);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (TooltipUI.Instance != null)
            TooltipUI.Instance.HideTooltip();
    }
}