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
        Fade // BackGround와 Result 제거
    }

    enum Texts
    {
        ResultText
    }

    private bool _isProcessing = false;

    // Result 이미지 관련 리스트 제거됨

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

    public void ShowRest()
    {
        Show();
        _isProcessing = false;

        Get<Button>(Buttons.Rest).gameObject.SetActive(true);
        Get<Button>(Buttons.Meditation).gameObject.SetActive(true);
        Get<Button>(Buttons.Training).gameObject.SetActive(true);

        Get<Button>(Buttons.Next).gameObject.SetActive(false);

        // 시작 시 ResultText는 비활성화 (휴식, 명상 시에는 안 보이도록)
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

            // Result Text 변경 및 UI 활성화 없음
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

            // Result Text 변경 및 UI 활성화 없음
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

            switch (rand)
            {
                case 0: // 공격력
                    beforeVal = stats.attackDamage.GetValue();
                    float randAtk = Random.Range(1, 3);
                    StatModifier atkmod = new StatModifier(randAtk, StatModType.Flat);
                    stats.attackDamage.AddModifier(atkmod);
                    afterVal = stats.attackDamage.GetValue();

                    diff = afterVal - beforeVal;
                    diffText = $"+{diff:F0}";
                    break;

                case 1: // 공격속도
                    beforeVal = stats.attackSpeed.GetValue();
                    float randAtkSpeed = Random.Range(1, 4) * 0.1f;
                    StatModifier asmod = new StatModifier(randAtkSpeed, StatModType.Flat);
                    stats.attackSpeed.AddModifier(asmod);
                    afterVal = stats.attackSpeed.GetValue();

                    diff = afterVal - beforeVal;
                    diffText = $"+{diff:F2}";
                    break;

                case 2: // 스태미나
                    beforeVal = stats.maxStamina.GetValue();
                    float randSt = Random.Range(10, 16);
                    StatModifier stmod = new StatModifier(randSt, StatModType.Flat);
                    stats.maxStamina.AddModifier(stmod);
                    afterVal = stats.maxStamina.GetValue();

                    diff = afterVal - beforeVal;
                    diffText = $"+{diff:F0}";
                    break;

                case 3: // 방어력
                    beforeVal = stats.defense.GetValue();
                    float randDf = Random.Range(1, 3);
                    StatModifier dfmod = new StatModifier(randDf, StatModType.Flat);
                    stats.defense.AddModifier(dfmod);
                    afterVal = stats.defense.GetValue();

                    diff = afterVal - beforeVal;
                    diffText = $"+{diff:F0}";
                    break;
            }

            // 훈련 시에만 Text UI 활성화 및 값 갱신
            TMP_Text resultTMP = Get<TMP_Text>(Texts.ResultText);
            resultTMP.gameObject.SetActive(true);
            resultTMP.text = diffText;

            // RectTransform 수정 구조 (원하는 위치/크기/애니메이션 등 적용 가능)
            RectTransform rect = resultTMP.rectTransform;
            // 예시: rect.anchoredPosition = new Vector2(0, 150);
            // 예시: rect.localScale = Vector3.one * 1.5f;
        });
    }

    // Func<string> 대신 Action 델리게이트를 사용하여 단순 로직 실행으로 구조 변경
    private void ProcessAction(string actionName, System.Action actionLogic)
    {
        _isProcessing = true;
        Image fadeImg = Get<Image>(Images.Fade);

        fadeImg.DOFade(1f, 0.5f).OnComplete(() =>
        {
            // 전달받은 개별 로직(스탯 증감, UI 활성화 등) 실행
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

    private void OnNextClicked()
    {
        Hide();
        GameManager.Rest.CompleteRest();
    }
}