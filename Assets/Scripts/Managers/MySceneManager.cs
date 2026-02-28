using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum SceneType
{
    Unknown,
    MainMenu,
    Game,
    Ending
}

public class MySceneManager
{
    private const int FADE_SORT_ORDER = 32767; // 최상단 노출 보장

    public void LoadScene(SceneType type)
    {
        string sceneName = GetSceneName(type);
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError($"[MySceneManager] Unknown Scene Type: {type}");
            return;
        }

        // GameManager를 통해 코루틴 실행
        GameManager.Instance.StartCoroutine(LoadSceneAsyncRoutine(sceneName));
    }

    private IEnumerator LoadSceneAsyncRoutine(string sceneName)
    {
        CanvasGroup fadeGroup = GetOrCreateFadeUI();
        fadeGroup.blocksRaycasts = true;
        //fadeGroup.gameObject.SetActive(true);

        float fadeDuration = 0.5f;
        float timer = 0f;

        //페이드 아웃
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            fadeGroup.alpha = Mathf.Lerp(0f, 1f, timer / fadeDuration);
            yield return null;
        }
        fadeGroup.alpha = 1f;

        // 비동기 씬 로드
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = false; // 로딩 완료 후 바로 넘어가지 않게 대기

        while (!op.isDone)
        {
            if (op.progress >= 0.9f)
            {
                op.allowSceneActivation = true;
            }
            yield return null;
        }

        //오류 방지용 씬 전환 후 0.1초 대기
        yield return new WaitForSeconds(0.1f);

        // 페이드 아웃
        timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            fadeGroup.alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);
            yield return null;
        }
        
        fadeGroup.alpha = 0f;
        fadeGroup.blocksRaycasts = false;
        //fadeGroup.gameObject.SetActive(false);
    }

    private CanvasGroup GetOrCreateFadeUI()
    {
        GameObject fadeObj = GameObject.Find("SceneFadeCanvas");
        if (fadeObj == null)
        {
            // 캔버스 생성
            fadeObj = new GameObject("SceneFadeCanvas");
            Canvas canvas = fadeObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = FADE_SORT_ORDER;
            
            fadeObj.AddComponent<CanvasScaler>();
            fadeObj.AddComponent<GraphicRaycaster>();
            
            Object.DontDestroyOnLoad(fadeObj);

            // 이미지(검은 배경) 생성
            GameObject imageObj = new GameObject("FadeImage");
            imageObj.transform.SetParent(fadeObj.transform, false);
            
            Image image = imageObj.AddComponent<Image>();
            image.color = Color.black;
            image.raycastTarget = true; // 클릭 막기용
            
            // 전체 화면 채우기
            RectTransform rect = image.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero; // offset을 0으로 만들어 꽉 채움
            
            // 알파값 조절을 위한 CanvasGroup
            CanvasGroup group = imageObj.AddComponent<CanvasGroup>();
            group.alpha = 0f;
            
            return group;
        }

        return fadeObj.GetComponentInChildren<CanvasGroup>();
    }

    private string GetSceneName(SceneType type)
    {
        switch (type)
        {
            case SceneType.MainMenu:
                return "MainScene";
            case SceneType.Game:
                return "GameScene";
            case SceneType.Ending:
                return "EndingScene";
            default:
                return "";
        }
    }
}