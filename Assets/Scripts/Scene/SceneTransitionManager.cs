using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.Events;
using System;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance { get; private set; }

    [Header("Transition Settings")]
    [SerializeField] private GameObject transitionUI;
    [SerializeField] private float transitionDuration = 0.5f;
    [SerializeField] private float minLoadTime = 1f; // Minimum loading time to prevent flashing
    [SerializeField] private string[] scenesToPreload; // Scenes to preload in the background
    
    public static event Action<float> OnLoadProgress; // Progress callback (0-1)
    public static event Action OnTransitionStart;
    public static event Action OnTransitionComplete;
    
    private static SceneTransitionManager instance;
    private CanvasGroup transitionCanvasGroup;
    private bool isTransitioning = false;
    private AsyncOperation loadOperation;
    private float loadStartTime;
    private bool isPreloading = false;

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

    public static void TransitionToScene(string sceneName, Vector3 spawnPosition, bool forceReload = false)
    {
        if (instance == null)
        {
            instance = FindObjectOfType<SceneTransitionManager>();
            if (instance == null)
            {
                Debug.LogError($"No {nameof(SceneTransitionManager)} found in the scene!");
                SceneManager.LoadScene(sceneName);
                return;
            }
        }

        if (instance.isTransitioning)
        {
            Debug.LogWarning("Scene transition already in progress");
            return;
        }

        instance.StartCoroutine(instance.TransitionCoroutine(sceneName, spawnPosition, forceReload));
    }

    private void Start()
    {
        if (scenesToPreload != null && scenesToPreload.Length > 0)
        {
            StartCoroutine(PreloadScenes());
        }
    }

    private IEnumerator PreloadScenes()
    {
        isPreloading = true;
        foreach (var sceneName in scenesToPreload)
        {
            if (!SceneManager.GetSceneByName(sceneName).isLoaded)
            {
                var loadOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
                loadOp.allowSceneActivation = false;
                
                while (loadOp.progress < 0.9f)
                {
                    yield return null;
                }
                
                // Keep the scene loaded but inactive
                Debug.Log($"Preloaded scene: {sceneName}");
            }
        }
        isPreloading = false;
    }

    private IEnumerator TransitionCoroutine(string sceneName, Vector3 spawnPosition, bool forceReload = false)
    {
        isTransitioning = true;
        loadStartTime = Time.realtimeSinceStartup;
        OnTransitionStart?.Invoke();

        try
        {
            // 1. Fade out
            if (transitionUI != null && transitionCanvasGroup != null)
            {
                transitionUI.SetActive(true);
                transitionCanvasGroup.alpha = 0f;
                yield return StartCoroutine(FadeTransition(1f));
            }

            // 2. Save game state if needed
            if (GameManager.Instance != null && GameManager.Instance.currentPlayer != null)
            {
                GameManager.Instance.SaveGame(spawnPosition);
            }

            // 3. Unload unused assets to free up memory
            yield return Resources.UnloadUnusedAssets();
            GC.Collect();

            // 4. Start loading the new scene
            loadOperation = SceneManager.LoadSceneAsync(sceneName);
            loadOperation.allowSceneActivation = false;

            // 5. Show loading progress
            while (loadOperation.progress < 0.9f)
            {
                float progress = Mathf.Clamp01(loadOperation.progress / 0.9f);
                OnLoadProgress?.Invoke(progress);
                yield return null;
            }

            // 6. Ensure minimum load time
            float elapsedTime = Time.realtimeSinceStartup - loadStartTime;
            if (elapsedTime < minLoadTime)
            {
                float remainingTime = minLoadTime - elapsedTime;
                float timer = 0f;
                while (timer < remainingTime)
                {
                    timer += Time.unscaledDeltaTime;
                    float progress = Mathf.Clamp01((elapsedTime + timer) / minLoadTime);
                    OnLoadProgress?.Invoke(progress);
                    yield return null;
                }
            }


            // 7. Activate the new scene
            loadOperation.allowSceneActivation = true;

            // Wait for scene to fully load
            while (!loadOperation.isDone)
            {
                yield return null;
            }

            // 8. Wait one more frame to ensure everything is initialized
            yield return null;

            // 9. Fade in
            if (transitionUI != null && transitionCanvasGroup != null)
            {
                yield return StartCoroutine(FadeTransition(0f));
                transitionUI.SetActive(false);
            }
        }
        finally
        {
            isTransitioning = false;
            loadOperation = null;
            OnTransitionComplete?.Invoke();
        }
    }

    private IEnumerator FadeTransition(float targetAlpha)
    {
        float startAlpha = transitionCanvasGroup.alpha;
        float elapsedTime = 0f;
        
        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsedTime / transitionDuration);
            transitionCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            yield return null;
        }
        
        transitionCanvasGroup.alpha = targetAlpha;
    }
    
    // Call this to preload a specific scene
    public void PreloadScene(string sceneName)
    {
        if (!isPreloading)
        {
            StartCoroutine(PreloadSingleScene(sceneName));
        }
    }
    
    private IEnumerator PreloadSingleScene(string sceneName)
    {
        if (!SceneManager.GetSceneByName(sceneName).isLoaded)
        {
            var loadOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            loadOp.allowSceneActivation = false;
            
            while (loadOp.progress < 0.9f)
            {
                yield return null;
            }
            
            Debug.Log($"Preloaded scene: {sceneName}");
        }
    }
}
