using System.Collections.Generic;

public abstract class NodeContent { }

public class BattleContent : NodeContent
{
    public List<UnitData> enemies = new List<UnitData>();

    public BattleContent(List<UnitData> enemies)
    {
        this.enemies = enemies;
    }
}

// TODO : 중복 처리를 위해 사용하지 않기로 했음
//public class EventContent : NodeContent
//{
//    public EventData eventData;

//    public EventContent(EventData evnetData)
//    {
//        this.eventData = evnetData;
//    }
//}