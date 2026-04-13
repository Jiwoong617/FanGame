using UnityEngine;
using UnityEngine.InputSystem;

public class UIElementFlee : MonoBehaviour
{
    [Header("Settings")]
    public RectTransform boundaryContainer; // 도망칠 영역 (필수!)
    public float fleeSpeed = 1000f;         // 도망가는 힘
    public float detectionRange = 150f;     // 인식 거리
    public float smoothness = 0.15f;        // 관성 속도

    private RectTransform rectTransform;
    private Vector2 currentVelocity;
    private Vector2 targetAnchoredPos;
    private Canvas parentCanvas;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        targetAnchoredPos = rectTransform.anchoredPosition;
        parentCanvas = GetComponentInParent<Canvas>();

        if (boundaryContainer == null)
            boundaryContainer = transform.parent as RectTransform;
    }

    void Update()
    {
        if (Mouse.current == null) return;

        // 1. 마우스 스크린 좌표 가져오기
        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();

        // 2. 캔버스 모드에 따른 카메라 할당 (Overlay면 null, Camera 모드면 worldCamera)
        Camera uiCamera = (parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : parentCanvas.worldCamera;

        // 3. 스크린 좌표를 로컬(RectTransform) 좌표로 변환
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            boundaryContainer, mouseScreenPos, uiCamera, out Vector2 mouseLocalPos))
        {
            float distance = Vector2.Distance(rectTransform.anchoredPosition, mouseLocalPos);

            // 마우스가 감지 범위 안에 들어오면
            if (distance < detectionRange)
            {
                // 반대 방향 벡터 계산
                Vector2 fleeDir = (rectTransform.anchoredPosition - mouseLocalPos).normalized;
                targetAnchoredPos = rectTransform.anchoredPosition + fleeDir * fleeSpeed;
            }

            // 4. 영역 밖으로 나가지 않게 제한 (Clamping)
            targetAnchoredPos = ClampToContainer(targetAnchoredPos);

            // 5. 부드러운 이동 (SmoothDamp)
            rectTransform.anchoredPosition = Vector2.SmoothDamp(
                rectTransform.anchoredPosition, targetAnchoredPos, ref currentVelocity, smoothness);
        }
    }

    private Vector2 ClampToContainer(Vector2 pos)
    {
        // 부모 컨테이너의 크기 안으로 제한
        Rect containerRect = boundaryContainer.rect;
        float halfWidth = rectTransform.rect.width * 0.5f;
        float halfHeight = rectTransform.rect.height * 0.5f;

        float clampedX = Mathf.Clamp(pos.x, containerRect.xMin + halfWidth, containerRect.xMax - halfWidth);
        float clampedY = Mathf.Clamp(pos.y, containerRect.yMin + halfHeight, containerRect.yMax - halfHeight);

        return new Vector2(clampedX, clampedY);
    }
}