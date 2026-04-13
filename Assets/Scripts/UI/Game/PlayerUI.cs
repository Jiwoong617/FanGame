using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.EventSystems; // 호버 이벤트를 위해 추가
using DG.Tweening;

public class PlayerUI : UI_Base
{
    #region enums
    enum Texts
    {
        HpText,
        StaminaText,
        FpText,
        Attack,
        AttackSpeed,
        Defense,
        StRegen,
        CritChance,
        CritDamage,
        SkillName,         // 추가: 스킬 이름 텍스트
        SkillDescription   // 추가: 스킬 설명 텍스트
    }

    enum Sliders
    {
        HpBar,
        StaminaBar,
        FpBar,
    }

    enum Images
    {
        Icon,
        PlayerPanel,
        InventoryPanel,
        CharacterSkill_Info, // 추가: 스킬 정보 패널 (켜고 끌 대상)
        SkillIcon            // 추가: 스킬 아이콘 이미지
    }
    #endregion

    private PlayerUnit Player;

    private Slider HpBar;
    private Slider StaminaBar;
    private Slider FpBar;
    private TMP_Text HpText;
    private TMP_Text StaminaText;
    private TMP_Text FpText;
    private TMP_Text CritChanceText;
    private TMP_Text CritDamageText;

    private TMP_Text AttackText;
    private TMP_Text AttackSpeedText;
    private TMP_Text DefenseText;
    private TMP_Text StRegenText;

    private GameObject playerPanelObj;
    private GameObject inventoryPanelObj;
    private RectTransform playerRect;
    private RectTransform inventoryRect;
    private CanvasGroup playerCG;
    private CanvasGroup inventoryCG;

    private bool isShowingPlayer = true;
    private Vector2 originalPos;
    private float slideOffset = 200f;
    private float animDuration = 0.3f; // 전환 속도

    protected override void Init()
    {
        Canvas canvas = GetComponent<Canvas>();
        if (canvas != null && canvas.worldCamera == null)
            canvas.worldCamera = Camera.main;

        Bind<TMP_Text>(typeof(Texts));
        Bind<Slider>(typeof(Sliders));
        Bind<Image>(typeof(Images));

        HpBar = Get<Slider>(Sliders.HpBar);
        StaminaBar = Get<Slider>(Sliders.StaminaBar);
        FpBar = Get<Slider>(Sliders.FpBar);

        HpText = Get<TMP_Text>(Texts.HpText);
        StaminaText = Get<TMP_Text>(Texts.StaminaText);
        FpText = Get<TMP_Text>(Texts.FpText);

        AttackText = Get<TMP_Text>(Texts.Attack);
        AttackSpeedText = Get<TMP_Text>(Texts.AttackSpeed);
        DefenseText = Get<TMP_Text>(Texts.Defense);
        StRegenText = Get<TMP_Text>(Texts.StRegen);
        CritChanceText = Get<TMP_Text>(Texts.CritChance);
        CritDamageText = Get<TMP_Text>(Texts.CritDamage);

        Get<Image>(Images.Icon).sprite = GameManager.Instance.SelectedPlayerClass.unitSprite;
        playerPanelObj = Get<Image>(Images.PlayerPanel).gameObject;
        inventoryPanelObj = Get<Image>(Images.InventoryPanel).gameObject;
        playerRect = playerPanelObj.GetComponent<RectTransform>();
        inventoryRect = inventoryPanelObj.GetComponent<RectTransform>();
        playerCG = playerPanelObj.GetComponent<CanvasGroup>();
        inventoryCG = inventoryPanelObj.GetComponent<CanvasGroup>();
        originalPos = playerRect.anchoredPosition;
        isShowingPlayer = true;

        // 시작 시 스킬 정보 창 비활성화
        Get<Image>(Images.CharacterSkill_Info).gameObject.SetActive(false);

        // Icon에 마우스 호버 이벤트 바인딩
        SetupIconHoverEvent(Get<Image>(Images.Icon).gameObject);
    }

    private void Start()
    {
        Player = GameManager.Instance.Player;
        var stats = Player.GetStat<PlayerStats>();
        if (stats == null) return;

        stats.OnHpChanged += UpdateHp;
        stats.OnStaminaChanged += UpdateStamina;
        stats.OnFpChanged += UpdateFp;

        stats.OnAttackDamageChanged += UpdateAttack;
        stats.OnDefenseChanged += UpdateDefense;
        stats.OnAttackSpeedChanged += UpdateAttackSpeed;
        stats.OnStaminaRegenChanged += UpdateStaminaRegen;
        stats.OnCriticalChanceChanged += UpdateCritChance;
        stats.OnCriticalDamageChanged += UpdateCritDamage;

        RefreshAll();

        if (GameManager.Input != null)
            GameManager.Input.OnTabTriggered += OnTabPressed;
    }

    private void OnDestroy()
    {
        if (Player == null) return;
        var stats = Player.GetStat<PlayerStats>();
        if (stats == null) return;

        stats.OnHpChanged -= UpdateHp;
        stats.OnStaminaChanged -= UpdateStamina;
        stats.OnFpChanged -= UpdateFp;

        stats.OnAttackDamageChanged -= UpdateAttack;
        stats.OnDefenseChanged -= UpdateDefense;
        stats.OnAttackSpeedChanged -= UpdateAttackSpeed;
        stats.OnStaminaRegenChanged -= UpdateStaminaRegen;
        stats.OnCriticalChanceChanged -= UpdateCritChance;
        stats.OnCriticalDamageChanged -= UpdateCritDamage;

        if (GameManager.Instance != null && GameManager.Input != null)
            GameManager.Input.OnTabTriggered -= OnTabPressed;
    }

    #region Event & UI Updates
    private void SetupIconHoverEvent(GameObject iconObj)
    {
        EventTrigger trigger = iconObj.GetComponent<EventTrigger>();
        if (trigger == null)
            trigger = iconObj.AddComponent<EventTrigger>();

        // 마우스가 아이콘 위로 올라왔을 때 (Hover Enter)
        EventTrigger.Entry entryEnter = new EventTrigger.Entry();
        entryEnter.eventID = EventTriggerType.PointerEnter;
        entryEnter.callback.AddListener((data) => { OnIconHoverEnter(); });
        trigger.triggers.Add(entryEnter);

        // 마우스가 아이콘에서 벗어났을 때 (Hover Exit)
        EventTrigger.Entry entryExit = new EventTrigger.Entry();
        entryExit.eventID = EventTriggerType.PointerExit;
        entryExit.callback.AddListener((data) => { OnIconHoverExit(); });
        trigger.triggers.Add(entryExit);
    }

    private void OnIconHoverEnter()
    {
        var playerClass = GameManager.Instance.SelectedPlayerClass;
        if (playerClass != null)
        {
            // 스킬 정보 업데이트
            Get<Image>(Images.SkillIcon).sprite = playerClass.skillIcon;
            Get<TMP_Text>(Texts.SkillName).text = playerClass.skillName;
            Get<TMP_Text>(Texts.SkillDescription).text = playerClass.skillDesc;
        }

        // 스킬 정보 UI 활성화
        Get<Image>(Images.CharacterSkill_Info).gameObject.SetActive(true);
    }

    private void OnIconHoverExit()
    {
        // 스킬 정보 UI 비활성화
        Get<Image>(Images.CharacterSkill_Info).gameObject.SetActive(false);
    }

    private void RefreshAll()
    {
        var stats = Player.GetStat<PlayerStats>();
        if (stats == null) return;

        UpdateHp(stats.hp, stats.maxHp.GetValue());
        UpdateStamina(stats.stamina, stats.maxStamina.GetValue());
        UpdateFp(stats.fp, stats.maxFp.GetValue());

        UpdateAttack(stats.attackDamage.GetValue());
        UpdateDefense(stats.defense.GetValue());
        UpdateAttackSpeed(stats.attackSpeed.GetValue());
        UpdateStaminaRegen(stats.staminaRegen.GetValue());
        UpdateCritChance(stats.criticalChance.GetValue());
        UpdateCritDamage(stats.criticalDamage.GetValue());
    }

    private void UpdateHp(float current, float max)
    {
        HpBar.value = Math.Max(max > 0 ? current / max : 0, 0);
        HpText.text = $"{current:F0} / {max:F0}";
    }

    private void UpdateStamina(float current, float max)
    {
        StaminaBar.value = Math.Max(max > 0 ? current / max : 0, 0);
        StaminaText.text = $"{current:F0} / {max:F0}";
    }

    private void UpdateFp(float current, float max)
    {
        FpBar.value = Math.Max(max > 0 ? current / max : 0, 0);
        FpText.text = $"{current:F0} / {max:F0}";
    }

    private void UpdateAttack(float value)
    {
        AttackText.text = $"{value:F0}";
    }

    private void UpdateDefense(float value)
    {
        DefenseText.text = $"{value:F0}";
    }

    private void UpdateAttackSpeed(float value)
    {
        AttackSpeedText.text = $"{value:F2}";
    }

    private void UpdateStaminaRegen(float value)
    {
        StRegenText.text = $"{value:F2}";
    }

    private void UpdateCritChance(float value)
    {
        CritChanceText.text = $"{Math.Min(100, value):F0}";
    }

    private void UpdateCritDamage(float value)
    {
        CritDamageText.text = $"{value:F0}";
    }

    private void OnTabPressed()
    {
        //이벤트 중복 방지
        if (DOTween.IsTweening(playerRect) || DOTween.IsTweening(inventoryRect)) return;

        isShowingPlayer = !isShowingPlayer;

        if (isShowingPlayer)
            SwitchPanels(inventoryRect, inventoryCG, playerRect, playerCG, -1);
        else
            SwitchPanels(playerRect, playerCG, inventoryRect, inventoryCG, 1);
    }

    private void SwitchPanels(RectTransform from, CanvasGroup fromCG, RectTransform to, CanvasGroup toCG, int direction)
    {
        from.DOKill(); fromCG.DOKill();
        to.DOKill(); toCG.DOKill();

        // 나가는 패널
        // 방향이 1이면 왼쪽으로, -1이면 오른쪽으로밀려남
        float exitDestX = originalPos.x + (direction == 1 ? -slideOffset : slideOffset);

        fromCG.blocksRaycasts = false; // 클릭 방지
        from.DOAnchorPosX(exitDestX, animDuration).SetEase(Ease.OutQuad);
        fromCG.DOFade(0f, animDuration).OnComplete(() =>
        {
            from.gameObject.SetActive(false);
        });

        //들어오는 패널
        to.gameObject.SetActive(true);
        toCG.blocksRaycasts = true;

        // 방향이 1이면 오른쪽(+)에서 -1이면 왼쪽(-)에서 등장
        float enterStartX = originalPos.x + (direction == 1 ? slideOffset : -slideOffset);
        to.anchoredPosition = new Vector2(enterStartX, originalPos.y);
        toCG.alpha = 0f;

        to.DOAnchorPosX(originalPos.x, animDuration).SetEase(Ease.OutQuad);
        toCG.DOFade(1f, animDuration);
    }
    #endregion
}