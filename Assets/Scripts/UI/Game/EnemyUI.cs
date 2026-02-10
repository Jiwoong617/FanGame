using UnityEngine;
using UnityEngine.UI;

public class EnemyUI : UI_Base
{
    enum Images
    {
        ActionImage,
    }

    enum Sliders
    {
        HpBar,
        ActionBar
    }

    private Image ActionImage;
    private Slider HpBar;
    private Slider ActionBar;

    protected override void Init()
    {
        Bind<Slider>(typeof(Sliders));
        Bind<Image>(typeof(Images));

        ActionImage = Get<Image>(Images.ActionImage);
        HpBar = Get<Slider>(Sliders.HpBar);
        ActionBar = Get<Slider>(Sliders.ActionBar);
    }

    public void UpdateHp(float current, float max)
    {
        HpBar.value = max > 0 ? current / max : 0;
    }

    public void UpdateActionBar(float progress)
    {
        if (ActionBar == null) return;
        ActionBar.value = progress;
    }

    public void SetIntentIcon(Sprite sprite)
    {
        if (sprite != null)
        {
            ActionImage.sprite = sprite;
            ActionImage.enabled = true;
        }
        else
        {
            ActionImage.enabled = false;
        }
    }
}
