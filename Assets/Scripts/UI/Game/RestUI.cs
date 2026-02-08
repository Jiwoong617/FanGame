using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;

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

    protected override void Init()
    {
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
            float healAmount = stats.maxHp * 0.3f;
            float beforeHp = stats.hp;
            stats.hp = Mathf.Min(stats.hp + healAmount, stats.maxHp);
            
            return $"체력을 회복했습니다.\n(HP : {beforeHp:F0} -> {stats.hp:F0})";
        });
    }

    private void OnMeditationClicked()
    {
        if (_isProcessing) return;
        ProcessAction("명상", () =>
        {
            var stats = GameManager.Instance.Player.GetStat<PlayerStats>();
            float beforeFp = stats.fp;
            stats.fp = stats.maxFp;

            return $"정신을 집중하여 FP를 모두 회복했습니다.\n(FP : {beforeFp:F0} -> {stats.fp:F0})";
        });
    }

    private void OnTrainingClicked()
    {
        if (_isProcessing) return;
        ProcessAction("훈련", () =>
        {
            var stats = GameManager.Instance.Player.GetStat<PlayerStats>();
            int rand = Random.Range(0, 3);
            string resultMsg = "";

            switch (rand)
            {
                case 0: // 공격력
                    float damageUp = Random.Range(1, 5);
                    stats.attackDamage += damageUp;
                    resultMsg = $"공격 훈련을 수행했습니다.\n공격력이 {damageUp} 상승했습니다.";
                    break;
                case 1: // 공격속도
                    float speedUp = Random.Range(0.1f, 0.3f);
                    stats.attackSpeed += speedUp;
                    resultMsg = $"민첩성 훈련을 수행했습니다.\n공격 속도가 {speedUp:F2} 상승했습니다.";
                    break;
                case 2: // 스태미나
                    float staminaUp = Random.Range(10, 20);
                    stats.maxStamina += staminaUp;
                    stats.stamina = stats.maxStamina;
                    resultMsg = $"지구력 훈련을 수행했습니다.\n최대 스태미나가 {staminaUp:F0} 상승했습니다.";
                    break;
            }
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
