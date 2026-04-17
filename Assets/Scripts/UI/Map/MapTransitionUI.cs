using DG.Tweening;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
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

    private enum Texts
    {
        BattleStartText
    }

    private List<RectTransform> bars = new List<RectTransform>();
    private TMP_Text battleStartText;

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
        Bind<TMP_Text>(typeof(Texts));

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
        battleStartText = Get<TMP_Text>(Texts.BattleStartText);
        battleStartText.gameObject.SetActive(false);
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

            outSeq.OnComplete(() =>
            {
                if(GameManager.Instance.State == GameState.Battle)
                {
                    PlayBattleStartEffect(GameManager.Battle.StartProcessing);
                }
                else
                    gameObject.SetActive(false);
            });
        });

        float randPitch = UnityEngine.Random.Range(0.9f, 1.2f);
        GameManager.Sound.PlaySFX(SFX.ScreenTransition, randPitch);
    }

    public void PlayBattleStartEffect(Action onEffectComplete)
    {
        if (battleStartText == null)
        {
            battleStartText.gameObject.SetActive(false);
            gameObject.SetActive(false);
            onEffectComplete?.Invoke();
            return;
        }

        battleStartText.gameObject.SetActive(true);
        battleStartText.alpha = 0;
        battleStartText.transform.localScale = Vector3.one * 0.5f;

        Sequence seq = DOTween.Sequence();
        // 등장
        seq.Append(battleStartText.DOFade(1f, 0.4f));
        seq.Join(battleStartText.transform.DOScale(1.2f, 0.4f).SetEase(Ease.OutBack));

        // 대기
        seq.AppendInterval(0.6f);

        // 퇴장
        seq.Append(battleStartText.DOFade(0f, 0.3f));
        seq.Join(battleStartText.transform.DOScale(1.5f, 0.3f));

        seq.OnComplete(() =>
        {
            battleStartText.gameObject.SetActive(false);
            gameObject.SetActive(false);
            onEffectComplete?.Invoke();
        });
    }
}