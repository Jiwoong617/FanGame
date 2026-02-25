using UnityEngine;
using DG.Tweening;

public class TargetMarker : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Sequence sequence;

    [Header("Settings")]
    [SerializeField] private float startScale = 3.0f;
    [SerializeField] private float targetScale = 1.0f;
    [SerializeField] private Vector3 offset = new Vector3(0, 0f, 0);

    [SerializeField] private float targetingDuration = 0.15f;
    [SerializeField] private float holdingDuration = 0.2f;
    [SerializeField] private float fadingDuration = 0.3f;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        Color c = spriteRenderer.color;
        c.a = 0f;
        spriteRenderer.color = c;
    }

    private void Start()
    {
        GameManager.Battle.SetTargetMarker(this);
    }

    public void PlaySnapEffect(Transform target)
    {
        if (target == null || spriteRenderer == null) return;

        sequence?.Kill();

        transform.position = target.position + offset;
        transform.localScale = Vector3.one * startScale;

        Color c = spriteRenderer.color;
        c.a = 1f;
        spriteRenderer.color = c;

        sequence = DOTween.Sequence();
        sequence.Append(transform.DOScale(targetScale, targetingDuration).SetEase(Ease.OutExpo));
        sequence.AppendInterval(holdingDuration);
        sequence.Append(spriteRenderer.DOFade(0f, fadingDuration));
        sequence.Join(transform.DOScale(targetScale * 1.3f, 0.3f).SetEase(Ease.OutQuad));
    }

    private void OnDestroy()
    {
        transform.DOKill();
    }
}