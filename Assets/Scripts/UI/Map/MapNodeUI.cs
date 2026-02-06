using UnityEngine;
using UnityEngine.UI;
using TMPro; // 필요시 사용
using System;

public class MapNodeUI : MonoBehaviour
{
    private Image iconImage;
    private Button button;

    [Header("Resources")]
    // TODO
    // 실제로는 ResourceManger나 Atlas에서 가져오는 게 좋음
    [SerializeField] private Sprite monsterSprite;
    [SerializeField] private Sprite eliteSprite;
    [SerializeField] private Sprite shopSprite;
    [SerializeField] private Sprite restSprite;
    [SerializeField] private Sprite eventSprite;
    [SerializeField] private Sprite bossSprite;
    [SerializeField] private Sprite unknownSprite;

    private MapNode nodeData;
    private Action<MapNode> onClickCallback;

    public void Init(MapNode node, Action<MapNode> onClick)
    {
        iconImage = GetComponent<Image>();
        button = GetComponent<Button>();

        this.nodeData = node;
        this.onClickCallback = onClick;

        // 아이콘 설정
        iconImage.sprite = GetSpriteByType(node.nodeType);

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

    private Sprite GetSpriteByType(NodeType type)
    {
        switch (type)
        {
            case NodeType.Monster: return monsterSprite;
            case NodeType.Elite: return eliteSprite;
            case NodeType.Rest: return restSprite;
            case NodeType.Event: return eventSprite;
            case NodeType.Boss: return bossSprite;
            default: return unknownSprite;
        }
    }
}