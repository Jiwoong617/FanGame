using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TooltipUI : UI_Base
{
    public static TooltipUI Instance { get; private set; }

    enum Texts
    {
        NameText,
        DescText,
        FlavorText
    }
    enum Images
    {
        IconImage
    }

    private TMP_Text nameText;
    private TMP_Text descText;
    private TMP_Text flavorText;
    private Image icon;

    RectTransform rectTransform;
    Canvas parentCanvas;
    RectTransform canvasRect;

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
        Bind<Image>(typeof(Images));
        nameText = Get<TMP_Text>(Texts.NameText);
        descText = Get<TMP_Text>(Texts.DescText);
        flavorText = Get<TMP_Text>(Texts.FlavorText);
        icon = Get<Image>(Images.IconImage);

        rectTransform = GetComponent<RectTransform>();
        parentCanvas = GetComponentInParent<Canvas>();
        canvasRect = parentCanvas.GetComponent<RectTransform>();
        Hide();
    }

    public void ShowTooltip(string itemName, string desc, string flavor, Sprite iconImage, Vector3 slotPosition)
    {
        nameText.text = itemName;
        descText.text = desc;
        flavorText.text = flavor;
        icon.sprite = iconImage;

        transform.position = slotPosition;
        Show();

        UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
        if (parentCanvas != null)
        {
            Vector3[] corners = new Vector3[4];
            rectTransform.GetWorldCorners(corners);
            
            Vector3[] canvasCorners = new Vector3[4];
            canvasRect.GetWorldCorners(canvasCorners);

            float minX = corners[0].x;
            float minY = corners[0].y;
            float maxX = corners[2].x;
            float maxY = corners[1].y;

            float cMinX = canvasCorners[0].x;
            float cMinY = canvasCorners[0].y;
            float cMaxX = canvasCorners[2].x;
            float cMaxY = canvasCorners[1].y;

            Vector3 newPos = transform.position;

            if (minX < cMinX) newPos.x += (cMinX - minX);
            else if (maxX > cMaxX) newPos.x -= (maxX - cMaxX);

            if (minY < cMinY) newPos.y += (cMinY - minY);
            else if (maxY > cMaxY) newPos.y -= (maxY - cMaxY);

            transform.position = newPos;
        }
    }

    public void HideTooltip()
    {
        Hide();
    }
}