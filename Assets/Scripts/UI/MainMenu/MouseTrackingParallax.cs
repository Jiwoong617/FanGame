using UnityEngine;
using UnityEngine.InputSystem;

public class MouseTrackingParallax : MonoBehaviour
{
    public float sensitivity = 20f;
    public float smoothTime = 0.2f;

    private Vector3 startPos;
    private Vector3 velocity = Vector3.zero;

    void Start() => startPos = transform.localPosition;

    void Update()
    {
        // 2. 신규 시스템 방식으로 마우스 위치 읽기
        Vector2 mousePos = Mouse.current.position.ReadValue();

        float x = (mousePos.x - Screen.width / 2f) / (Screen.width / 2f);
        float y = (mousePos.y - Screen.height / 2f) / (Screen.height / 2f);

        Vector3 targetPos = startPos + new Vector3(x * sensitivity, y * sensitivity, 0);
        transform.localPosition = Vector3.SmoothDamp(transform.localPosition, targetPos, ref velocity, smoothTime);
    }
}
