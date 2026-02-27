using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapUI : UI_Base
{
    private enum RectT
    {
        Content,
        LineContainer,
        NodeContainer
    }

    [Header("References")]
    [SerializeField] private GameObject nodePrefab;
    [SerializeField] private GameObject linePrefab;

    private ScrollRect scrollRect;
    private RectTransform content;
    private RectTransform lineContainer;
    private RectTransform nodeContainer;


    // 생성된 UI 객체 관리
    private List<MapNodeUI> nodeUIs = new List<MapNodeUI>();
    private List<MapLineUI> lines = new List<MapLineUI>();
    private Dictionary<MapNode, MapNodeUI> nodeUiMap = new Dictionary<MapNode, MapNodeUI>();

    private void OnEnable()
    {
        if (GameManager.Map == null || GameManager.Map.mapGrid == null) return;

        if (GameManager.Map.currentNode == null || nodeUIs.Count == 0)
            GenerateMapUI();
        else
            UpdateMapUI();
    }

    protected override void Init()
    {
        Canvas canvas = GetComponent<Canvas>();
        if (canvas != null && canvas.worldCamera == null)
            canvas.worldCamera = Camera.main;

        GameManager.Map.SetMapUI(this);

        Bind<RectTransform>(typeof(RectT));
        content = Get<RectTransform>(RectT.Content);
        lineContainer = Get<RectTransform>(RectT.LineContainer);
        nodeContainer = Get<RectTransform>(RectT.NodeContainer);
        scrollRect = GetComponentInChildren<ScrollRect>();

        gameObject.SetActive(false);
    }

    public void GenerateMapUI()
    {
        ClearMap();

        var mapGrid = GameManager.Map.mapGrid;
        Rect contentRect = content.rect;
        
        // 패딩
        float paddingX = 50f; 
        float paddingY = 50f;

        float availableWidth = contentRect.width - (paddingX * 2);
        float availableHeight = contentRect.height - (paddingY * 2);
        
        int floors = mapGrid.Count;
        int paths = 7; 
        
        // 분할 단위 계산
        float xStep = availableWidth / paths;
        float yStep = availableHeight / floors;

        foreach (var floor in mapGrid)
        {
            foreach (var node in floor)
            {
                GameObject go = Instantiate(nodePrefab, nodeContainer);
                MapNodeUI ui = go.GetComponent<MapNodeUI>();
                
                float xPos = -contentRect.width * 0.5f + paddingX + (node.x * xStep) + (xStep * 0.5f);
                float yPos = -contentRect.height * 0.5f + paddingY + (node.y * yStep) + (yStep * 0.5f);

                RectTransform rect = go.GetComponent<RectTransform>();
                rect.anchoredPosition = new Vector2(xPos, yPos);
                
                ui.Init(node, OnNodeClicked);
                
                nodeUIs.Add(ui);
                nodeUiMap.Add(node, ui);
            }
        }

        DrawLines();
        ScrollToCurrentFloor();
    }

    public void UpdateMapUI()
    {
        foreach (var ui in nodeUIs)
            ui.UpdateState();

        foreach (var line in lines)
            line.UpdateState();

        ScrollToCurrentFloor();
    }

    private void ScrollToCurrentFloor()
    {
        if (scrollRect == null) return;

        if (GameManager.Map.currentNode == null)
        {
            // 시작하면 맨 아래층
            scrollRect.verticalNormalizedPosition = 0f;
        }
        else
        {
            // 현재층으로 스크롤 이동
            int currentFloor = GameManager.Map.currentNode.y;
            int totalFloors = GameManager.Map.mapGrid.Count;
            
            float ratio = (float)currentFloor / (totalFloors - 1);
            scrollRect.verticalNormalizedPosition = ratio;
        }
    }

    private void CreateLine(RectTransform start, RectTransform end, MapNode from, MapNode to)
    {
        GameObject lineObj = Instantiate(linePrefab, lineContainer);
        RectTransform lineRect = lineObj.GetComponent<RectTransform>();
        MapLineUI lineUI = lineObj.GetComponent<MapLineUI>();

        lineUI.Init(from, to);
        lines.Add(lineUI);

        // 두 점 사이 벡터
        Vector2 dir = end.anchoredPosition - start.anchoredPosition;
        float distance = dir.magnitude;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        // 위치 및 회전 설정
        lineRect.sizeDelta = new Vector2(distance, lineRect.sizeDelta.y); // 길이는 거리, 두께는 유지
        lineRect.anchoredPosition = start.anchoredPosition + dir * 0.5f; // 중간 지점
        lineRect.localRotation = Quaternion.Euler(0, 0, angle);
    }

    private void DrawLines()
    {
        var mapGrid = GameManager.Map.mapGrid;

        foreach (var floor in mapGrid)
        {
            foreach (var node in floor)
            {
                if (!nodeUiMap.TryGetValue(node, out MapNodeUI fromUI)) continue;

                foreach (var nextNode in node.outgoing)
                {
                    if (nodeUiMap.TryGetValue(nextNode, out MapNodeUI toUI))
                        CreateLine(fromUI.GetComponent<RectTransform>(), toUI.GetComponent<RectTransform>(), node, nextNode);
                }
            }
        }
    }

    private void OnNodeClicked(MapNode node)
    {
        Debug.Log($"Node Selected: {node.nodeType} at ({node.x}, {node.y})");

        if (GameManager.Map.GetMapTransitionUI() != null)
        {
            GameManager.Map.GetMapTransitionUI().PlayTransition(() =>
            {
                GameManager.Map.SelectNode(node);
                UpdateMapUI();
                Hide();
            });
        }
        else
        {
            GameManager.Map.SelectNode(node);
            UpdateMapUI();
            Hide() ;
        }
    }


    private void ClearMap()
    {
        foreach (var ui in nodeUIs) Destroy(ui.gameObject);
        foreach (var line in lines) Destroy(line.gameObject);
        nodeUIs.Clear();
        lines.Clear();
        nodeUiMap.Clear();
    }
}
