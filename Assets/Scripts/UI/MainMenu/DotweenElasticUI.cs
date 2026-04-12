using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class DirectionalElasticUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private Vector2 originalScale;

    [Header("Stretch Settings")]
    [SerializeField] private float stretchMultiplier = 0.004f; // 늘어나는 민감도 (조금 더 예민하게)
    [SerializeField] private float maxStretchFactor = 3.0f;    // 최대 늘어남 배수

    [Header("Elastic Return Settings")]
    [SerializeField] private float returnDuration = 0.6f;     // 복귀 시간 (빠르게)
    [SerializeField] private float amplitude = 2.0f;          // 진동폭 (크게)
    [SerializeField] private float period = 0.25f;            // 주기 (탱글탱글하게)

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        originalScale = rectTransform.localScale;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        rectTransform.DOKill(); // 드래그 시작 시 이전 트윈 중지
    }

    public void OnDrag(PointerEventData eventData)
    {
        // 1. 드래그 벡터 계산 (중심 기준)
        Vector2 dragVector = eventData.position - (Vector2)rectTransform.position;
        float distance = dragVector.magnitude;

        // 2. 방향성 스케일 계산
        float stretchForce = distance * stretchMultiplier;

        // 중요: 드래그 벡터의 X, Y 성분에 비례하여 개별적으로 늘림 (비회전 방식의 한계)
        // normalized 값을 사용하여 방향 가중치를 둡니다.
        Vector2 directionNormalized = dragVector.normalized;

        // 절대값으로 방향 가중치 계산 (0~1 사이)
        float weightX = Mathf.Abs(directionNormalized.x);
        float weightY = Mathf.Abs(directionNormalized.y);

        // 기본 1.0f에 방향 가중치를 곱한 힘을 더해 최종 스케일 계산
        // Clamp를 사용하여 무한정 늘어나는 것을 방지
        float targetSx = Mathf.Clamp(1f + (stretchForce * weightX), 1f, maxStretchFactor);
        float targetSy = Mathf.Clamp(1f + (stretchForce * weightY), 1f, maxStretchFactor);

        // 가로축으로 당겨질 땐 세로축을 살짝 줄여주는 고무줄 효과 적용 (선택 사항)
        if (weightX > weightY) // 가로 지배적
            targetSy = 1f / targetSx;
        else // 세로 지배적
            targetSx = 1f / targetSy;

        // 최종 스케일 적용 (회전 없음)
        rectTransform.localScale = new Vector2(originalScale.x * targetSx, originalScale.y * targetSy);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // 3. 고탄성 복귀 (스케일만)
        rectTransform.DOScale(originalScale, returnDuration)
            .SetEase(Ease.OutElastic, amplitude, period);
    }
}