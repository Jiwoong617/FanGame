using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

public class EndingPage : MonoBehaviour
{
    private List<RectTransform> cuts = new List<RectTransform>();
    private int currentCutIndex = -1;

    [Header("Animation Settings")]
    [SerializeField] private float floatDistance = 50f; // 아래에서 떠오르는 거리
    [SerializeField] private float animDuration = 0.5f; // 연출 시간

    public void Init()
    {
        currentCutIndex = 0;
        cuts.Clear();

        if (cuts.Count == 0)
        {
            foreach (Transform child in transform)
            {
                RectTransform rt = child.GetComponent<RectTransform>();
                if (rt != null)
                    cuts.Add(rt);
            }
        }

        foreach (var cut in cuts)
        {
            var config = cut.GetComponent<EndingCutConfig>();
            if (config != null)
            {
                config.Init();
            }
            else
            {
                cut.localScale = Vector3.one * 1.1f;
                cut.GetComponent<CanvasGroup>().alpha = 0f;
                cut.gameObject.SetActive(false);
            }
        }
    }

    public bool ShowNextCut()
    {
        if (currentCutIndex >= cuts.Count)
            return false;

        RectTransform targetCut = cuts[currentCutIndex];
        if(targetCut != null)
        {
            EndingCutConfig config = targetCut.GetComponent<EndingCutConfig>();
            if (config != null)
            {
                config.Play();
            }
            else
            {
                // EndingCutConfig가 없을 경우 기본 연출
                targetCut.gameObject.SetActive(true);
                Vector2 destPos = targetCut.anchoredPosition;
                targetCut.anchoredPosition = destPos + Vector2.down * floatDistance;
                targetCut.GetComponent<CanvasGroup>().DOFade(1f, animDuration);
                targetCut.DOAnchorPos(destPos, animDuration).SetEase(Ease.OutBack);
            }
        }

        currentCutIndex++;

        return true;
    }

    public bool IsFinished()
    {
        return currentCutIndex >= cuts.Count;
    }
}