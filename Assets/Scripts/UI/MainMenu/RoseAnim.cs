using UnityEngine;

public class RoseAnim : MonoBehaviour
{
    [Header("연결할 애니메이터")]
    public Animator targetAnimator;

    [Header("애니메이터 파라미터 이름")]
    public string triggerName = "AnimTrigger"; // Animator에 설정한 Trigger 이름과 똑같이 맞춰주세요.

    // 버튼의 OnClick 이벤트에 연결할 함수
    public void PlayAnimation()
    {
        if (targetAnimator != null)
        {
            // 설정된 이름의 Trigger를 작동시킵니다.
            targetAnimator.SetTrigger(triggerName);
        }
        else
        {
            Debug.LogWarning("Animator가 할당되지 않았습니다!");
        }
    }
}
