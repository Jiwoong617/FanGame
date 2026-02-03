using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState
{
    MainMenu,
    Battle,
    Reward,
    MapSelect,
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
    public static StageManager Stage { get; private set; }
    public static MapManager Map { get; private set; }
    #endregion

    private GameState state = GameState.MainMenu;
    
    // Selected Data
    private UnitData selectedPlayerClass;
    private CombatResourceData selectedPlayerResource;

    //TEST 용
    [SerializeField] private UnitData testUnitData;
    [SerializeField] private CombatResourceData testSelectedPlayerResource;

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
            Debug.Log("[GameManager] GameScene Loaded. Initializing Stage...");

            GameObject pAnchorObj = GameObject.Find("PlayerAnchor");
            GameObject eAnchorObj = GameObject.Find("EnemyAnchor"); // 이름 변경

            if (pAnchorObj && eAnchorObj)
            {
                Stage.SetAnchors(pAnchorObj.transform, eAnchorObj.transform);
                Stage.SpawnPlayer(selectedPlayerClass, selectedPlayerResource);
            }
            else
            {
                Debug.LogError("[GameManager] Anchors not found in GameScene!");
            }

            ChangeState(GameState.MapSelect);
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
        Stage = new StageManager();
        Map = new MapManager();

        
        // 전투 -> (승리) -> 보상
        Battle.OnBattleWon += () => ChangeState(GameState.Reward);
        // 전투 -> (패배) -> 게임오버
        Battle.OnPlayerDead += () => ChangeState(GameState.GameOver);
        // 보상 선택 완료 -> 맵 선택
        Reward.OnRewardSelected += () => ChangeState(GameState.MapSelect);
        // 맵 선택 완료 -> 다음 전투 (또는 이벤트)
        Stage.OnStageSelected += (stageInfo) => 
        {
            Debug.Log($"Go to {stageInfo}");
            // TODO: stageInfo에 따라 전투인지 이벤트인지 분기 처리 필요
            ChangeState(GameState.Battle); 
        };
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
                Battle.StartBattle();
                break;

            case GameState.Reward:
                Reward.ShowRewardUI();
                break;

            case GameState.MapSelect:
                if (Map.mapGrid == null)
                    Map.GenerateMap();
                Map.ShowMapUI();
                break;

            case GameState.GameOver:
                Debug.Log("GAME OVER");
                break;
        }
    }
}
