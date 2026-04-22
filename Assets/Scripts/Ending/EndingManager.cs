using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem; // [필수] New Input System 사용

public class EndingManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform pageContainer;

    private EndingData currentEndingData;
    private GameObject currentPageObj;
    private EndingPage currentPageScript;

    private int pageIndex = 0;

    private void Start()
    {
        LoadEndingData();

        if (currentEndingData == null)
        {
            Debug.LogError("엔딩 데이터 없음.");
            return;
        }

        if (GameManager.Input != null)
            GameManager.Input.OnEndingNextCutTriggered += OnScreenClick;

        pageIndex = 0;
        LoadPage(pageIndex);

        // 첫 컷 바로 보여주기
        OnScreenClick();
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null && GameManager.Input != null)
            GameManager.Input.OnEndingNextCutTriggered -= OnScreenClick;
    }


    private void LoadEndingData()
    {
        if (GameManager.Instance == null || GameManager.Instance.SelectedPlayerClass == null)
        {
            return;
        }

        string charName = GameManager.Instance.SelectedPlayerClass.name;
        string path = $"Endings/{charName}_Ending";
        currentEndingData = Resources.Load<EndingData>(path);
        if (currentEndingData == null)
        {
            Debug.LogWarning($"엔딩 데이터 없음");
        }
    }

    private void LoadPage(int index)
    {
        if (index >= currentEndingData.pagePrefabs.Count)
        {
            currentPageScript = null;
            FinishEnding();
            return;
        }

        // 이전 페이지 제거
        if (currentPageObj != null)
            Destroy(currentPageObj);

        GameObject prefab = currentEndingData.pagePrefabs[index];
        currentPageObj = Instantiate(prefab, pageContainer);

        RectTransform rt = currentPageObj.GetComponent<RectTransform>();
        rt.anchoredPosition = Vector2.zero;
        rt.localScale = Vector3.one;

        currentPageScript = currentPageObj.GetComponent<EndingPage>();
        currentPageScript.Init();
    }

    private void OnScreenClick()
    {
        if (Time.timeScale == 0 || currentPageScript == null) return;

        while (!currentPageScript.ShowNextCut())
        {
            pageIndex++;
            LoadPage(pageIndex);

            if (currentPageScript == null) return; // FinishEnding() 호출됨
        }
    }

    private void FinishEnding()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetGame();
            GameManager.Scene.LoadScene(SceneType.MainMenu);
        }
    }
}