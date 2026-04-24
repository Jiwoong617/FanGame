using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Runtime.InteropServices;

public enum GameState
{
    MainMenu,
    Battle,
    Reward,
    MapSelect,
    Event,
    Rest,
    GameOver,
    Ending,
}

public class GameManager : MonoBehaviour
{
    #region Managers

    private static GameManager instance = null;
    public static GameManager Instance { get { return instance; } }
    public static InputManager Input { get; private set;  }
    public static MySceneManager Scene { get; private set; }
    public static BattleManager Battle { get; private set; }
    public static RewardManager Reward { get; private set; }
    public static MapManager Map { get; private set; }
    public static EventManager Event { get; private set; }
    public static RestManager Rest { get; private set; }
    public static InventoryManager Inventory { get; private set; }
    public static VFXManager VFX { get; private set; }
    public static SoundManager Sound { get; private set; }
    public static SpriteDataManager SpriteData { get; private set; }
    #endregion

    public GameState State { get; private set; } = GameState.MainMenu;
    public event Action<GameState> OnGameStateChanged;

    // Selected Data
    public PlayerData SelectedPlayerClass { get; private set; }

    // In-Game Objects
    public PlayerUnit Player { get; private set; }
    private Transform playerAnchor;

    public NodeType CurrentBattleType { get; private set; }

    //TEST 용
    [SerializeField] private PlayerData testUnitData;

    [Header("Stages")]
    [SerializeField] private List<StageData> stageList;
    public int CurrentStageIndex { get; private set; } = 0;
    public StageData currentStageData => stageList[CurrentStageIndex];

#if UNITY_STANDALONE_WIN
    [DllImport("AspectWindow")]
    static extern void HookUnityWindow();

    [DllImport("AspectWindow")]
    static extern void SetAspectRatio(float ratio);
#endif

    private void Awake()
    {
        SetPlayerData(testUnitData);

        Init();
        Application.targetFrameRate = 60;
    }

    private void Start()
    {
#if UNITY_STANDALONE_WIN
        SetAspectRatio(16f / 9f);
        HookUnityWindow();
#endif

        // AudioMixer.SetFloat은 Awake 타이밍에 무시되는 Unity 버그가 있어 Start에서 적용
        Sound.ApplySavedVolumes();
        

        // TODO : 지울거 - 테스트용 아이템 적용
        // if (testInventoryItems != null)
        // {
        //     foreach (var item in testInventoryItems)
        //     {
        //         item.Apply(Player);
        //         Inventory.AddItem(item);
        //     }
        //     testInventoryItems.Clear();
        // }
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
        Input?.OnUpdate();

        switch (State)
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
        Input = new InputManager();
        Scene = new MySceneManager();
        Battle = new BattleManager();
        Reward = new RewardManager();
        Reward.Init();
        Map = new MapManager();
        Event = new EventManager();
        Rest = new RestManager();
        Inventory = new InventoryManager();
        VFX = new VFXManager();
        VFX.Init();
        SpriteData = new SpriteDataManager();
        SpriteData.Init();
        Sound = new SoundManager();
        Sound.Init();


        // 전투 -> (승리) -> 보상 (마지막 스테이지 보스면 바로 엔딩)
        Battle.OnBattleWon += () =>
        {
            bool isLastStage = CurrentStageIndex >= stageList.Count - 1;
            bool isBoss = CurrentBattleType == NodeType.Boss;
            if (isLastStage && isBoss)
            {
                ChangeState(GameState.Ending);
                Scene.LoadScene(SceneType.Ending);
            }
            else
            {
                ChangeState(GameState.Reward);
            }
        };
        // 전투 -> (패배) -> 게임오버
        Battle.OnPlayerDead += () => ChangeState(GameState.GameOver);


        Reward.OnRewardSelected += () => ChangeState(GameState.MapSelect);
        Rest.OnRestFinished += () => ChangeState(GameState.MapSelect);
        Event.OnEventFinished += () => ChangeState(GameState.MapSelect);
    }


    public void SetPlayerData(PlayerData playerData)
    {
        if (playerData == null) return;

        SelectedPlayerClass = playerData;
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
        else if(scene.name == "MainScene")
        {
            ChangeState(GameState.MainMenu);
        }
        else if(scene.name == "EndingScene")
        {
            ChangeState(GameState.Ending);
        }
    }

    private void SpawnPlayer()
    {
        if (SelectedPlayerClass == null || SelectedPlayerClass.prefab == null) return;
        if (playerAnchor == null) return;

        GameObject go = Instantiate(SelectedPlayerClass.prefab, playerAnchor.position, playerAnchor.rotation);
        Player = go.GetComponent<PlayerUnit>();
        Player.Init(SelectedPlayerClass);

        Event.LoadEvents(SelectedPlayerClass.name);
    }

    // MapManager에서 노드 선택 시 호출
    public void ProcessNode(MapNode node)
    {
        Debug.Log($"[GameManager] ProcessNode: {node.nodeType} at Floor {node.y}");
        switch (node.nodeType)
        {
            case NodeType.Monster:
            case NodeType.Elite:
            case NodeType.Boss:
                CurrentBattleType = node.nodeType;

                if (node.content is BattleContent battleContent)
                    Battle.SetupBattle(Player, battleContent.enemies);
                ChangeState(GameState.Battle);
                break;
            case NodeType.Event:
                ChangeState(GameState.Event);
                break;
            case NodeType.Rest:
                ChangeState(GameState.Rest);
                break;
        }
    }

    public void ChangeState(GameState next)
    {
        State = next;
        Debug.Log($"[GameManager] ChangeState: {State}");

        OnGameStateChanged?.Invoke(next);

        switch (next)
        {
            case GameState.MainMenu:
                break;

            case GameState.Battle:
                Map.HideMapUI();
                Battle.StartBattle();
                break;

            case GameState.Reward:
                Reward.ShowRewardUI(CurrentBattleType);
                break;

            case GameState.MapSelect:
                Battle.CleanupBattle(); // 전투 정리

                if (Map.mapGrid == null)
                    Map.GenerateMap(currentStageData);
                else if (Map.IsCleared)
                {
                    NextStage();
                    return;
                }

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

            case GameState.Ending:
                Debug.Log("Ending");
                break;
        }
    }

    private void NextStage()
    {
        CurrentStageIndex++;

        OnGameStateChanged?.Invoke(GameState.MapSelect);
        // 플레이어 상태 회복
        Player.FullyRestore();
        // 2스테이지
        Map.GenerateMap(currentStageData);

        Map.ShowMapUI();
    }

    public void RevivePlayer()
    {
        if (Player == null) return;

        var gameOverUI = FindFirstObjectByType<GameOverUI>();
        if (gameOverUI != null)
            gameOverUI.ForceHide();

        Player.Revive();

        State = GameState.Battle;
        OnGameStateChanged?.Invoke(GameState.Battle);
        Battle.ResumeBattle();
    }

    public void ResetGame()
    {
        Inventory.ClearInventory();
        Battle.CleanupBattle();
        Map.ClearMapData();
        VFX.ClearPools();
        CurrentStageIndex = 0;

        if (Player != null)
        {
            Destroy(Player.gameObject);
            Player = null;
        }

        ChangeState(GameState.MainMenu);
        Reward.Init();
    }
}
