using DG.Tweening;
using UnityEngine;

public class CharacterRotate : MonoBehaviour
{
    [Header("회전 설정")]
    [Tooltip("Z축 기준 양수 값이 반시계 방향입니다.")]
    public float rotateAngle = 15f;
    public float duration = 0.15f;

    [Header("반복 주기 설정")]
    public float repeatInterval = 3f;
    private float timer = 0f;

    // 원본 회전값을 저장할 변수
    private Vector3 originalRotation;

    void Start()
    {
        // 게임 시작 시, 오브젝트의 최초 회전값을 기억해 둡니다.
        originalRotation = transform.localEulerAngles;
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= repeatInterval)
        {
            PlayRotation();
            timer = 0f;
        }
    }

    public void PlayRotation()
    {
        // 1. 기존 애니메이션 취소 및 원상태 강제 초기화
        // 애니메이션 도중에 다시 실행되더라도 각도가 꼬이지 않게 만듭니다.
        transform.DOKill();
        transform.localEulerAngles = originalRotation;

        // 2. DOTween 시퀀스(Sequence) 생성
        // 여러 개의 애니메이션을 순서대로 실행할 때 유용합니다.
        Sequence rotSeq = DOTween.Sequence();

        // [1단계] 원래 각도에서 Z축으로 rotateAngle만큼 회전
        rotSeq.Append(transform.DOLocalRotate(originalRotation + new Vector3(0, 0, rotateAngle), duration));

        // [2단계] 다시 원래 각도로 정확하게 원상복구
        rotSeq.Append(transform.DOLocalRotate(originalRotation, duration));
    }
}