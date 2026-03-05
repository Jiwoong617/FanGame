using TMPro;
using UnityEngine;

public class TooltipUI : UI_Base
{
    public static TooltipUI Instance { get; private set; }

    enum Texts
    {
        NameText,
        DescText,
        FlavorText
    }

    private TMP_Text nameText;
    private TMP_Text descText;
    private TMP_Text flavorText;

    protected override void Init()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        Bind<TMP_Text>(typeof(Texts));
        nameText = Get<TMP_Text>(Texts.NameText);
        descText = Get<TMP_Text>(Texts.DescText);
        flavorText = Get<TMP_Text>(Texts.FlavorText);
        Hide();
    }

    public void ShowTooltip(string itemName, string desc, string flavor, Vector3 slotPosition)
    {
        nameText.text = itemName;
        descText.text = desc;
        flavorText.text = flavor;

        transform.position = slotPosition;
        Show();
    }

    public void HideTooltip()
    {
        Hide();
    }
}