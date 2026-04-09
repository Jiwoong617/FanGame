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
            screenPosition, // 변경된 부분
            eventCamera,
            out localPoint);

        bgRect.localPosition = localPoint + new Vector2(15f, -15f);
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