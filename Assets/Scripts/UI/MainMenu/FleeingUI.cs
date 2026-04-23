using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using DG.Tweening;

public class UIElementFlee : MonoBehaviour, IPointerClickHandler
{
    [Header("Settings")]
    public RectTransform boundaryContainer;
    public float fleeSpeed = 1000f;
    public float detectionRange = 150f;
    public float smoothness = 0.15f;

    [Header("Difficulty Limits")]
    private const float MaxFleeSpeed = 1500f;
    private const float MaxDetectionRange = 400f;

    [Header("DOTween Settings")]
    public float fadeDuration = 0.5f;
    public float respawnDelay = 0.5f;

    private RectTransform rectTransform;
    private Vector2 currentVelocity;
    private Vector2 targetAnchoredPos;
    private Vector2 initialPosition;
    private Canvas parentCanvas;
    private CanvasGroup canvasGroup;
    private bool isHidden = false;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        parentCanvas = GetComponentInParent<Canvas>();

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();

        initialPosition = rectTransform.anchoredPosition;
        targetAnchoredPos = initialPosition;

        if (boundaryContainer == null)
            boundaryContainer = transform.parent as RectTransform;
    }

    void Update()
    {
        if (isHidden || Mouse.current == null) return;

        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        Camera uiCamera = (parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : parentCanvas.worldCamera;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            boundaryContainer, mouseScreenPos, uiCamera, out Vector2 mouseLocalPos))
        {
            float distance = Vector2.Distance(rectTransform.anchoredPosition, mouseLocalPos);

            if (distance < detectionRange)
            {
                // 1. 기본 회피 방향 (마우스 반대 방향)
                Vector2 fleeDir = (rectTransform.anchoredPosition - mouseLocalPos).normalized;

                // 2. 사이드 회피(Sliding) 로직 추가
                // 벽 근처에 있는지 확인 (약간의 마진 20f 적용)
                Rect containerRect = boundaryContainer.rect;
                float halfW = rectTransform.rect.width * 0.5f;
                float halfH = rectTransform.rect.height * 0.5f;
                float margin = 20f;

                bool isAtLeft = rectTransform.anchoredPosition.x <= containerRect.xMin + halfW + margin;
                bool isAtRight = rectTransform.anchoredPosition.x >= containerRect.xMax - halfW - margin;
                bool isAtBottom = rectTransform.anchoredPosition.y <= containerRect.yMin + halfH + margin;
                bool isAtTop = rectTransform.anchoredPosition.y >= containerRect.yMax - halfH - margin;

                // 상하단 벽에 막혔는데 마우스가 위/아래에서 접근하면 좌우로 회피 가중치 부여
                if (isAtTop || isAtBottom)
                {
                    float horizontalPush = (rectTransform.anchoredPosition.x > mouseLocalPos.x) ? 1.2f : -1.2f;
                    fleeDir.x += horizontalPush;
                }
                // 좌우측 벽에 막혔는데 마우스가 좌/우에서 접근하면 상하로 회피 가중치 부여
                if (isAtLeft || isAtRight)
                {
                    float verticalPush = (rectTransform.anchoredPosition.y > mouseLocalPos.y) ? 1.2f : -1.2f;
                    fleeDir.y += verticalPush;
                }

                // 최종 방향 정규화 후 타겟 지점 계산
                targetAnchoredPos = rectTransform.anchoredPosition + fleeDir.normalized * fleeSpeed;
            }

            targetAnchoredPos = ClampToContainer(targetAnchoredPos);

            rectTransform.anchoredPosition = Vector2.SmoothDamp(
                rectTransform.anchoredPosition, targetAnchoredPos, ref currentVelocity, smoothness);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isHidden) return;

        fleeSpeed = Mathf.Min(fleeSpeed + 100f, MaxFleeSpeed);
        detectionRange = Mathf.Min(detectionRange + 20f, MaxDetectionRange);

        Sequence respawnSequence = DOTween.Sequence();

        respawnSequence
            .AppendCallback(() => {
                isHidden = true;
                canvasGroup.blocksRaycasts = false;
            })
            .Append(canvasGroup.DOFade(0, fadeDuration))
            .AppendInterval(respawnDelay)
            .AppendCallback(() => {
                rectTransform.anchoredPosition = initialPosition;
                targetAnchoredPos = initialPosition;
                currentVelocity = Vector2.zero;
            })
            .Append(canvasGroup.DOFade(1, fadeDuration))
            .OnComplete(() => {
                isHidden = false;
                canvasGroup.blocksRaycasts = true;
            });
    }

    private Vector2 ClampToContainer(Vector2 pos)
    {
        Rect containerRect = boundaryContainer.rect;
        float halfWidth = rectTransform.rect.width * 0.5f;
        float halfHeight = rectTransform.rect.height * 0.5f;

        float clampedX = Mathf.Clamp(pos.x, containerRect.xMin + halfWidth, containerRect.xMax - halfWidth);
        float clampedY = Mathf.Clamp(pos.y, containerRect.yMin + halfHeight, containerRect.yMax - halfHeight);

        return new Vector2(clampedX, clampedY);
    }
}