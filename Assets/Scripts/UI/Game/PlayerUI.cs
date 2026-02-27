using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

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
        CritDamage
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
        BackGround,
        InventoryUI,
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

    private GameObject PlayerPanel; 
    private GameObject InventoryPanel;
    private bool isShowingPlayer = true;

    private InputAction tabAction;

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
        PlayerPanel = Get<Image>(Images.BackGround).gameObject;
        InventoryPanel = Get<Image>(Images.InventoryUI).gameObject;

        tabAction = new InputAction(binding: "<Keyboard>/tab");
        tabAction.performed += OnTabPressed;
        tabAction.Enable();
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

        if (tabAction != null)
        {
            tabAction.performed -= OnTabPressed;
            tabAction.Disable();
            tabAction.Dispose();
        }
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
        CritChanceText.text = $"{value:F0}";
    }

    private void UpdateCritDamage(float value)
    {
        CritDamageText.text = $"{value:F0}";
    }

    private void OnTabPressed(InputAction.CallbackContext context)
    {
        isShowingPlayer = !isShowingPlayer;

        PlayerPanel.SetActive(isShowingPlayer);
        InventoryPanel.SetActive(!isShowingPlayer);
    }
}
