using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class BuffSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text stackText;

    private StatusEffect currentStatusEffect;
    private PassiveAbility currentPassiveAbility;

    private void Update()
    {
        if (currentStatusEffect == null || currentStatusEffect.IsFinished) return;

        if (!currentStatusEffect.isPermanent && currentStatusEffect.duration > 0)
        {
            stackText.gameObject.SetActive(true);
            stackText.text = currentStatusEffect.duration.ToString("F0");
        }
    }


    public void Init(StatusEffect effect)
    {
        if(iconImage == null)
            iconImage = GetComponent<Image>();
        if(stackText == null)
            stackText = GetComponentInChildren<TMP_Text>();

        iconImage.sprite = null;

        currentStatusEffect = effect;
        currentPassiveAbility = null;

        Sprite loadedIcon = GameManager.SpriteData.GetSprite(effect.effectType, "Icons/Buffs");
        if (loadedIcon != null)
        {
            iconImage.sprite = loadedIcon;
        }

        UpdateSlot();
    }

    // 패시브 전용
    public void Init(PassiveAbility passive)
    {
        if (iconImage == null)
            iconImage = GetComponent<Image>();
        if (stackText == null)
            stackText = GetComponentInChildren<TMP_Text>();

        currentPassiveAbility = passive;
        currentStatusEffect = null;

        iconImage.sprite = null;

        if (passive.passiveIcon != null)
        {
            iconImage.sprite = passive.passiveIcon;
        }

        stackText.gameObject.SetActive(false);
    }

    public void UpdateSlot()
    {
        if (currentStatusEffect == null) return;

        if (currentStatusEffect.stacks > 1)
        {
            stackText.text = currentStatusEffect.stacks.ToString("F0");
            stackText.gameObject.SetActive(true);
        }
        else if (currentStatusEffect.isPermanent)
        {
            stackText.gameObject.SetActive(false);
        }
    }

    public StatusEffect GetEffect() => currentStatusEffect;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if ((currentStatusEffect == null && currentPassiveAbility == null) || SimpleTooltipUI.Instance == null) return;

        string name = "";
        string desc = "";

        // 직관적인 null 체크로 분기 처리
        if (currentStatusEffect != null)
        {
            var tooltipData = BuffSlotTooltipDB.GetStatusTooltip(currentStatusEffect.effectType);
            name = tooltipData.name;
            desc = tooltipData.desc;
        }
        else if (currentPassiveAbility != null)
        {
            name = currentPassiveAbility.passiveName;
            desc = currentPassiveAbility.passiveDescription;
        }

        SimpleTooltipUI.Instance.ShowTooltip(name, desc);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (SimpleTooltipUI.Instance != null)
            SimpleTooltipUI.Instance.Hide();
    }

    private void OnDisable()
    {
        if (SimpleTooltipUI.Instance != null)
            SimpleTooltipUI.Instance.Hide();
    }
}