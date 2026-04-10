using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections;

public class PauseUI : UI_Base
{
    private static PauseUI instance;
    public static PauseUI Instance => instance;

    enum Texts { PauseText, CountdownText }
    enum Buttons { ResumeButton, SettingButton }

    private Canvas canvas;
    private bool isCountingDown = false;

    protected override void Init()
    {
        instance = this;
        canvas = GetComponent<Canvas>();

        Bind<TextMeshProUGUI>(typeof(Texts));
        Bind<Button>(typeof(Buttons));

        Get<Button>(Buttons.ResumeButton).onClick.AddListener(ResumeGame);
        Get<Button>(Buttons.SettingButton).onClick.AddListener(() => SettingUI.Instance.Show());

        Get<TextMeshProUGUI>(Texts.CountdownText).gameObject.SetActive(false);
        canvas.enabled = false;
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    public void TogglePause()
    {
        if (isCountingDown) return;

        if (canvas.enabled)
            ResumeGame();
        else
            PauseGame();
    }

    private void PauseGame()
    {
        Time.timeScale = 0f;
        canvas.enabled = true;
        Get<TextMeshProUGUI>(Texts.PauseText).gameObject.SetActive(true);
        Get<Button>(Buttons.ResumeButton).gameObject.SetActive(true);
        Get<Button>(Buttons.SettingButton).gameObject.SetActive(true);
    }

    public void ResumeGame()
    {
        if (isCountingDown) return;
        StartCoroutine(CountdownCoroutine());
    }

    private IEnumerator CountdownCoroutine()
    {
        isCountingDown = true;

        // 메뉴 요소 숨기기
        Get<TextMeshProUGUI>(Texts.PauseText).gameObject.SetActive(false);
        Get<Button>(Buttons.ResumeButton).gameObject.SetActive(false);
        Get<Button>(Buttons.SettingButton).gameObject.SetActive(false);

        var countdownText = Get<TextMeshProUGUI>(Texts.CountdownText);
        countdownText.gameObject.SetActive(true);

        for (int i = 3; i > 0; i--)
        {
            countdownText.text = i.ToString();
            countdownText.transform.localScale = Vector3.one * 2f;
            countdownText.transform.DOScale(1f, 0.4f).SetUpdate(true).SetEase(Ease.OutBack);
            yield return new WaitForSecondsRealtime(1f); // 실제 시간 기준 대기
        }

        countdownText.gameObject.SetActive(false);
        Time.timeScale = 1f; // 여기서 시간 복구
        canvas.enabled = false;
        isCountingDown = false;
    }
}