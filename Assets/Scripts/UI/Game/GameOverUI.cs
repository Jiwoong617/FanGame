using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class GameOverUI : UI_Base
{
    enum Buttons { TitleButton }
    enum Images { Background }

    protected override void Init()
    {
        Canvas canvas = GetComponent<Canvas>();
        if (canvas != null && canvas.worldCamera == null)
            canvas.worldCamera = Camera.main;

        Bind<Button>(typeof(Buttons));
        Bind<Image>(typeof(Images));

        Get<Button>(Buttons.TitleButton).onClick.AddListener(OnTitleClicked);

        if (GameManager.Instance != null)
            GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;

        Hide();
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
    }

    private void HandleGameStateChanged(GameState state)
    {
        if (state == GameState.GameOver)
        {
            Show();

            Image bg = Get<Image>(Images.Background);
            bg.color = new Color(0f, 0f, 0f, 0f);
            bg.DOFade(0.99f, 2f);

            Button titleBtn = Get<Button>(Buttons.TitleButton);
            titleBtn.gameObject.SetActive(false);
            DOVirtual.DelayedCall(2.5f, () => titleBtn.gameObject.SetActive(true));
        }
    }

    private void OnTitleClicked()
    {
        GameManager.Instance.ResetGame();

        GameManager.Scene.LoadScene(SceneType.MainMenu);
    }
}