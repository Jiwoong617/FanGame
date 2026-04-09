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
        BackGround,
        Result,
        Fade
    }

    enum Texts
    {
        ResultText
    }

    private bool _isProcessing = false;

    //0 - sleep, 1 - training, 2 - meditate
    [SerializeField] private List<Sprite> resultSpriteList;

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
        
        Get<Image>(Images.Result).gameObject.SetActive(false);
        Get<Button>(Buttons.Next).gameObject.SetActive(false);
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
            Get<Image>(Images.Result).sprite = resultSpriteList[0];

            CombatEventContext ctx = new CombatEventContext(GameManager.Instance.Player, GameManager.Instance.Player, 0);
            GameManager.Instance.Player.TriggerAbility(CombatEvent.OnRest, ctx);

            return $"잠을 자 체력을 회복했습니다.";
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
            Get<Image>(Images.Result).sprite = resultSpriteList[2];

            return $"정신을 집중하여 FP를 모두 회복했습니다.";
        });
    }

    private void OnTrainingClicked()
    {
        if (_isProcessing) return;
        ProcessAction("훈련", () =>
        {
            var stats = GameManager.Instance.Player.GetStat<PlayerStats>();
            int rand = Random.Range(0, 4);
            string resultMsg = "";

            float beforeVal = 0f;
            float afterVal = 0f;
            float diff = 0f;

            switch (rand)
            {
                case 0: // 공격력
                    beforeVal = stats.attackDamage.GetValue();

                    float randAtk = Random.Range(1, 3);
                    StatModifier atkmod = new StatModifier(randAtk, StatModType.Flat);
                    stats.attackDamage.AddModifier(atkmod);

                    afterVal = stats.attackDamage.GetValue();
                    diff = afterVal - beforeVal;

                    resultMsg = $"공격 훈련을 수행했습니다.\n공격력이 {diff:F0} 상승했습니다.";
                    break;

                case 1: // 공격속도
                    beforeVal = stats.attackSpeed.GetValue();

                    float randAtkSpeed = Random.Range(1, 4) * 0.1f;
                    StatModifier asmod = new StatModifier(randAtkSpeed, StatModType.Flat);
                    stats.attackSpeed.AddModifier(asmod);

                    afterVal = stats.attackSpeed.GetValue();
                    diff = afterVal - beforeVal;

                    resultMsg = $"민첩성 훈련을 수행했습니다.\n공격 속도가 {diff:F2} 상승했습니다.";
                    break;

                case 2: // 스태미나
                    beforeVal = stats.maxStamina.GetValue();

                    float randSt = Random.Range(10, 16);
                    StatModifier stmod = new StatModifier(randSt, StatModType.Flat);
                    stats.maxStamina.AddModifier(stmod);

                    afterVal = stats.maxStamina.GetValue();
                    diff = afterVal - beforeVal;

                    resultMsg = $"지구력 훈련을 수행했습니다.\n최대 스태미나가 {diff:F0} 상승했습니다.";
                    break;

                case 3: // 방어력
                    beforeVal = stats.defense.GetValue();

                    float randDf = Random.Range(1, 3);
                    StatModifier dfmod = new StatModifier(randDf, StatModType.Flat);
                    stats.defense.AddModifier(dfmod);

                    afterVal = stats.defense.GetValue();
                    diff = afterVal - beforeVal;

                    resultMsg = $"맷집 훈련을 수행했습니다.\n방어력이 {diff:F0} 상승했습니다.";
                    break;
            }

            Get<Image>(Images.Result).sprite = resultSpriteList[1];

            return resultMsg;
        });
    }

    private void ProcessAction(string actionName, System.Func<string> actionLogic)
    {
        _isProcessing = true;
        Image fadeImg = Get<Image>(Images.Fade);
        
        fadeImg.DOFade(1f, 0.5f).OnComplete(() =>
        {
            string resultText = actionLogic.Invoke();

            Get<Button>(Buttons.Rest).gameObject.SetActive(false);
            Get<Button>(Buttons.Meditation).gameObject.SetActive(false);
            Get<Button>(Buttons.Training).gameObject.SetActive(false);

            Get<Image>(Images.Result).gameObject.SetActive(true);
            Get<TMP_Text>(Texts.ResultText).text = resultText;
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
