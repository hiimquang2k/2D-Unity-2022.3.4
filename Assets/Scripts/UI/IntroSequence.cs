using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class IntroSequence : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject introPanel;
    [SerializeField] private TextMeshProUGUI introText;
    [SerializeField] private Image fadePanel;
    
    [Header("Intro Settings")]
    [TextArea(3, 5)]
    [SerializeField] private string[] introLines = 
    {
        "In a world where darkness spreads...",
        "A hero rises to face the unknown...",
        "Your journey begins now..."
    };
    
    [SerializeField] private float timePerLine = 3f;
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private string nextScene = "GameScene";
    
    private void Start()
    {
        if (introPanel != null)
        {
            introPanel.SetActive(false);
        }
        
        if (fadePanel != null)
        {
            fadePanel.gameObject.SetActive(true);
            fadePanel.color = Color.black;
            StartCoroutine(StartIntroDelayed());
        }
    }
    
    private IEnumerator StartIntroDelayed()
    {
        // Small delay to ensure everything is initialized
        yield return new WaitForEndOfFrame();
        StartIntro();
    }
    
    public void StartIntro()
    {
        StartCoroutine(PlayIntroSequence());
    }
    
    private IEnumerator PlayIntroSequence()
    {
        Debug.Log("Starting PlayIntroSequence");
        if (introPanel == null || introText == null)
        {
            Debug.LogError("Missing references! introPanel: " + (introPanel != null) + ", introText: " + (introText != null));
            yield break;
        }
        
        introPanel.SetActive(true);
        
        // Fade in the intro panel
        Debug.Log("Fading in intro panel");
        Image panelImage = introPanel.GetComponent<Image>();
        if (panelImage != null)
        {
            Color startColor = panelImage.color;
            startColor.a = 0f;
            panelImage.color = startColor;
            
            float timer = 0f;
            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                startColor.a = Mathf.Clamp01(timer / fadeDuration);
                panelImage.color = startColor;
                yield return null;
            }
        }
        
        // Display each line of text
        Debug.Log("Starting to display intro lines");
        foreach (string line in introLines)
        {
            introText.text = "";
            foreach (char letter in line)
            {
                introText.text += letter;
                yield return new WaitForSeconds(0.02f); // Typing effect
            }
            yield return new WaitForSeconds(timePerLine);
        }
        
        // Fade out
        Debug.Log("Fading out and loading next scene: " + nextScene);
        if (fadePanel != null)
        {
            yield return FadeOut();
        }
        
        // Ensure GameManager is ready
        if (GameManager.Instance != null)
        {
            // Reset save state for new game
            GameManager.Instance.playerData.ResetData();
            GameManager.Instance.playerData.saveState.savedScene = nextScene;
            GameManager.Instance.playerData.saveState.savedPosition = Vector3.zero;
        }
        
        // Load the next scene using the SceneTransitionManager
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.TransitionToScene(nextScene, Vector3.zero);
        }
        else
        {
            Debug.LogError("SceneTransitionManager not found! Falling back to direct scene load.");
            SceneManager.LoadScene(nextScene);
        }
    }
    
    private IEnumerator FadeIn()
    {
        float timer = 0f;
        Color color = fadePanel.color;
        
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            color.a = 1f - (timer / fadeDuration);
            fadePanel.color = color;
            yield return null;
        }
        
        fadePanel.gameObject.SetActive(false);
    }
    
    private IEnumerator FadeOut()
    {
        fadePanel.gameObject.SetActive(true);
        float timer = 0f;
        Color color = fadePanel.color;
        
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            color.a = timer / fadeDuration;
            fadePanel.color = color;
            yield return null;
        }
        
        color.a = 1f;
        fadePanel.color = color;
    }
}
