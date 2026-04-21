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

[System.Serializable]
public class StatusVFXAnimation : VFXAnimationStrategy
{
    [Tooltip("이펙트가 유지되는 시간")]
    public float duration = 2f;

    [Tooltip("UV 스크롤 속도 (양수: 버프(위로), 음수: 디버프(아래로))")]
    public float scrollSpeed = 2f;

    [Tooltip("이펙트가 표시될 Y축 오프셋 (머리 위, 발 밑 등)")]
    public float yOffset = 1f;

    public override void PlaySequence(Transform t, SpriteRenderer sr, Vector3 attackerPos, Vector3 targetPos, float hitDelay, Color color, Action onHit, Action onComplete)
    {
        t.position = targetPos + new Vector3(0, yOffset, 0);
        t.rotation = Quaternion.identity;
        t.localScale = Vector3.one;

        // 머티리얼 파라미터 조작 (MaterialPropertyBlock 사용 권장)
        // sr.material을 직접 수정하면 인스턴스가 복제되어 메모리 누수가 발생 가능
        // 유니티 2D에서 머티리얼 수치를 바꿀 때는 PropertyBlock이 가장 안전
        MaterialPropertyBlock mpb = new MaterialPropertyBlock();
        sr.GetPropertyBlock(mpb);
        mpb.SetFloat("_ScrollSpeed", scrollSpeed); // 쉐이더에 만들어둔 변수명
        sr.SetPropertyBlock(mpb);

        sr.color = new Color(color.r, color.g, color.b, 0f);

        Sequence seq = DOTween.Sequence();
        if (hitDelay > 0) seq.AppendInterval(hitDelay);

        seq.AppendCallback(() => onHit?.Invoke());

        seq.Append(sr.DOFade(1f, 0.2f));
        //seq.Join(t.DOMoveY(t.position.y + 0.2f, 0.3f).SetEase(Ease.OutQuad));
        seq.AppendInterval(duration);
        seq.Append(sr.DOFade(0f, 0.3f));

        seq.OnComplete(() => onComplete?.Invoke());
    }
}

[System.Serializable]
public class TauntVFXAnimation : VFXAnimationStrategy
{
    public override void PlaySequence(Transform t, SpriteRenderer sr, Vector3 attackerPos, Vector3 targetPos, float hitDelay, Color color, Action onHit, Action onComplete)
    {
        t.position = targetPos + new Vector3(1.5f, 1, 0);
        t.localScale = Vector3.one * 1.5f;
        sr.color = new Color(color.r, color.g, color.b, 0f);

        Sequence seq = DOTween.Sequence();
        if (hitDelay > 0) seq.AppendInterval(hitDelay);

        seq.Append(t.DOScale(Vector3.one, 0.15f).SetEase(Ease.InExpo));
        seq.Join(sr.DOFade(1f, 0.1f));

        seq.AppendInterval(0.3f);

        seq.Append(t.DOScale(Vector3.zero, 0.15f).SetEase(Ease.InBack));
        seq.Join(sr.DOFade(0f, 0.15f));

        seq.OnComplete(() => onComplete?.Invoke());
    }
}

[System.Serializable]
public class IronFortressVFXAnimation : VFXAnimationStrategy
{
    [Tooltip("방패가 나타나는 시간")]
    public float deployTime = 0.15f;

    [Tooltip("사라질 때 얼마나 커질 것인가")]
    public float expandScale = 1.25f;

    [Tooltip("사라지는 시간")]
    public float fadeTime = 0.2f;

    public override void PlaySequence(Transform t, SpriteRenderer sr, Vector3 attackerPos, Vector3 targetPos, float hitDelay, Color color, Action onHit, Action onComplete)
    {
        t.position = targetPos;
        t.localScale = Vector3.zero;
        sr.color = new Color(color.r, color.g, color.b, 0f);

        Sequence seq = DOTween.Sequence();

        if (hitDelay > 0) seq.AppendInterval(hitDelay);

        seq.Append(t.DOScale(Vector3.one, deployTime).SetEase(Ease.OutBack));
        seq.Join(sr.DOFade(0.5f, deployTime));

        seq.AppendCallback(() => onHit?.Invoke());

        seq.AppendInterval(0.1f);

        // 4. 무효화 성공 후 에너지가 퍼지며 사라짐 (확 커지면서 투명해짐)
        seq.Append(t.DOScale(Vector3.one * expandScale, fadeTime).SetEase(Ease.OutQuad));
        seq.Join(sr.DOFade(0f, fadeTime));

        seq.OnComplete(() => onComplete?.Invoke());
    }
}

[System.Serializable]
public class ReflectVFXAnimation : VFXAnimationStrategy
{
    public override void PlaySequence(Transform t, SpriteRenderer sr, Vector3 attackerPos, Vector3 targetPos, float hitDelay, Color color, Action onHit, Action onComplete)
    {
        t.position = targetPos;
        t.localScale = Vector3.one * 0.5f;

        Color startColor = new Color(color.r, color.g, color.b, 0.5f);
        sr.color = startColor;

        Sequence seq = DOTween.Sequence();

        if (hitDelay > 0) seq.AppendInterval(hitDelay);

        seq.Append(t.DOScale(Vector3.one, 0.1f).SetEase(Ease.OutBack));
        seq.Append(t.DOScale(Vector3.one * 1.1f, 0.1f).SetEase(Ease.OutQuad));
        seq.Join(sr.DOFade(0f, 0.1f));

        seq.OnComplete(() => onComplete?.Invoke());
    }
}

[System.Serializable]
public class BabaFireVFXAnimation : VFXAnimationStrategy
{
    public override void PlaySequence(Transform t, SpriteRenderer sr,
        Vector3 attackerPos, Vector3 targetPos, float hitDelay, Color color, Action onHit, Action onComplete)
    {
        t.position = attackerPos;
        t.localScale = Vector3.one;
        sr.color = color;

        Sequence seq = DOTween.Sequence();

        if (hitDelay > 0) seq.AppendInterval(hitDelay);

        seq.Append(t.DOMove(targetPos, 1.5f).SetEase(Ease.InCubic));
        seq.AppendCallback(() => onHit?.Invoke());
        seq.Append(sr.DOFade(0f, 0.2f).SetEase(Ease.InQuad));

        seq.OnComplete(() => onComplete?.Invoke());
    }
}

[System.Serializable]
public class BababExplodeVFXAnimation : VFXAnimationStrategy
{
    public override void PlaySequence(Transform t, SpriteRenderer sr,
        Vector3 attackerPos, Vector3 targetPos, float hitDelay, Color color, Action onHit, Action onComplete)
    {
        t.position = targetPos;
        t.localScale = Vector3.zero;
        sr.color = new Color(color.r, color.g, color.b, 0.5f);

        Sequence seq = DOTween.Sequence();

        if (hitDelay > 0) seq.AppendInterval(hitDelay);

        seq.AppendCallback(() => onHit?.Invoke());

        seq.Append(t.DOScale(new Vector3(4f, 2.5f, 1f), 0.2f).SetEase(Ease.OutExpo));
        seq.Append(sr.DOFade(0f, 0.2f).SetEase(Ease.InQuad));

        seq.OnComplete(() => onComplete?.Invoke());
    }
}
