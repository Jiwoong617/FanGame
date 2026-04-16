using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class TutorialUI : UI_Base
{
    private static TutorialUI instance;
    public static TutorialUI Instance { get { return instance; } }
    public bool IsVisible => canvas != null && canvas.enabled;

    // 추가된 컴포넌트들을 enum에 반영
    enum Buttons { CloseButton, NextButton, PrevButton }
    enum Images { BlockPanel, Background, TutorialImage }
    enum Texts { TutorialPageText }

    [Header("Tutorial Resources")]
    [SerializeField] private List<Sprite> tutorialSprites = new List<Sprite>();
    private int currentIdx = 0;

    private Canvas canvas;
    private GraphicRaycaster raycaster;
    private RectTransform BG;

    [Header("Animation Settings")]
    [SerializeField] private float animDuration = 0.5f;
    [SerializeField] private float startYOffset = -500f;

    protected override void Init()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        canvas = GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 55;

        raycaster = GetComponent<GraphicRaycaster>();

        CanvasScaler scaler = GetComponent<CanvasScaler>();
        if (scaler == null) scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        // 바인딩
        Bind<Button>(typeof(Buttons));
        Bind<Image>(typeof(Images));
        Bind<TMP_Text>(typeof(Texts));

        // 버튼 리스너 등록
        Get<Button>(Buttons.CloseButton).onClick.AddListener(Hide);
        Get<Button>(Buttons.NextButton).onClick.AddListener(OnClickNext);
        Get<Button>(Buttons.PrevButton).onClick.AddListener(OnClickPrev);

        BG = Get<Image>(Images.Background).rectTransform;

        if (canvas != null) canvas.enabled = false;
        if (raycaster != null) raycaster.enabled = false;
    }

    public override void Show()
    {
        if (canvas != null) canvas.enabled = true;
        if (raycaster != null) raycaster.enabled = true;

        // 열릴 때마다 첫 페이지로 초기화
        currentIdx = 0;
        UpdatePage();

        if (BG != null)
        {
            BG.anchoredPosition = new Vector2(0, startYOffset);
            BG.localScale = new Vector3(0.05f, 1f, 1f);

            BG.DOKill();

            Sequence seq = DOTween.Sequence();
            seq.SetUpdate(true);

            seq.Append(BG.DOAnchorPosY(0, animDuration * 0.6f).SetEase(Ease.OutBack));
            seq.Append(BG.DOScaleX(1f, animDuration * 0.4f).SetEase(Ease.OutBack));
        }
    }

    public override void Hide()
    {
        if (BG != null)
        {
            BG.DOKill();

            Sequence seq = DOTween.Sequence();
            seq.SetUpdate(true);

            seq.Append(BG.DOScaleX(0.05f, animDuration * 0.4f).SetEase(Ease.InBack));
            seq.Append(BG.DOAnchorPosY(startYOffset, animDuration * 0.6f).SetEase(Ease.InBack));

            seq.OnComplete(() => {
                if (canvas != null) canvas.enabled = false;
                if (raycaster != null) raycaster.enabled = false;
            });
        }
        else
        {
            if (canvas != null) canvas.enabled = false;
            if (raycaster != null) raycaster.enabled = false;
        }
    }

    public void Toggle()
    {
        if (canvas.enabled)
            Hide();
        else
            Show();
    }

    #region 페이지 로직
    private void OnClickNext()
    {
        if (currentIdx < tutorialSprites.Count - 1)
        {
            currentIdx++;
            UpdatePage();
        }
    }

    private void OnClickPrev()
    {
        if (currentIdx > 0)
        {
            currentIdx--;
            UpdatePage();
        }
    }

    private void UpdatePage()
    {
        if (tutorialSprites.Count == 0) return;

        // 이미지 교체
        Get<Image>(Images.TutorialImage).sprite = tutorialSprites[currentIdx];

        // 페이지 텍스트 업데이트 (1부터 시작 / 전체 수)
        Get<TMP_Text>(Texts.TutorialPageText).text = $"{currentIdx + 1} / {tutorialSprites.Count}";

        // 버튼 활성/비활성 처리
        Get<Button>(Buttons.PrevButton).interactable = (currentIdx > 0);
        Get<Button>(Buttons.NextButton).interactable = (currentIdx < tutorialSprites.Count - 1);
    }
    #endregion
}