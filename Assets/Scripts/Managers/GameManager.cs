using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState
{
    MainMenu,
    Battle,
    Reward,
    MapSelect,
    Event,
    Rest,
    GameOver
}

public class GameManager : MonoBehaviour
{
    private static GameManager instance = null;
    public static GameManager Instance { get { return instance; } }

    #region Managers
    public static MySceneManager Scene { get; private set; }
    public static BattleManager Battle { get; private set; }
    public static RewardManager Reward { get; private set; }
    public static MapManager Map { get; private set; }
    public static EventManager Event { get; private set; }
    public static RestManager Rest { get; private set; }
    #endregion

    private GameState state = GameState.MainMenu;
    
    // Selected Data
    private UnitData selectedPlayerClass;
    private CombatResourceData selectedPlayerResource;

    // In-Game Objects
    private PlayerUnit playerUnitInstance;
    private Transform playerAnchor;


    //TEST 용
    [SerializeField] private UnitData testUnitData;
    [SerializeField] private CombatResourceData testSelectedPlayerResource;
    [SerializeField] private StageData currentStageData;


    private void Awake()
    {
        SetPlayerData(testUnitData, testSelectedPlayerResource);

        Init();
        Application.targetFrameRate = 60;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Update()
    {
        switch (state)
        {
            case GameState.Battle:
                Battle?.OnUpdate();
                break;
        }
    }

    private void Init()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeManagers();
    }

    private void InitializeManagers()
    {
        Scene = new MySceneManager();
        Battle = new BattleManager();
        Reward = new RewardManager();
        // Stage = new StageManager(); // 제거
        Map = new MapManager();
        Event = new EventManager();
        Rest = new RestManager();


        // 전투 -> (승리) -> 보상
        Battle.OnBattleWon += () => ChangeState(GameState.MapSelect);
        // 전투 -> (패배) -> 게임오버
        Battle.OnPlayerDead += () => ChangeState(GameState.GameOver);

        // 보상 선택 완료 -> 맵 선택
        Reward.OnRewardSelected += () => ChangeState(GameState.MapSelect);

        // 휴식/이벤트 완료 -> 맵 선택
        Rest.OnRestFinished += () => ChangeState(GameState.MapSelect);
        Event.OnEventFinished += () => ChangeState(GameState.MapSelect);
    }


    public void SetPlayerData(UnitData playerUnit, CombatResourceData playerResource)
    {
        if (playerUnit == null || playerResource == null) return;

        selectedPlayerClass = playerUnit;
        selectedPlayerResource = playerResource;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "GameScene")
        {
            Debug.Log("[GameManager] GameScene Loaded. Initializing Game Objects...");

            GameObject pAnchorObj = GameObject.Find("PlayerAnchor");

            if (pAnchorObj)
            {
                playerAnchor = pAnchorObj.transform;
                SpawnPlayer();
                Battle.SetEnemyAnchor();
            }
            else
            {
                Debug.LogError("[GameManager] Anchors not found in GameScene!");
            }

            ChangeState(GameState.MapSelect);
        }
    }

    private void SpawnPlayer()
    {
        if (selectedPlayerClass == null || selectedPlayerClass.prefab == null) return;
        if (playerAnchor == null) return;

        GameObject go = Instantiate(selectedPlayerClass.prefab, playerAnchor.position, playerAnchor.rotation);
        playerUnitInstance = go.GetComponent<PlayerUnit>();
        playerUnitInstance.Init(selectedPlayerClass, selectedPlayerResource);
    }

    // MapManager에서 노드 선택 시 호출
    public void ProcessNode(MapNode node)
    {
        Debug.Log($"[GameManager] ProcessNode: {node.nodeType} at Floor {node.y}");

        if (node.content is BattleContent battleContent)
        {
            Battle.SetupBattle(playerUnitInstance, battleContent.enemies);
            ChangeState(GameState.Battle);
        }
        else if (node.content is EventContent eventContent)
        {
            ChangeState(GameState.Event);
        }
        else if (node.content is RestContent restContent)
        {
            ChangeState(GameState.Rest);
        }
        else
        {
            switch (node.nodeType)
            {
                case NodeType.Monster:
                case NodeType.Elite:
                case NodeType.Boss:
                    Debug.LogWarning("Battle node but no BattleContent found.");
                    ChangeState(GameState.Battle);
                    break;
                case NodeType.Rest:
                    ChangeState(GameState.Rest);
                    break;
                case NodeType.Event:
                    ChangeState(GameState.Event);
                    break;
            }
        }
    }

    public void ChangeState(GameState next)
    {
        state = next;
        Debug.Log($"[GameManager] ChangeState: {state}");

        switch (next)
        {
            case GameState.MainMenu:
                break;

            case GameState.Battle:
                Map.HideMapUI();
                Battle.StartBattle();
                break;

            case GameState.Reward:
                Reward.ShowRewardUI();
                break;

            case GameState.MapSelect:
                Battle.CleanupBattle(); // 전투 정리
                if (Map.mapGrid == null)
                    Map.GenerateMap(currentStageData);
                Map.OnClearMap();
                Map.ShowMapUI();
                break;
            
            case GameState.Event:
                Map.HideMapUI();
                Event.StartEvent();
                break;

            case GameState.Rest:
                Map.HideMapUI();
                Rest.StartRest();
                break;

            case GameState.GameOver:
                Debug.Log("GAME OVER");
                break;
        }
    }
}
