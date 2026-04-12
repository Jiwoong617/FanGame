using DG.Tweening;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class CharacterPop : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // 메인 화면 진입 시 연출
        Sequence introSeq = DOTween.Sequence();
        introSeq.Append(transform.DOScale(1.4f, 1f).From(0f).SetEase(Ease.OutBack))
                .Join(transform.DORotate(new Vector3(0, 0, 10f), 0.5f).SetLoops(2, LoopType.Yoyo))
                .OnComplete(() => Debug.Log("등장 완료!"))
                .Append(transform.DOScale(1.0f, 1.4f).SetEase(Ease.OutBack))
                .OnComplete(() => Debug.Log("등장 완료!"));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
