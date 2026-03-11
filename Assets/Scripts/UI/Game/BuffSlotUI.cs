using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class BuffSlotUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text stackText;

    private StatusEffect currentEffect;

    // TODO : 초단위 일때 보여주는거 추가 했음 맘에 안들면 삭제
    private void Update()
    {
        if (currentEffect == null || currentEffect.IsFinished) return;

        if (!currentEffect.isPermanent && currentEffect.duration > 0)
        {
            stackText.gameObject.SetActive(true);
            stackText.text = currentEffect.duration.ToString("F1");
        }
    }


    public void Init(StatusEffect effect)
    {
        if(iconImage == null)
            iconImage = GetComponent<Image>();
        if(stackText == null)
            stackText = GetComponentInChildren<TMP_Text>();

        currentEffect = effect;

        Sprite loadedIcon = GameManager.SpriteData.GetSprite(effect.effectType, "Icons/Buffs");
        if (loadedIcon != null)
        {
            iconImage.sprite = loadedIcon;
        }

        UpdateSlot();
    }

    public void UpdateSlot()
    {
        if (currentEffect == null) return;

        if (currentEffect.stacks > 1)
        {
            stackText.text = currentEffect.stacks.ToString();
            stackText.gameObject.SetActive(true);
        }
        else
        {
            stackText.gameObject.SetActive(false);
        }
    }

    public StatusEffect GetEffect() => currentEffect;
}