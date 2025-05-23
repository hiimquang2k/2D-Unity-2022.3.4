using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance { get; private set; }

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
            Instance = this;
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
        Debug.Log($"TransitionToScene called with scene: {sceneName}");
        if (instance == null)
        {
            Debug.LogError("SceneTransitionManager not found!");
            // Try to find it in the scene
            instance = FindObjectOfType<SceneTransitionManager>();
            if (instance == null)
            {
                Debug.LogError("No SceneTransitionManager found in the scene!");
                // Fall back to direct scene load
                SceneManager.LoadScene(sceneName);
                return;
            }
        }

        instance.StartCoroutine(instance.TransitionCoroutine(sceneName, spawnPosition));
    }

    private IEnumerator TransitionCoroutine(string sceneName, Vector3 spawnPosition)
    {
        if (isTransitioning) 
        {
            Debug.LogWarning("Already transitioning, ignoring new transition request.");
            yield break;
        }
        
        isTransitioning = true;
        Debug.Log($"Starting transition to scene: {sceneName}");

        try
        {
            // Skip saving game state if we're transitioning from menu
            if (GameManager.Instance != null && GameManager.Instance.currentPlayer != null)
            {
                Debug.Log("Saving game state...");
                GameManager.Instance.SaveGame(spawnPosition);
            }

            // Show transition UI if available
            if (transitionUI != null && transitionCanvasGroup != null)
            {
                transitionUI.SetActive(true);
                transitionCanvasGroup.alpha = 0f;
                yield return StartCoroutine(FadeTransition(1f));
            }
            else
            {
                Debug.LogWarning("Transition UI or CanvasGroup not assigned, skipping fade out");
                // Small delay to ensure everything is ready
                yield return new WaitForSeconds(0.1f);
            }


            // Load the new scene
            Debug.Log("Starting scene load...");
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
            asyncLoad.allowSceneActivation = true;

            // Wait for the scene to finish loading
            while (!asyncLoad.isDone)
            {
                Debug.Log($"Loading progress: {asyncLoad.progress * 100}%");
                yield return null;
            }
            Debug.Log("Scene load complete");

            // Small delay to ensure the new scene is fully initialized
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            // Fade in if we have a transition UI
            if (transitionUI != null && transitionCanvasGroup != null)
            {
                yield return StartCoroutine(FadeTransition(0f));
                transitionUI.SetActive(false);
            }
        }
        finally
        {
            isTransitioning = false;
            Debug.Log("Transition complete");
        }
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
