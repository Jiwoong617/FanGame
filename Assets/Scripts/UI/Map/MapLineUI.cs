using UnityEngine;
using UnityEngine.UI;

public class MapLineUI : MonoBehaviour
{
    private Image lineImage;
    private MapNode fromNode;
    private MapNode toNode;

    public void Init(MapNode from, MapNode to)
    {
        lineImage = GetComponent<Image>();
        fromNode = from;
        toNode = to;
        
        UpdateState();
    }

    public void UpdateState()
    {
        if (lineImage == null) return;

        NodeStatus parentStatus = fromNode.status;
        NodeStatus childStatus = toNode.status;

        if (parentStatus == NodeStatus.Visited)
        {
            if (childStatus == NodeStatus.Visited)
                lineImage.color = Color.green; // 이미 지나간 길
            else if (childStatus == NodeStatus.Available)
                lineImage.color = Color.white; // 지금 갈 수 있는 길
            else
                lineImage.color = new Color(0.3f, 0.3f, 0.3f, 0.5f); // 선택하지 않은 길
        }
        else
            lineImage.color = new Color(0.3f, 0.3f, 0.3f, 0.5f); 
    }
}
