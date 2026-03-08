using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MapManager
{
    public List<List<MapNode>> mapGrid { get; private set; }
    public MapNode currentNode { get; private set; }
    public bool IsCleared => (currentNode != null && 
        currentNode.nodeType == NodeType.Boss && 
        currentNode.status == NodeStatus.Visited);
    
    // 설정값
    private const int FLOORS = 11; // 전체 층 수 (보스 포함)
    private const int PATHS = 7;   // 가로 폭 (최대 노드 수)
    private const float BRANCH = 0.4f; // 이거 미만이면 2갈래, 초과하면 1갈래

    private MapUI mapUI;
    private MapTransitionUI mapTransitionUI;
    
    public void GenerateMap(StageData stageData)
    {
        currentNode = null;

        //Walker 알고리즘 사용
        mapGrid = new List<List<MapNode>>();
        for (int i = 0; i < FLOORS; i++) mapGrid.Add(new List<MapNode>());

        // 워커 초기화
        // 일단 1층은 4개만 있게 했음
        List<int> startingPositions = Enumerable.Range(0, PATHS).OrderBy(x => Random.value).Take(4).ToList();
        List<MapNode> currentWalkers = new List<MapNode>();
        foreach (int x in startingPositions)
        {
            MapNode node = GetOrCreateNode(x, 0);
            node.nodeType = NodeType.Monster;
            currentWalkers.Add(node);
        }

        // 워커 등반
        for (int y = 0; y < FLOORS - 2; y++)
        {
            List<MapNode> nextWalkers = new List<MapNode>();

            foreach (var walker in currentWalkers)
            {
                // 다음 층 경로 결정 - 왼,앞,오
                int startX = Mathf.Max(0, walker.x - 1);
                int endX = Mathf.Min(PATHS - 1, walker.x + 1);
                
                // 1~2갈래 분기
                int branchCount = (Random.value < BRANCH) ? 2 : 1; 
                
                // 가능한 x 좌표들 중 랜덤 선택
                List<int> possibleNextX = Enumerable.Range(startX, endX - startX + 1).OrderBy(x => Random.value).Take(branchCount).ToList();

                foreach(int nextX in possibleNextX)
                {
                    MapNode nextNode = GetOrCreateNode(nextX, y + 1);
                    Connect(walker, nextNode);
                    if (!nextWalkers.Contains(nextNode)) nextWalkers.Add(nextNode);
                }
            }


            // 층별 노드 수 조정 (2 ~ 4)
            // 과밀도 방지
            while (nextWalkers.Count > 4)
            {
                // 랜덤 노드 제거
                MapNode victim = nextWalkers[Random.Range(0, nextWalkers.Count)];
                nextWalkers.Remove(victim);
                mapGrid[y + 1].Remove(victim);

                var parents = victim.incoming.ToList(); 
                foreach (var parent in parents)
                {
                    parent.outgoing.Remove(victim);
                }
            }
            
            // 고립된 워커 구하기
            foreach(var walker in currentWalkers)
            {
                if (walker.outgoing.Count == 0)
                {
                     // 가장 가까운 x를 가진 워커 찾기
                     var survivor = nextWalkers.OrderBy(n => Mathf.Abs(n.x - walker.x)).First();
                     Connect(walker, survivor);
                }
            }

            // 최소 2개 보장
            int safetyLoop = 0;
            while (nextWalkers.Count < 2 && safetyLoop < 100)
            {
                safetyLoop++;
                // 확산시킬 부모 선택
                MapNode parent = currentWalkers[Random.Range(0, currentWalkers.Count)];
                
                // 좌우 중 빈 곳 찾기
                List<int> candidates = new List<int>();
                if (parent.x > 0) candidates.Add(parent.x - 1);
                if (parent.x < PATHS - 1) candidates.Add(parent.x + 1);
                
                // 이미 노드가 있는 곳은 제외
                candidates.RemoveAll(x => nextWalkers.Any(n => n.x == x));
                
                if (candidates.Count > 0)
                {
                    int spawnX = candidates[Random.Range(0, candidates.Count)];
                    MapNode newNode = GetOrCreateNode(spawnX, y + 1);
                    Connect(parent, newNode);
                    nextWalkers.Add(newNode);
                }
                else
                {
                    //이미 인접한데 다 찼으면 먼데라도 강제 연결
                    int forcedX = -1;
                    for(int k=0; k<PATHS; k++)
                    {
                        if (!nextWalkers.Any(n => n.x == k)) 
                        { 
                            forcedX = k;
                            break; 
                        }
                    }
                    
                    if (forcedX != -1) 
                    {
                        MapNode newNode = GetOrCreateNode(forcedX, y + 1);
                        Connect(parent, newNode);
                        nextWalkers.Add(newNode);
                    }
                }
            }

            currentWalkers = nextWalkers;
        }

        // 마지막층 - 보스
        MapNode bossNode = new MapNode(PATHS / 2, FLOORS - 1, NodeType.Boss);
        mapGrid[FLOORS - 1].Add(bossNode);
        foreach (var walker in currentWalkers)
            Connect(walker, bossNode);

        // 노드 정렬 및 컨텐츠 할당, 이벤트 개수 제한 추가
        Dictionary<MapNode, int> maxEventsPath = new Dictionary<MapNode, int>();

        for (int y = 0; y < FLOORS; y++)
        {
            // x 좌표 순 정렬
            mapGrid[y] = mapGrid[y].OrderBy(n => n.x).ToList();
            
            foreach (var node in mapGrid[y])
            {
                int parentMaxEventCount = 0;
                if (node.incoming.Count > 0)
                {
                    // 부모들 중 최대값 찾기
                    parentMaxEventCount = node.incoming.Max(parent =>
                        maxEventsPath.ContainsKey(parent) ? maxEventsPath[parent] : 0
                    );
                }

                // 이벤트 횟수 제한 체크
                NodeType type = GetRandomNodeType(y);
                if (type == NodeType.Event)
                {
                    if (parentMaxEventCount >= 3)
                        type = Random.value > 0.5f ? NodeType.Monster : NodeType.Elite;
                }

                // 컨텐츠 할당
                node.nodeType = type;
                AssignNodeContent(node, stageData);

                //이벤트 카운드 기록
                int currentCount = parentMaxEventCount + (type == NodeType.Event ? 1 : 0);
                maxEventsPath[node] = currentCount;
            }
        }

        // 1층 활성화
        foreach(var node in mapGrid[0])
            node.status = NodeStatus.Available;
    }

    private void AssignNodeContent(MapNode node, StageData stageData)
    {
        if (stageData == null) return;

        switch (node.nodeType)
        {
            case NodeType.Monster:
                List<UnitData> enemies = new List<UnitData>();
                if (stageData.normalEnemyPool != null && stageData.normalEnemyPool.Count > 0)
                    enemies = stageData.normalEnemyPool[Random.Range(0, stageData.normalEnemyPool.Count)].enemies;
                node.content = new BattleContent(enemies);
                break;
            case NodeType.Elite:
                List<UnitData> elites = new List<UnitData>();
                if (stageData.eliteEnemyPool != null && stageData.eliteEnemyPool.Count > 0)
                    elites = stageData.eliteEnemyPool[Random.Range(0, stageData.eliteEnemyPool.Count)].enemies;
                node.content = new BattleContent(elites);
                break;
            case NodeType.Boss:
                List<UnitData> bosses = new List<UnitData>();
                if (stageData.bossPool != null && stageData.bossPool.Count > 0)
                    bosses = stageData.bossPool[Random.Range(0, stageData.bossPool.Count)].enemies;
                node.content = new BattleContent(bosses);
                break;
            case NodeType.Event:
                node.content = null;
                break;
        }
    }

    private MapNode GetOrCreateNode(int x, int y)
    {
        // 이미 해당 위치에 노드가 있는지 확인
        MapNode existing = mapGrid[y].FirstOrDefault(n => n.x == x);
        if (existing != null) return existing;

        MapNode newNode = new MapNode(x, y, NodeType.Monster); // 타입은 나중에 덮어씌움
        mapGrid[y].Add(newNode);
        return newNode;
    }

    private void Connect(MapNode from, MapNode to)
    {
        if (!from.outgoing.Contains(to)) from.outgoing.Add(to);
        if (!to.incoming.Contains(from)) to.incoming.Add(from);
    }

    private NodeType GetRandomNodeType(int floor)
    {
        if (floor == 0) return NodeType.Monster;        // 1층은 무조건 몬스터
        if (floor == FLOORS - 1) return NodeType.Boss;  // 마지막은 무조건 보스
        if (floor == FLOORS - 2) return NodeType.Rest;  // 보스 전은 휴식
        if (floor == FLOORS / 2) return NodeType.Elite; // 중간에 엘리트 하나

        float r = Random.value;
        if (r < 0.6f) return NodeType.Monster;
        if (r < 0.8f) return NodeType.Event;
        if (r < 0.85f) return NodeType.Rest;
        return NodeType.Elite;
    }

    // 플레이어가 노드를 선택했을 때
    public void SelectNode(MapNode node)
    {
        if (node.status != NodeStatus.Available) return;

        currentNode = node;
        node.status = NodeStatus.Visited;

        // 같은 층의 다른 노드들은 더 이상 선택 불가능하게 잠금
        int currentFloorIdx = node.y;
        if (currentFloorIdx < mapGrid.Count)
        {
            foreach (var siblingNode in mapGrid[currentFloorIdx])
            {
                if (siblingNode != node && siblingNode.status == NodeStatus.Available)
                {
                    siblingNode.status = NodeStatus.Locked; // 혹은 Skipped 등의 별도 상태
                }
            }
        }

        GameManager.Instance.ProcessNode(node);
    }

    // 스테이지 클리어 후 호출
    public void OnClearMap()
    {
        if (currentNode == null) return;
        if (currentNode.outgoing.Count == 0) return; //마지막 보스면 리턴

        foreach(var nextNode in currentNode.outgoing)
        {
            nextNode.status = NodeStatus.Available;
        }

        ShowMapUI();
    }

    public void SetMapUI(MapUI ui)
    {
        if (ui == null) return;

        mapUI = ui;
    }

    public void SetMapTransitionUI(MapTransitionUI ui)
    {
        if (ui == null) return;

        mapTransitionUI = ui;
    }

    public void ShowMapUI() => mapUI.Show();
    public void HideMapUI() => mapUI.Hide();

    public MapTransitionUI GetMapTransitionUI() => mapTransitionUI;

    public void ClearMapData()
    {
        if (mapGrid != null)
        {
            mapGrid.Clear();
            mapGrid = null;
        }
        currentNode = null;
    }
}
