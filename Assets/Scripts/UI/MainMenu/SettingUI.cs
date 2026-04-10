using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using DG.Tweening;

public class SettingUI : UI_Base
{
    private static SettingUI instance;
    public static SettingUI Instance { get { return instance; } }
    public bool IsVisible => canvas != null && canvas.enabled;  

    enum Sliders { MasterVolumeSlider, BGMVolumeSlider, SFXVolumeSlider }
    enum Dropdowns { WindowModeDropdown }
    enum Buttons { CloseButton }
    enum Images { BlockPanel, Background }

    private Canvas canvas;
    private GraphicRaycaster raycaster;
    private RectTransform BG;
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
        canvas.sortingOrder = 50;

        raycaster = GetComponent<GraphicRaycaster>();

        CanvasScaler scaler = GetComponent<CanvasScaler>();
        if (scaler == null) scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        Bind<Slider>(typeof(Sliders));
        Bind<TMP_Dropdown>(typeof(Dropdowns));
        Bind<Button>(typeof(Buttons));
        Bind<Image>(typeof(Images));

        Get<Button>(Buttons.CloseButton).onClick.AddListener(Hide);
        Get<Slider>(Sliders.MasterVolumeSlider).onValueChanged.AddListener(OnMasterChanged);
        Get<Slider>(Sliders.BGMVolumeSlider).onValueChanged.AddListener(OnBGMChanged);
        Get<Slider>(Sliders.SFXVolumeSlider).onValueChanged.AddListener(OnSFXChanged);

        InitWindowModeDropdown();
        Get<TMP_Dropdown>(Dropdowns.WindowModeDropdown).onValueChanged.AddListener(OnWindowModeChanged);
        BG = Get<Image>(Images.Background).rectTransform;

        if (canvas != null) canvas.enabled = false;
        if (raycaster != null) raycaster.enabled = false;
    }

    public override void Show()
    {
        if (canvas != null) canvas.enabled = true;
        if (raycaster != null) raycaster.enabled = true;

        if (BG != null)
        {
            BG.anchoredPosition = new Vector2(0, startYOffset);
            BG.localScale = new Vector3(0.05f, 1f, 1f); // 가로로 납작하게

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
            seq.SetUpdate(true); // timescale 0 이어도 작동함

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
            Time.timeScale = 1f;
        }
    }

    public void Toggle()
    {
        if (canvas.enabled)
            Hide();
        else
        {
            Show();
            RefreshUI();
        }
    }

    private void RefreshUI()
    {
        if (GameManager.Sound != null)
        {
            Get<Slider>(Sliders.MasterVolumeSlider).value = GameManager.Sound.MasterVolume;
            Get<Slider>(Sliders.BGMVolumeSlider).value = GameManager.Sound.BGMVolume;
            Get<Slider>(Sliders.SFXVolumeSlider).value = GameManager.Sound.SFXVolume;
        }
        int modeIndex = PlayerPrefs.GetInt("WindowMode", 0);
        Get<TMP_Dropdown>(Dropdowns.WindowModeDropdown).value = modeIndex;
    }

    private void InitWindowModeDropdown()
    {
        var dropdown = Get<TMP_Dropdown>(Dropdowns.WindowModeDropdown);
        dropdown.ClearOptions();
        List<string> options = new List<string> { "전체 화면", "창 모드", "전체 창 모드" };
        dropdown.AddOptions(options);
    }

    private void OnMasterChanged(float val) => GameManager.Sound?.SetMasterVolume(val);
    private void OnBGMChanged(float val) => GameManager.Sound?.SetBGMVolume(val);
    private void OnSFXChanged(float val) => GameManager.Sound?.SetSFXVolume(val);
    private void OnWindowModeChanged(int index)
    {
        FullScreenMode mode = FullScreenMode.ExclusiveFullScreen;
        switch (index)
        {
            case 0: mode = FullScreenMode.ExclusiveFullScreen; break;
            case 1: mode = FullScreenMode.Windowed; break;
            case 2: mode = FullScreenMode.FullScreenWindow; break;
        }
        Screen.SetResolution(1920, 1080, mode);
        PlayerPrefs.SetInt("WindowMode", index);
    }
}