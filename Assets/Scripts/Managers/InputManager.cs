using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class InputManager
{
    public bool DodgePressed => Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;
    public bool ParryPressed => Keyboard.current != null && Keyboard.current.qKey.wasPressedThisFrame;
    public bool SkillPressed => Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;

    public event Action OnTabTriggered;
    public event Action OnEndingNextCutTriggered;

    public void OnUpdate()
    {
        if (Keyboard.current == null) return;

        // ESC 키 (설정창/일시정지)
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
            HandleEscapeKey();

        // Tab 키 (플레이어 패널 <-> 인벤토리 토글)
        if (Keyboard.current.tabKey.wasPressedThisFrame)
            OnTabTriggered?.Invoke();

        // 마우스 좌클릭 사운드
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            GameManager.Sound.PlaySFX(SFX.UIClick);

        // 엔딩 씬 전용 입력 (Space or 마우스 좌클릭)
        if (GameManager.Instance != null && GameManager.Instance.State == GameState.Ending)
        {
            if (Keyboard.current.spaceKey.wasPressedThisFrame ||
               (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame))
            {
                OnEndingNextCutTriggered?.Invoke();
            }
        }
    }

    private void HandleEscapeKey()
    {
        GameState state = GameManager.Instance.State;

        // 배틀일 때만
        if (state == GameState.Battle)
        {
            // 1순위: 설정창이 켜져있다면 설정창 닫기
            if (SettingUI.Instance != null && SettingUI.Instance.IsVisible)
            {
                SettingUI.Instance.Hide();
            }
            // 2순위: 설정창이 꺼져있다면 일시정지(PauseUI) 토글
            else if (PauseUI.Instance != null)
            {
                PauseUI.Instance.TogglePause();
            }
        }
        else
        {
            // 1순위: TutorialUI가 열려있으면 닫기
            if (TutorialUI.Instance != null && TutorialUI.Instance.IsVisible)
                TutorialUI.Instance.Hide();
            // 2순위: 아무것도 없으면 SettingUI 토글
            else
                SettingUI.Instance?.Toggle();
        }
    }
}