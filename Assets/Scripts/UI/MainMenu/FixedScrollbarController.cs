using UnityEngine;
using UnityEngine.UI;

public class FixedScrollbarController : MonoBehaviour
{
    public ScrollRect scrollRect;
    public Scrollbar scrollbar;
    [Range(0f, 1f)]
    public float handleSize = 0.2f;

    void Start()
    {
        if (scrollRect == null || scrollbar == null) return;

        // 1. ScrollRect에서 스크롤바 연결을 끊음 (에디터에서 이미 끊었다면 생략 가능)
        scrollRect.verticalScrollbar = null;

        // 2. 핸들 크기 고정
        scrollbar.size = handleSize;

        // 3. 초기 위치 강제 설정 (컨텐츠가 사라지는 것 방지)
        // 보통 verticalNormalizedPosition 1은 리스트의 맨 위를 의미합니다.
        scrollRect.verticalNormalizedPosition = 1f;
        scrollbar.value = 1f;

        // 4. 이벤트 연결
        scrollbar.onValueChanged.AddListener(OnScrollbarValueChanged);
        scrollRect.onValueChanged.AddListener(OnScrollRectValueChanged);
    }

    void OnScrollbarValueChanged(float value)
    {
        // 방향이 반대라면 1f - value 로 수정해보세요.
        scrollRect.verticalNormalizedPosition = value;
    }

    void OnScrollRectValueChanged(Vector2 value)
    {
        // ScrollRect의 y값 변화를 스크롤바에 전달
        // 이벤트 무한 루프 방지를 위해 값 비교 후 업데이트
        if (Mathf.Abs(scrollbar.value - value.y) > 0.001f)
        {
            scrollbar.value = value.y;
        }
    }

    void LateUpdate()
    {
        // 핸들 크기 강제 유지
        if (scrollbar.size != handleSize)
        {
            scrollbar.size = handleSize;
        }
    }
}