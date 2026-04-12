using UnityEngine;

public class CharacterHorizontalWave : MonoBehaviour
{
    [SerializeField] private float frequency = 2.0f; // 속도
    [SerializeField] private float amplitude = 0.05f; // 변형 폭 (5% 정도가 적당)

    void Update()
    {
        // 시간 t에 따른 사인 값 계산: sin(2 * pi * f * t)
        float wave = Mathf.Sin(Time.time * frequency);

        // 좌우(x)로만 늘어났다 줄어들었다 하는 로직
        // 원본 스케일이 1.0일 때, 0.95 ~ 1.05 사이를 왕복
        transform.localScale = new Vector3(1.0f + wave * amplitude, 1.0f, 1.0f);
    }
}
