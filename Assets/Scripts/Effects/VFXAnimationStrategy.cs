using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.U2D;

[System.Serializable]
public abstract class VFXAnimationStrategy
{
    public abstract void PlaySequence(Transform t, SpriteRenderer sr, 
        Vector3 attackerPos, Vector3 targetPos, float hitDelay, Color color, Action onHit, Action onComplete);
}


[System.Serializable]
public class SlashAnimation : VFXAnimationStrategy
{
    public override void PlaySequence(Transform t, SpriteRenderer sr,
        Vector3 attackerPos, Vector3 targetPos, float hitDelay, Color color, Action onHit, Action onComplete)
    {
        sr.color = new Color(color.r, color.g, color.b, 0f);

        Sequence seq = DOTween.Sequence();
        if (hitDelay > 0) seq.AppendInterval(hitDelay);
        seq.AppendCallback(() => sr.color = color);

        seq.Append(t.DOScale(new Vector3(1.5f, 0.75f, 1f), 0.05f).SetEase(Ease.OutExpo));
        seq.Join(sr.DOFade(0f, 0.2f).SetEase(Ease.InQuad));

        seq.OnComplete(() => onComplete?.Invoke());
    }
}

[System.Serializable]
public class SmashAnimation : VFXAnimationStrategy
{
    public override void PlaySequence(Transform t, SpriteRenderer sr,
        Vector3 attackerPos, Vector3 targetPos, float hitDelay, Color color, Action onHit, Action onComplete)
    {
        t.localScale = Vector3.one * 0.5f;
        sr.color = new Color(color.r, color.g, color.b, 0f);

        Sequence seq = DOTween.Sequence();
        if (hitDelay > 0) seq.AppendInterval(hitDelay);
        seq.AppendCallback(() => sr.color = color);

        seq.Append(t.DOScale(1.1f, 0.05f).SetEase(Ease.OutBack));
        seq.Join(sr.DOFade(0f, 0.2f).SetEase(Ease.InQuad));

        seq.OnComplete(() => onComplete?.Invoke());
    }
}

[System.Serializable]
public class MagicAnimation : VFXAnimationStrategy
{
    public override void PlaySequence(Transform t, SpriteRenderer sr,
        Vector3 attackerPos, Vector3 targetPos, float hitDelay, Color color, Action onHit, Action onComplete)
    {
        Sequence seq = DOTween.Sequence();
        if (hitDelay > 0) seq.AppendInterval(hitDelay);
        seq.AppendCallback(() => sr.color = color);

        seq.Append(t.DOScale(1.1f, 0.3f));
        seq.Join(sr.DOFade(0f, 0.3f));

        seq.OnComplete(() => onComplete?.Invoke());
    }
}

[System.Serializable]
public class RoseAnimation : VFXAnimationStrategy
{
    public override void PlaySequence(Transform t, SpriteRenderer sr,
        Vector3 attackerPos, Vector3 targetPos, float hitDelay, Color color, Action onHit, Action onComplete)
    {
        t.localScale = new Vector3(0.2f, 0.1f, 1f);

        Vector3 dir = targetPos - attackerPos;
        float angle = UnityEngine.Random.Range(0, 360f);
        t.rotation = Quaternion.Euler(0, 0, angle);
        sr.color = new Color(color.r, color.g, color.b, 0f);
        t.localScale = new Vector3(0.2f, 0.1f, 1f);

        Sequence seq = DOTween.Sequence();
        if (hitDelay > 0) seq.AppendInterval(hitDelay);
        seq.AppendCallback(() => sr.color = color);

        seq.Append(t.DOScale(new Vector3(1.2f, 0.8f, 1f), 0.05f).SetEase(Ease.OutBack));
        seq.Append(t.DOScale(new Vector3(0.1f, 0.1f, 1f), 0.05f).SetEase(Ease.InQuad));
        seq.Join(t.DOMove(targetPos - (dir.normalized * 0.2f), 0.05f).SetEase(Ease.InQuad));
        seq.Join(sr.DOFade(0f, 0.05f).SetEase(Ease.InQuad));

        seq.OnComplete(() => onComplete?.Invoke());
    }
}

[System.Serializable]
public class HasiyoAttackAnimation : VFXAnimationStrategy
{
    public override void PlaySequence(Transform t, SpriteRenderer sr,
        Vector3 attackerPos, Vector3 targetPos, float hitDelay, Color color, Action onHit, Action onComplete)
    {
        Vector3 dir = targetPos - attackerPos;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        t.position = targetPos + new Vector3(0.5f, 0.5f);
        t.rotation = Quaternion.Euler(0, 0, angle - 150f);
        sr.color = color;

        Sequence seq = DOTween.Sequence();
        if (hitDelay > 0)
            seq.Append(t.DORotate(new Vector3(0, 0, angle - 90f), hitDelay).SetEase(Ease.InExpo));
        else
            t.rotation = Quaternion.Euler(0, 0, angle - 90f);

        seq.Append(sr.DOFade(0f, 0.5f));

        seq.OnComplete(() => onComplete?.Invoke());
    }
}

[System.Serializable]
public class RoseSkillAnimation : VFXAnimationStrategy
{
    public override void PlaySequence(Transform t, SpriteRenderer sr,
        Vector3 attackerPos, Vector3 targetPos, float hitDelay, Color color, Action onHit, Action onComplete)
    {
        float angle = UnityEngine.Random.Range(0, 360f);
        t.rotation = Quaternion.Euler(0, 0, angle);
        t.localScale = new Vector3(0.2f, 0.1f, 1f);
        sr.color = color;

        Sequence seq = DOTween.Sequence();
        seq.Append(t.DOScale(new Vector3(1f, 0.8f, 1f), 0.05f).SetEase(Ease.OutBack));
        seq.Append(sr.DOFade(0f, 0.25f).SetEase(Ease.InQuad));

        seq.OnComplete(() => onComplete?.Invoke());
    }
}

[System.Serializable]
public class HasiyoSkillAnimation : VFXAnimationStrategy
{
    [Tooltip("0 = Meteo, 1 = Ice, 2 = Plant")]
    public int skillNum = 0;

    public override void PlaySequence(Transform t, SpriteRenderer sr,
    Vector3 attackerPos, Vector3 targetPos, float hitDelay, Color color, Action onHit, Action onComplete)
    {
        Sequence seq = DOTween.Sequence();
        if (hitDelay > 0) seq.AppendInterval(hitDelay);

        switch (skillNum)
        {
            case 0:
                Vector3 startPos = targetPos + Vector3.up * 4f;
                t.position = startPos;
                t.localScale = Vector3.one;
                sr.color = color;
                if (hitDelay > 0) seq.AppendInterval(hitDelay);
                seq.Append(t.DOMove(targetPos, 0.5f).SetEase(Ease.InExpo));
                seq.AppendCallback(() => onHit?.Invoke());

                Sequence impactSeq = DOTween.Sequence();
                impactSeq.Append(t.DOScale(new Vector3(1.3f, 0.2f, 1f), 0.15f).SetEase(Ease.OutQuad));
                impactSeq.Join(t.DOMoveY(targetPos.y - 0.2f, 0.15f));
                impactSeq.Join(sr.DOFade(0f, 0.15f));

                seq.Append(impactSeq);
                break;

            case 1:
                Vector3 feetPos = targetPos + Vector3.down * 0.5f;
                t.position = feetPos;
                t.localScale = new Vector3(1f, 0f, 1f);
                sr.color = new Color(color.r, color.g, color.b, 0f);

                seq.AppendCallback(() => onHit?.Invoke());

                seq.Append(t.DOScaleY(1f, 1f).SetEase(Ease.OutCubic));
                seq.Join(sr.DOFade(0.7f, 1f));
                seq.Join(t.DOMoveY(targetPos.y, 1f).SetEase(Ease.OutCubic));
                seq.AppendInterval(2f);
                seq.Append(sr.DOFade(0f, 0.3f));

                break;

            case 2:
                t.position = targetPos + Vector3.down * 0.2f;
                t.localScale = new Vector3(1.2f, 0.2f, 1f);
                sr.color = new Color(color.r, color.g, color.b, 0f);

                seq.AppendCallback(() => onHit?.Invoke());

                seq.Append(t.DOScaleY(1.2f, 0.5f).SetEase(Ease.OutBack));
                seq.Join(sr.DOFade(1f, 0.5f));
                seq.AppendInterval(1f);
                seq.Append(sr.DOFade(0f, 0.3f));

                break;
        }

        seq.OnComplete(() => onComplete?.Invoke());
    }
}