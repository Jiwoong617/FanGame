using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;

public class RestUI : UI_Base
{
    enum Buttons
    {
        Rest,
        Meditation,
        Training,
        Next,
    }

    enum Images
    {
        Fade
    }

    enum Texts
    {
        ResultText
    }

    private bool _isProcessing = false;

    [Header("Target Stat UI RectTransforms")]
    [SerializeField] private RectTransform atkTextRect;
    [SerializeField] private RectTransform atkSpeedTextRect;
    [SerializeField] private RectTransform staminaBarRect;
    [SerializeField] private RectTransform defenseTextRect;

    protected override void Init()
    {
        Canvas canvas = GetComponent<Canvas>();
        if (canvas != null && canvas.worldCamera == null)
            canvas.worldCamera = Camera.main;

        Bind<Button>(typeof(Buttons));
        Bind<Image>(typeof(Images));
        Bind<TMP_Text>(typeof(Texts));

        Get<Button>(Buttons.Rest).onClick.AddListener(OnRestClicked);
        Get<Button>(Buttons.Meditation).onClick.AddListener(OnMeditationClicked);
        Get<Button>(Buttons.Training).onClick.AddListener(OnTrainingClicked);
        Get<Button>(Buttons.Next).onClick.AddListener(OnNextClicked);

        GameManager.Rest.SetUI(this);
        Hide();
    }

    private void Start()
    {
        if (GameManager.Input != null)
            GameManager.Input.OnTabTriggered += OnTabPressed;
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null && GameManager.Input != null)
            GameManager.Input.OnTabTriggered -= OnTabPressed;
    }

    public void ShowRest()
    {
        Show();
        _isProcessing = false;

        Get<Button>(Buttons.Rest).gameObject.SetActive(true);
        Get<Button>(Buttons.Meditation).gameObject.SetActive(true);
        Get<Button>(Buttons.Training).gameObject.SetActive(true);

        Get<Button>(Buttons.Next).gameObject.SetActive(false);

        Get<TMP_Text>(Texts.ResultText).gameObject.SetActive(false);
        Get<TMP_Text>(Texts.ResultText).text = "";

        Image fadeImg = Get<Image>(Images.Fade);
        fadeImg.gameObject.SetActive(true);
        fadeImg.color = new Color(0, 0, 0, 0);
    }

    private void OnRestClicked()
    {
        if (_isProcessing)
            return;

        ProcessAction("휴식", () =>
        {
            var stats = GameManager.Instance.Player.GetStat<PlayerStats>();
            float healAmount = stats.maxHp.GetValue() * 0.3f;
            float beforeHp = stats.hp;

            stats.hp = Mathf.Min(stats.hp + healAmount, stats.maxHp.GetValue());

            CombatEventContext ctx = new CombatEventContext(GameManager.Instance.Player, GameManager.Instance.Player, 0);
            GameManager.Instance.Player.TriggerAbility(CombatEvent.OnRest, ctx);

            GameManager.Sound.PlaySFX(SFX.Rest_Sleep);
        });
    }

    private void OnMeditationClicked()
    {
        if (_isProcessing) return;
        ProcessAction("명상", () =>
        {
            var stats = GameManager.Instance.Player.GetStat<PlayerStats>();
            float beforeFp = stats.fp;
            stats.fp = stats.maxFp.GetValue();

            stats.maxFp.SetBaseValue(stats.maxFp.GetBaseValue() + 1f);

            GameManager.Sound.PlaySFX(SFX.Rest_Meditate);
        });
    }

    private void OnTrainingClicked()
    {
        if (_isProcessing) return;
        ProcessAction("훈련", () =>
        {
            var stats = GameManager.Instance.Player.GetStat<PlayerStats>();
            int rand = Random.Range(0, 4);

            float beforeVal = 0f;
            float afterVal = 0f;
            float diff = 0f;
            string diffText = "";

            RectTransform targetStatRect = null;
            Color textColor = Color.black; // 기본 색상은 검정색으로 설정

            switch (rand)
            {
                case 0: // 공격력
                    beforeVal = stats.attackDamage.GetValue();
                    float randAtk = Random.Range(1, 3);
                    stats.attackDamage.SetBaseValue(stats.attackDamage.GetBaseValue() + randAtk);
                    afterVal = stats.attackDamage.GetValue();

                    diff = afterVal - beforeVal;
                    diffText = $"+{diff:F0}";

                    targetStatRect = atkTextRect;
                    break;

                case 1: // 공격속도
                    beforeVal = stats.attackSpeed.GetValue();
                    float randAtkSpeed = Random.Range(1, 4) * 0.1f;
                    stats.attackSpeed.SetBaseValue(stats.attackSpeed.GetBaseValue() + randAtkSpeed);
                    afterVal = stats.attackSpeed.GetValue();

                    diff = afterVal - beforeVal;
                    diffText = $"+{diff:F2}";

                    targetStatRect = atkSpeedTextRect;
                    break;

                case 2: // 스태미나
                    beforeVal = stats.maxStamina.GetValue();
                    float randSt = Random.Range(10, 16);
                    stats.maxStamina.SetBaseValue(stats.maxStamina.GetBaseValue() + randSt);
                    afterVal = stats.maxStamina.GetValue();

                    diff = afterVal - beforeVal;
                    diffText = $"+{diff:F0}";

                    targetStatRect = staminaBarRect;
                    textColor = Color.white; // 스태미나일 때만 흰색으로 변경
                    break;

                case 3: // 방어력
                    beforeVal = stats.defense.GetValue();
                    float randDf = Random.Range(1, 3);
                    stats.defense.SetBaseValue(stats.defense.GetBaseValue() + randDf);
                    afterVal = stats.defense.GetValue();

                    diff = afterVal - beforeVal;
                    diffText = $"+{diff:F0}";

                    targetStatRect = defenseTextRect;
                    break;
            }

            // 훈련 시에만 Text UI 활성화 및 값, 색상 갱신
            TMP_Text resultTMP = Get<TMP_Text>(Texts.ResultText);
            resultTMP.gameObject.SetActive(true);
            resultTMP.text = diffText;
            resultTMP.color = textColor; // 결정된 색상 적용

            resultTMP.alignment = TextAlignmentOptions.Left;

            if (targetStatRect != null)
            {
                AlignToTargetUI(targetStatRect);
            }

            GameManager.Sound.PlaySFX(SFX.Rest_Train);
        });
    }

    private void ProcessAction(string actionName, System.Action actionLogic)
    {
        _isProcessing = true;
        Image fadeImg = Get<Image>(Images.Fade);

        fadeImg.DOFade(1f, 0.5f).OnComplete(() =>
        {
            actionLogic?.Invoke();

            Get<Button>(Buttons.Rest).gameObject.SetActive(false);
            Get<Button>(Buttons.Meditation).gameObject.SetActive(false);
            Get<Button>(Buttons.Training).gameObject.SetActive(false);

            Get<Button>(Buttons.Next).gameObject.SetActive(true);

            fadeImg.DOFade(0f, 0.5f).OnComplete(() =>
            {
                _isProcessing = false;
            });
        });
    }

    private void AlignToTargetUI(RectTransform targetRect)
    {
        if (targetRect == null) return;

        Canvas.ForceUpdateCanvases();

        RectTransform myRect = Get<TMP_Text>(Texts.ResultText).rectTransform;

        myRect.pivot = targetRect.pivot;
        myRect.sizeDelta = targetRect.rect.size;
        myRect.position = targetRect.position;
    }

    private void OnNextClicked()
    {
        Hide();
        GameManager.Rest.CompleteRest();
    }

    private void OnTabPressed()
    {
        TMP_Text resultTMP = Get<TMP_Text>(Texts.ResultText);
        if (resultTMP != null && resultTMP.gameObject.activeSelf)
        {
            resultTMP.gameObject.SetActive(false);
        }
    }
}