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

public class EventContent : NodeContent
{
    public EventData eventData;

    public EventContent(EventData evnetData)
    {
        this.eventData = evnetData;
    }
}