using UnityEngine;
using DG.Tweening;
using System;

public abstract class BaseVFX : MonoBehaviour
{
    [HideInInspector] public AttackVFXType vfxType;

    protected SpriteRenderer spriteRenderer;

    protected virtual void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
            spriteRenderer.sortingOrder = 11;
    }

    public abstract void PlayEffect(Vector3 attackerPos, Vector3 targetPos, float hitDelay, Color color, Action onHit, Action onComplete);

    public void ReturnToPool()
    {
        transform.DOKill();
        if (spriteRenderer != null) spriteRenderer.DOKill();

        if (GameManager.Instance != null && GameManager.VFX != null)
            GameManager.VFX.ReturnEffect(this);
        else
            Destroy(gameObject);
    }
}