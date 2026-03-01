using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : UI_Base
{
    [SerializeField] private CharacterSelectUI characterSelectUI;

    #region Enum
    private enum Buttons
    {
        StartButton,
        SettingButton,
        ExitButton
    }
    #endregion


    protected override void Init()
    {
        Bind<Button>(typeof(Buttons));

        BindButtonEvent();
    }

    private void BindButtonEvent()
    {
        Get<Button>(Buttons.StartButton).onClick.AddListener(OnGameStartClicked);
        Get<Button>(Buttons.SettingButton).onClick.AddListener(OnSettingClicked);
        Get<Button>(Buttons.ExitButton).onClick.AddListener(OnExitClicked);
    }

    private void OnGameStartClicked()
    {
        characterSelectUI.Show();
    }

    private void OnSettingClicked()
    {
        if (SettingUI.Instance != null)
        {
            SettingUI.Instance.Toggle();
        }
    }

    private void OnExitClicked()
    {
        Debug.Log("Quit Game");
        Application.Quit();
    }
}
