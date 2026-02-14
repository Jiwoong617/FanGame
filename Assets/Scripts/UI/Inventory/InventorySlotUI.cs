using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventorySlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private RewardBase itemData;
    private Image icon;

    public void Init(RewardBase item)
    {
        icon = GetComponent<Image>();

        itemData = item;

        icon.sprite = item.Icon;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (itemData != null && TooltipUI.Instance != null)
        {
            TooltipUI.Instance.ShowTooltip(itemData.RewardName, itemData.Description, transform.position);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (TooltipUI.Instance != null)
            TooltipUI.Instance.HideTooltip();
    }
}