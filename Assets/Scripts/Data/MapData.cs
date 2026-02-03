using System.Collections.Generic;

public enum NodeType
{
    Monster,
    Elite,
    Rest,
    Event,
    Boss,
    Start
}

public enum NodeStatus
{
    Locked,
    Available,
    Visited,
}

public class MapNode
{
    public int x; // 가로 위치 (라인)
    public int y; // 세로 위치 (층, Floor)
    public NodeType nodeType;
    public NodeStatus status;

    public List<MapNode> incoming = new List<MapNode>(); //이전
    public List<MapNode> outgoing = new List<MapNode>(); //다음

    public MapNode(int x, int y, NodeType type)
    {
        this.x = x;
        this.y = y;
        this.nodeType = type;
        this.status = NodeStatus.Locked;
    }
}
