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

    void Awake()
    {
        m_TextComponent = GetComponent<TMP_Text>();
    }

    void Update()
    {
        if (m_TextComponent == null || radius == 0) return;

        // 텍스트 메쉬 강제 업데이트 (최신 상태 가져오기)
        m_TextComponent.ForceMeshUpdate();
        TMP_TextInfo textInfo = m_TextComponent.textInfo;
        int characterCount = textInfo.characterCount;

        if (characterCount == 0) return;

        for (int i = 0; i < characterCount; i++)
        {
            // 안 보이는 글자(공백 등)는 스킵
            if (!textInfo.characterInfo[i].isVisible) continue;

            int vertexIndex = textInfo.characterInfo[i].vertexIndex;
            int materialIndex = textInfo.characterInfo[i].materialReferenceIndex;
            Vector3[] vertices = textInfo.meshInfo[materialIndex].vertices;

            // 해당 글자의 중심점 X 좌표 구하기
            Vector3 charMidBaselinePos = new Vector2(
                (vertices[vertexIndex + 0].x + vertices[vertexIndex + 2].x) / 2,
                textInfo.characterInfo[i].baseLine);

            // 중심점을 기준으로 각도 계산 (원의 둘레 공식 활용)
            float angle = charMidBaselinePos.x / radius;

            // 각 정점(Vertex) 회전 및 이동 적용
            for (int j = 0; j < 4; j++)
            {
                Vector3 vert = vertices[vertexIndex + j];

                // 1. 기준점을 0,0으로 맞추기 위해 중심 좌표를 뺌
                vert -= charMidBaselinePos;

                // 2. 글자 자체를 각도에 맞춰 회전
                float cos = Mathf.Cos(-angle);
                float sin = Mathf.Sin(-angle);
                Vector3 rotatedVert = new Vector3(
                    vert.x * cos - vert.y * sin,
                    vert.x * sin + vert.y * cos,
                    vert.z
                );

                // 3. 지정한 반지름(Radius) 궤도 위로 이동
                rotatedVert += new Vector3(Mathf.Sin(angle) * radius, Mathf.Cos(angle) * radius - radius, 0);

                vertices[vertexIndex + j] = rotatedVert;
            }
        }

        // 변경된 형태를 실제 화면에 적용
        for (int i = 0; i < textInfo.materialCount; i++)
        {
            if (textInfo.meshInfo[i].mesh != null)
            {
                textInfo.meshInfo[i].mesh.vertices = textInfo.meshInfo[i].vertices;
                m_TextComponent.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
            }
        }
    }
}