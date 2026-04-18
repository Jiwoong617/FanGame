using DG.Tweening;
using UnityEngine;

public enum CutAnimType
{
    FromBottom,
    FromLeft,
    FromRight,
    FromTop,
    Pop,
    FadeIn,
}

public class EndingCutConfig : MonoBehaviour
{
    public CutAnimType animType = CutAnimType.FromBottom;
    public float floatDistance = 50f;
    public float animDuration = 0.5f;
    public Ease ease = Ease.OutBack;

    public void Init()
    {
        if (animType == CutAnimType.Pop)
            transform.localScale = Vector3.one * 0.7f;
        else
            transform.localScale = Vector3.one;

        GetComponent<CanvasGroup>().alpha = 0f;
        gameObject.SetActive(false);
    }

    public void Play()
    {
        gameObject.SetActive(true);

        RectTransform rt = GetComponent<RectTransform>();
        CanvasGroup cg   = GetComponent<CanvasGroup>();

        cg.DOFade(1f, animDuration);

        if (animType == CutAnimType.Pop)
        {
            rt.localScale = Vector3.one * 0.7f;
            rt.DOScale(Vector3.one, animDuration).SetEase(ease);
        }
        else if (animType == CutAnimType.FadeIn)
        {
            // 위치/스케일 변화 없이 페이드인만
        }
        else
        {
            Vector2 destPos = rt.anchoredPosition;
            Vector2 offset  = animType switch
            {
                CutAnimType.FromBottom => Vector2.down  * floatDistance,
                CutAnimType.FromTop    => Vector2.up    * floatDistance,
                CutAnimType.FromLeft   => Vector2.left  * floatDistance,
                CutAnimType.FromRight  => Vector2.right * floatDistance,
                _                      => Vector2.zero,
            };
            rt.anchoredPosition = destPos + offset;
            rt.DOAnchorPos(destPos, animDuration).SetEase(ease);
        }
    }
}
