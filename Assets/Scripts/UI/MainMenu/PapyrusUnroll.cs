using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

public class PapyrusFixedRoll : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI Elements")]
    [SerializeField] private RectTransform openImageRect;   // 2번 이미지 (OpenImage)
    [SerializeField] private CanvasGroup closedRollGroup;   // 1번 이미지 (ClosedRoll)

    [Header("Animation Settings")]
    [SerializeField] private float duration = 0.4f;
    [SerializeField] private Ease moveEase = Ease.OutCubic;

    private float scrollWidth;

    void Awake()
    {
        // 2번 이미지의 가로 길이를 미리 파악
        scrollWidth = openImageRect.rect.width;

        // 초기 상태: 2번 이미지는 왼쪽으로 완전히 숨겨짐 (X = -가로길이)
        openImageRect.anchoredPosition = new Vector2(-scrollWidth, 0);
        closedRollGroup.alpha = 1f;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        KillAllTweens();

        // 1. 1번 이미지는 고정된 상태에서 대기 (나중에 숨김)
        closedRollGroup.alpha = 1f;

        // 2. 2번 이미지(종이)가 왼쪽(-Width)에서 오른쪽(0)으로 슬라이드하며 나옴
        openImageRect.DOAnchorPosX(0, duration)
            .SetEase(moveEase)
            .OnUpdate(() => {
                // 종이가 어느 정도 뽑혀 나오면(예: 80% 이상) 1번 뭉치를 서서히 숨김
                if (openImageRect.anchoredPosition.x > -scrollWidth * 0.2f)
                {
                    closedRollGroup.alpha = Mathf.Lerp(closedRollGroup.alpha, 0f, Time.deltaTime * 10f);
                }
            })
            .OnComplete(() => {
                closedRollGroup.alpha = 0f; // 확실히 제거
            });
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        KillAllTweens();

        // 되돌아갈 때는 1번 뭉치가 바로 나타남
        closedRollGroup.alpha = 1f;

        // 2번 이미지(종이)가 다시 왼쪽(-Width)으로 숨어 들어감
        openImageRect.DOAnchorPosX(-scrollWidth, duration).SetEase(moveEase);
    }

    private void KillAllTweens()
    {
        openImageRect.DOKill();
        closedRollGroup.DOKill();
    }
}