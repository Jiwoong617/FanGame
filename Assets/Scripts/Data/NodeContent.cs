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
    // TODO: 이벤트 관련 데이터 추가
}

public class RestContent : NodeContent
{
    // TODO: 휴식 관련 데이터 추가
}
