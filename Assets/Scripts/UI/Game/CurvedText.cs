using UnityEngine;
using TMPro;

// 게임을 실행하지 않아도 Scene 화면에서 바로 휘어짐을 확인할 수 있게 해줍니다.
[ExecuteAlways]
[RequireComponent(typeof(TMP_Text))]
public class CurvedText : MonoBehaviour
{
    [Header("곡선 설정")]
    [Tooltip("값이 작을수록 동그랗게 말리고, 클수록 완만해집니다. 음수면 아래로 휨")]
    public float radius = 500f;

    private TMP_Text m_TextComponent;
    private float m_OldRadius = float.MaxValue; // 이전 프레임의 반지름 저장용
    private bool m_ForceUpdate = false; // 강제 업데이트 플래그

    void Awake()
    {
        m_TextComponent = GetComponent<TMP_Text>();
    }

    void OnEnable()
    {
        // 텍스트가 변경될 때 발생하는 TMPro 이벤트 구독
        TMPro_EventManager.TEXT_CHANGED_EVENT.Add(OnTextChanged);
    }

    void OnDisable()
    {
        TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(OnTextChanged);
    }

    void OnTextChanged(Object obj)
    {
        if (obj == m_TextComponent)
            m_ForceUpdate = true;
    }

    void Update()
    {
        if (m_TextComponent == null) return;

        // 1. 반지름(Radius) 값이 인스펙터나 코드로 변경되었는지 체크
        // 2. 텍스트가 변경되어 강제 업데이트(m_ForceUpdate)가 필요한지 체크
        if (!Mathf.Approximately(radius, m_OldRadius) || m_ForceUpdate)
        {
            UpdateCurve();
        }
    }

    void UpdateCurve()
    {
        // 반지름이 0이면 계산 불가 (Divide by zero 방지)
        if (radius == 0) return;

        // 텍스트 메쉬를 최신 상태로 갱신 (기본 직선 배치 상태로 리셋)
        m_TextComponent.ForceMeshUpdate();

        TMP_TextInfo textInfo = m_TextComponent.textInfo;
        int characterCount = textInfo.characterCount;

        if (characterCount == 0) return;

        // 모든 글자의 정점(Vertex)을 순회하며 위치 조정
        for (int i = 0; i < characterCount; i++)
        {
            if (!textInfo.characterInfo[i].isVisible) continue;

            int vertexIndex = textInfo.characterInfo[i].vertexIndex;
            int materialIndex = textInfo.characterInfo[i].materialReferenceIndex;
            Vector3[] vertices = textInfo.meshInfo[materialIndex].vertices;

            // 글자의 중심점 X 좌표 계산
            Vector3 charMidBaselinePos = new Vector2(
                (vertices[vertexIndex + 0].x + vertices[vertexIndex + 2].x) / 2,
                textInfo.characterInfo[i].baseLine);

            // 중심점을 기준으로 각도 계산 (x좌표 / 반지름)
            float angle = charMidBaselinePos.x / radius;

            float cos = Mathf.Cos(-angle);
            float sin = Mathf.Sin(-angle);

            // 4개의 정점 변환 (좌하, 좌상, 우상, 우하)
            for (int j = 0; j < 4; j++)
            {
                Vector3 vert = vertices[vertexIndex + j];

                // 1. 기준점을 0,0으로 이동
                vert -= charMidBaselinePos;

                // 2. 회전 적용
                float x = vert.x * cos - vert.y * sin;
                float y = vert.x * sin + vert.y * cos;

                // 3. 곡선 궤도로 이동 (반지름만큼 Y축 이동)
                vert.x = x + Mathf.Sin(angle) * radius;
                vert.y = y + Mathf.Cos(angle) * radius - radius; // 시작 높이를 0에 맞춤
                vert.z = 0; // Z축 평탄화

                vertices[vertexIndex + j] = vert;
            }
        }

        // 변경된 정점 데이터를 메쉬에 적용
        m_TextComponent.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices);

        // 상태값 갱신
        m_OldRadius = radius;
        m_ForceUpdate = false;
    }
}