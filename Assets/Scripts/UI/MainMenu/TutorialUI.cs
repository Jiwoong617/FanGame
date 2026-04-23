using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

[System.Serializable]
public class TutorialSection
{
    public string sectionName;
    public List<Sprite> sprites = new List<Sprite>();
}

public class TutorialUI : UI_Base
{
    private static TutorialUI instance;
    public static TutorialUI Instance { get { return instance; } }
    public bool IsVisible => canvas != null && canvas.enabled;

    enum Buttons { CloseButton, NextButton, PrevButton }
    enum Images { BlockPanel, Background, TutorialImage }
    enum Texts { TutorialPageText }
    enum Layouts { TutorialSection }

    [Header("Tutorial Resources")]
    [SerializeField] private List<TutorialSection> sections = new List<TutorialSection>();
    [SerializeField] private Button sectionButtonPrefab;
    [SerializeField] private Sprite commonSectionButtonSprite;

    private int currentSectionIdx = 0;
    private int currentSpriteIdx = 0;
    private List<Button> sectionButtons = new List<Button>();

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
        Bind<VerticalLayoutGroup>(typeof(Layouts));

        // 버튼 리스너 등록
        Get<Button>(Buttons.CloseButton).onClick.AddListener(Hide);
        Get<Button>(Buttons.NextButton).onClick.AddListener(OnClickNext);
        Get<Button>(Buttons.PrevButton).onClick.AddListener(OnClickPrev);

        // --- 공통 버튼 컬러 설정 (Disabled 시 알파 200) ---
        ColorBlock commonColors = ColorBlock.defaultColorBlock;
        commonColors.disabledColor = new Color(1f, 1f, 1f, 200f / 255f);
        commonColors.fadeDuration = 0.1f; // 자연스러운 전환을 위해 추가

        // 하단 조작 버튼에도 적용
        Get<Button>(Buttons.NextButton).colors = commonColors;
        Get<Button>(Buttons.PrevButton).colors = commonColors;

        BG = Get<Image>(Images.Background).rectTransform;

        // 섹션 버튼 동적 생성 및 설정
        Transform TutorialSectionParent = Get<VerticalLayoutGroup>(Layouts.TutorialSection).transform;
        for (int i = 0; i < sections.Count; i++)
        {
            int idx = i;
            Button btn = Instantiate(sectionButtonPrefab, TutorialSectionParent);

            // 텍스트 및 스프라이트 설정
            var btnText = btn.GetComponentInChildren<TMP_Text>();
            if (btnText != null) btnText.text = sections[i].sectionName;

            if (commonSectionButtonSprite != null)
                btn.image.sprite = commonSectionButtonSprite;

            // 컬러 블록 적용 (생성 시 한 번만 수행)
            btn.colors = commonColors;

            btn.onClick.AddListener(() => OnClickSection(idx));
            sectionButtons.Add(btn);
        }

        if (canvas != null) canvas.enabled = false;
        if (raycaster != null) raycaster.enabled = false;
    }

    public override void Show()
    {
        if (canvas != null) canvas.enabled = true;
        if (raycaster != null) raycaster.enabled = true;

        currentSectionIdx = 0;
        currentSpriteIdx = 0;
        UpdatePage();
        HighlightSectionButton(0);

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
        if (canvas.enabled) Hide();
        else Show();
    }

    #region 섹션 선택
    private void OnClickSection(int idx)
    {
        currentSectionIdx = idx;
        currentSpriteIdx = 0;
        UpdatePage();
        HighlightSectionButton(idx);
    }

    private void HighlightSectionButton(int idx)
    {
        for (int i = 0; i < sectionButtons.Count; i++)
        {
            // 이제 interactable만 조절하면 유니티가 미리 설정한 ColorBlock의 disabledColor를 적용합니다.
            sectionButtons[i].interactable = (i != idx);
        }
    }
    #endregion

    #region 페이지 조작
    private void OnClickNext()
    {
        var sprites = sections[currentSectionIdx].sprites;
        if (currentSpriteIdx < sprites.Count - 1)
        {
            currentSpriteIdx++;
            UpdatePage();
        }
    }

    private void OnClickPrev()
    {
        if (currentSpriteIdx > 0)
        {
            currentSpriteIdx--;
            UpdatePage();
        }
    }

    private void UpdatePage()
    {
        if (sections.Count == 0) return;
        var sprites = sections[currentSectionIdx].sprites;
        if (sprites.Count == 0) return;

        Get<Image>(Images.TutorialImage).sprite = sprites[currentSpriteIdx];
        Get<TMP_Text>(Texts.TutorialPageText).text = $"{currentSpriteIdx + 1} / {sprites.Count}";

        // 버튼 비활성화 시 설정해둔 알파값이 자동 적용됩니다.
        Get<Button>(Buttons.PrevButton).interactable = (currentSpriteIdx > 0);
        Get<Button>(Buttons.NextButton).interactable = (currentSpriteIdx < sprites.Count - 1);
    }
    #endregion
}