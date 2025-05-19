using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneTransitionManager : MonoBehaviour
{
    [Header("Transition Settings")]
    [SerializeField] private GameObject transitionUI;
    [SerializeField] private float transitionDuration = 1f;
    
    private static SceneTransitionManager instance;
    private CanvasGroup transitionCanvasGroup;
    private bool isTransitioning = false;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (transitionUI != null)
        {
            transitionCanvasGroup = transitionUI.GetComponent<CanvasGroup>();
            if (transitionCanvasGroup == null)
            {
                Debug.LogError("Transition UI must have a CanvasGroup component!");
            }
        }
    }

    public static void TransitionToScene(string sceneName, Vector3 spawnPosition)
    {
        if (instance == null)
        {
            Debug.LogError("SceneTransitionManager not found!");
            return;
        }

        instance.StartCoroutine(instance.TransitionCoroutine(sceneName, spawnPosition));
    }

    private IEnumerator TransitionCoroutine(string sceneName, Vector3 spawnPosition)
    {
        if (isTransitioning) yield break;
        isTransitioning = true;

        // Skip saving game state if we're transitioning from menu
        if (GameManager.Instance != null && GameManager.Instance.currentPlayer != null)
        {
            GameManager.Instance.SaveGame(spawnPosition);
        }

        // 2. Show transition UI
        if (transitionUI != null)
        {
            transitionUI.SetActive(true);
            transitionCanvasGroup.alpha = 0f;
        }

        // 3. Fade out
        yield return StartCoroutine(FadeTransition(1f));

        // 4. Load new scene
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        // 5. Wait until scene is ready
        while (!asyncLoad.isDone)
        {
            if (asyncLoad.progress >= 0.9f)
            {
                asyncLoad.allowSceneActivation = true;
            }
            yield return null;
        }

        // 6. Fade in
        yield return StartCoroutine(FadeTransition(0f));

        // 7. Hide transition UI
        if (transitionUI != null)
        {
            transitionUI.SetActive(false);
        }

        isTransitioning = false;
    }

    private IEnumerator FadeTransition(float targetAlpha)
    {
        float elapsedTime = 0f;
        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / transitionDuration;
            transitionCanvasGroup.alpha = Mathf.Lerp(transitionCanvasGroup.alpha, targetAlpha, t);
            yield return null;
        }
        transitionCanvasGroup.alpha = targetAlpha;
    }
}
