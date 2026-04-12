using UnityEngine;

public class CharacterSineWave : MonoBehaviour
{
    public float amplitude = 0.1f; // 움직임 범위
    public float frequency = 1f;   // 움직임 속도

    Vector3 startPos;

    void Start() => startPos = transform.localPosition;

    void Update()
    {
        // 상하 부유 효과
        float newY = startPos.y + Mathf.Sin(Time.time * frequency) * amplitude;
        transform.localPosition = new Vector3(startPos.x, newY, startPos.z);

        // 미세한 호흡 효과 (Scale)
        float scaleOffset = 1.0f + Mathf.Sin(Time.time * frequency * 0.8f) * 0.02f;
        transform.localScale = Vector3.one * scaleOffset;
    }
}
