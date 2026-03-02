using UnityEngine;
using DG.Tweening;
using System;

public class SimpleEffect : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sortingOrder = 11;
    }

    public void Play(Vector3 position, Sprite sprite, Action<Transform, SpriteRenderer, Action> animationLogic, Color color)
    {
        // 상태 초기화
        transform.position = position;
        transform.rotation = Quaternion.identity;
        transform.localScale = Vector3.one;

        spriteRenderer.sprite = sprite;
        spriteRenderer.color = color;
        spriteRenderer.enabled = true;

        if (animationLogic != null)
            animationLogic.Invoke(transform, spriteRenderer, ReturnToPool);
        else
            DOVirtual.DelayedCall(0.5f, ReturnToPool);
    }

    private void ReturnToPool()
    {
        transform.DOKill();

        if (GameManager.Instance != null && GameManager.VFX != null)
            GameManager.VFX.ReturnEffect(this);
        else
            Destroy(gameObject);
    }
}