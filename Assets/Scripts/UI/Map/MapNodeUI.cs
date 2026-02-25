using UnityEngine;
using UnityEngine.UI;
using TMPro; // 필요시 사용
using System;

public class MapNodeUI : MonoBehaviour
{
    private Image iconImage;
    private Button button;

    [Header("Resources")]

    private MapNode nodeData;
    private Action<MapNode> onClickCallback;

    public void Init(MapNode node, Action<MapNode> onClick)
    {
        iconImage = GetComponent<Image>();
        button = GetComponent<Button>();

        this.nodeData = node;
        this.onClickCallback = onClick;

        // 아이콘 설정
        Sprite loadedIcon = GameManager.SpriteData.GetSprite(node.nodeType, "Icons/MapNodes");
        if (loadedIcon != null)
        {
            iconImage.sprite = loadedIcon;
        }

        // 버튼 이벤트
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClick);

        // 상태 업데이트
        UpdateState();
    }

    public void UpdateState()
    {
        switch (nodeData.status)
        {
            case NodeStatus.Locked:
                button.interactable = false;
                iconImage.color = Color.gray;
                break;
            case NodeStatus.Available:
                button.interactable = true;
                iconImage.color = Color.white;
                break;
            case NodeStatus.Visited:
                button.interactable = false;
                iconImage.color = Color.blue;
                break;
        }
    }

    private void OnClick()
    {
        onClickCallback?.Invoke(nodeData);
    }
}