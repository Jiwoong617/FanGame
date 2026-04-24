using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class SimpleTooltipUI : UI_Base
{
    public static SimpleTooltipUI Instance { get; private set; }

    enum Texts
    {
        TitleText,
        DescriptionText
    }

    enum RectTransforms
    {
        BackgroundRect
    }

    private Canvas myCanvas;

    protected override void Init()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        myCanvas = GetComponent<Canvas>();

        Bind<TMP_Text>(typeof(Texts));
        Bind<RectTransform>(typeof(RectTransforms));

        Hide();
    }


    private void UpdatePosition()
    {
        RectTransform bgRect = Get<RectTransform>(RectTransforms.BackgroundRect);
        if (bgRect == null || myCanvas == null) return;

        Vector2 localPoint;
        Camera eventCamera = myCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : myCanvas.worldCamera;

        Vector2 screenPosition = Pointer.current != null ? Pointer.current.position.ReadValue() : Vector2.zero;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            myCanvas.transform as RectTransform,
            screenPosition,
            eventCamera,
            out localPoint);

        Vector2 offset = new Vector2(5f, -5f);
        Vector2 tooltipPos = localPoint + offset;

        // 캔버스 RectTransform 기준으로 툴팁이 화면 밖으로 나가지 않도록 클램프
        RectTransform canvasRect = myCanvas.transform as RectTransform;
        Vector2 half = canvasRect.rect.size * 0.5f;
        Vector2 tooltipSize = bgRect.rect.size;

        float minX = -half.x;
        float maxX =  half.x - tooltipSize.x;
        float minY = -half.y + tooltipSize.y;
        float maxY =  half.y;

        tooltipPos.x = Mathf.Clamp(tooltipPos.x, minX, maxX);
        tooltipPos.y = Mathf.Clamp(tooltipPos.y, minY, maxY);

        bgRect.localPosition = tooltipPos;
    }

    public void ShowTooltip(string title, string description)
    {
        Show();

        TMP_Text titleText = Get<TMP_Text>(Texts.TitleText);
        TMP_Text descText = Get<TMP_Text>(Texts.DescriptionText);
        RectTransform bgRect = Get<RectTransform>(RectTransforms.BackgroundRect);

        if (titleText != null && descText != null)
        {
            if (titleText.text != title || descText.text != description)
            {
                titleText.text = title;
                descText.text = description;

                if (bgRect != null)
                    LayoutRebuilder.ForceRebuildLayoutImmediate(bgRect);
            }
        }

        UpdatePosition();
    }
}