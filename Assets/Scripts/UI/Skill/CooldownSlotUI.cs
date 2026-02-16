using UnityEngine;
using UnityEngine.UI;

public class CooldownSlotUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private Image overlayImage;

    private float currentTimer;
    private float maxCooldown;

    public void StartCooldown(Sprite icon, float duration)
    {
        if (iconImage != null && icon != null)
            iconImage.sprite = icon;

        if (duration <= 0f)
        {
            gameObject.SetActive(false);
            return;
        }

        maxCooldown = duration;
        currentTimer = duration;

        overlayImage.fillAmount = 1f;
        gameObject.SetActive(true);
        transform.SetAsLastSibling();
    }

    private void Update()
    {
        if (currentTimer > 0)
        {
            currentTimer -= Time.deltaTime;

            overlayImage.fillAmount = currentTimer / maxCooldown;
            if (currentTimer <= 0)
                gameObject.SetActive(false);
        }
    }
}