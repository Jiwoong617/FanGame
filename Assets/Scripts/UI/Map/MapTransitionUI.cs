using UnityEngine;
using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine.UI;

public class MapTransitionUI : UI_Base
{
    private enum Images
    {
        Bar1,
        Bar2,
        Bar3,
        Bar4,
        Bar5,
    }

    private List<RectTransform> bars = new List<RectTransform>();

    [Header("Settings")]
    [SerializeField] private float moveDuration = 0.4f;
    [SerializeField] private float staggerDelay = 0.1f;

    private float screenHeight;

    protected override void Init()
    {
        Canvas canvas = GetComponent<Canvas>();
        if (canvas != null && canvas.worldCamera == null)
            canvas.worldCamera = Camera.main;

        Bind<Image>(typeof(Images));

        bars.Clear();
        foreach (Images imgEnum in Enum.GetValues(typeof(Images)))
        {
            Image barImage = Get<Image>(imgEnum);
            if (barImage != null)
            {
                bars.Add(barImage.rectTransform);
            }
        }

        GameManager.Map.SetMapTransitionUI(this);

        screenHeight = GetComponent<RectTransform>().rect.height;
        gameObject.SetActive(false);
    }

    public void PlayTransition(Action onScreenCovered)
    {
        gameObject.SetActive(true);
        foreach (var bar in bars)
        {
            bar.anchoredPosition = new Vector2(bar.anchoredPosition.x, -screenHeight);
        }

        // 일단 계단식으로 올리기
        Sequence seq = DOTween.Sequence();
        for (int i = 0; i < bars.Count; i++)
        {
            seq.Insert(i * staggerDelay, bars[i].DOAnchorPosY(0, moveDuration).SetEase(Ease.OutQuart));
        }

        //싹다 까매졌으면 맵 변경 수행 및 다시 올리기
        seq.OnComplete(() =>
        {
            onScreenCovered?.Invoke();

            Sequence outSeq = DOTween.Sequence();
            for (int i = 0; i < bars.Count; i++)
            {
                outSeq.Insert(i * staggerDelay, bars[i].DOAnchorPosY(screenHeight, moveDuration).SetEase(Ease.InQuart));
            }

            outSeq.OnComplete(() => gameObject.SetActive(false));
        });
    }
}