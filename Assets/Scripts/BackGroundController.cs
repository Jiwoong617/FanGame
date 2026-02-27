using DG.Tweening;
using NUnit.Framework.Internal;
using System;
using UnityEngine;
using static UnityEngine.InputSystem.LowLevel.InputStateHistory;
public enum BackgroundType
{
    None,
    Stage1_Battle,
    Stage1_Rest,
    Stage2_Battle,
    Stage2_Rest,
    Stage3_Battle,
    Stage3_Rest,
}

public class BackGroundController : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStateChanged -= HandleStateChanged;
            GameManager.Instance.OnGameStateChanged += HandleStateChanged;

            HandleStateChanged(GameManager.Instance.State);
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStateChanged -= HandleStateChanged;
        }

        spriteRenderer.DOKill();
    }

    private void HandleStateChanged(GameState newState)
    {
        if (newState == GameState.Reward || newState == GameState.MapSelect || newState == GameState.MainMenu || newState == GameState.GameOver)
            return;

        int stageNum = GameManager.Instance.CurrentStageIndex + 1;

        // Stage1_Battle
        string enumStr = $"Stage{stageNum}_{newState}";

        if (Enum.TryParse(enumStr, out BackgroundType bgType))
        {
            ChangeBackground(bgType);
        }
    }

    private void ChangeBackground(BackgroundType bgType)
    {
        if (bgType == BackgroundType.None) return;

        Sprite newBg = GameManager.SpriteData.GetSprite(bgType, "Backgrounds");

        if (newBg == null || spriteRenderer.sprite == newBg) return;

        FitToScreen(newBg);

        Sequence seq = DOTween.Sequence();
        seq.Append(spriteRenderer.DOColor(Color.black, 0.25f));
        seq.AppendCallback(() => spriteRenderer.sprite = newBg);
        seq.Append(spriteRenderer.DOColor(Color.white, 0.25f));
    }

    private void FitToScreen(Sprite sprite)
    {
        if (sprite == null || Camera.main == null) return;

        float cameraHeight = Camera.main.orthographicSize * 2f;
        float cameraWidth = cameraHeight * Camera.main.aspect;

        float spriteHeight = sprite.bounds.size.y;
        float spriteWidth = sprite.bounds.size.x;

        float scaleX = cameraWidth / spriteWidth;
        float scaleY = cameraHeight / spriteHeight;

        float finalScale = Mathf.Max(scaleX, scaleY);

        transform.localScale = new Vector3(finalScale, finalScale, 1f);
    }
}
