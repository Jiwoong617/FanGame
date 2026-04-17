using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MapNodeUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Image iconImage;
    private Button button;

    [Header("Resources")]

    private MapNode nodeData;
    private Action<MapNode> onClickCallback;

    private float defaultScale = 1.0f;
    private float idleScale = 1.1f;  // 숨쉴 때 커지는 크기
    private float hoverScale = 1.2f; // 마우스 올렸을 때 크기

    public void Init(MapNode node, Action<MapNode> onClick)
    {
        iconImage = GetComponent<Image>();
        button = GetComponent<Button>();

        this.nodeData = node;
        this.onClickCallback = onClick;

        // 아이콘 설정
        Sprite loadedIcon = null;
        if (node.nodeType == NodeType.Boss)
        {
            string bossName = (node.content as BattleContent)?.enemies[0]?.unitName;
            if (!string.IsNullOrEmpty(bossName))
                loadedIcon = GameManager.SpriteData.GetSprite(bossName, "Icons/MapNodes");
        }
        if (loadedIcon == null)
            loadedIcon = GameManager.SpriteData.GetSprite(node.nodeType, "Icons/MapNodes");
        
        if (loadedIcon != null)
            iconImage.sprite = loadedIcon;

        // 버튼 이벤트
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClick);

        // 상태 업데이트
        UpdateState();
    }

    public void UpdateState()
    {
        StopAnimation();

        switch (nodeData.status)
        {
            case NodeStatus.Locked:
                button.interactable = false;
                iconImage.color = Color.gray;
                transform.localScale = Vector3.one * 0.9f;
                break;
            case NodeStatus.Available:
                button.interactable = true;
                iconImage.color = Color.white;
                StartIdleAnimation();
                break;
            case NodeStatus.Visited:
                button.interactable = false;
                iconImage.color = Color.blue;
                transform.localScale = Vector3.one * defaultScale;
                break;
        }
    }

    private void StartIdleAnimation()
    {
        transform.DOKill();
        transform.DOScale(idleScale, 0.9f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }

    private void StopAnimation()
    {
        transform.DOKill();
        transform.localScale = Vector3.one * defaultScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (nodeData.status == NodeStatus.Available)
        {
            StopAnimation();
            transform.DOScale(hoverScale, 0.2f).SetEase(Ease.OutBack);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (nodeData.status == NodeStatus.Available)
        {
            StopAnimation();
            StartIdleAnimation();
        }
    }

    private void OnClick()
    {
        onClickCallback?.Invoke(nodeData);
    }

    private void OnDisable()
    {
        transform.DOKill();
    }
}