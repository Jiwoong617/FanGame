using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.U2D;

public class ModularAttackVFX : BaseVFX
{
    [SerializeField] protected bool isRotate = false;
    [SerializeField] protected bool isRandPos = false;

    [SerializeReference, SerializeReferenceDropdown]
    public VFXAnimationStrategy animationStrategy;

    public override void PlayEffect(Vector3 attackerPos, Vector3 targetPos, float hitDelay, Color color, Action onHit, Action onComplete)
    {
        transform.localScale = Vector3.one;
        transform.DOKill();
        spriteRenderer.DOKill();

        if (isRandPos)
        {
            Vector3 randPos = new Vector3(UnityEngine.Random.Range(-0.25f, 0.25f), UnityEngine.Random.Range(-0.25f, 0.25f), 0);
            transform.position = targetPos + randPos;
        }
        else
        {
            transform.position = targetPos; 
        }

        if (isRotate)
        {
            Vector3 dir = targetPos - attackerPos;
            float baseAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            float randomOffset = UnityEngine.Random.Range(-30f, 30f);
            transform.rotation = Quaternion.Euler(0, 0, baseAngle + randomOffset);
        }
        else
        {
            transform.rotation = Quaternion.identity;
        }


        if (animationStrategy != null)
        {
            animationStrategy.PlaySequence(transform, spriteRenderer, attackerPos, targetPos, hitDelay, color, onHit, () =>
            {
                onComplete?.Invoke();
                ReturnToPool();
            });
        }
        else
        {
            onComplete?.Invoke();
            ReturnToPool();
        }
    }
}