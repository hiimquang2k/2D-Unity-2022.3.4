using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Events;
using UnityEngine.UI;

public class Bonfire : MonoBehaviour, IInteractable
{
    [Header("Bonfire Settings")]
    [SerializeField] private float interactionRange = 2f;
    [SerializeField] private Light2D bonfireLight;
    [SerializeField] private ParticleSystem fireParticles;
    [SerializeField] private AudioClip litSound;
    [SerializeField] private AudioClip saveSound;
    
    [Header("UI")]
    [SerializeField] private GameObject savePromptUI;
    [SerializeField] private float promptDisplayTime = 2f;
    
    [Header("Light Settings")]
    [SerializeField] private float minIntensity = 0.5f;
    [SerializeField] private float maxIntensity = 1f;
    [SerializeField] private float flickerSpeed = 1f;
    
    private bool isLit = false;
    private AudioSource audioSource;
    private float timeOffset;
    private float savePromptTimer = 0f;
    private bool showingSavePrompt = false;
    private bool playerInRange = false;
    
    // Event for when the bonfire is lit
    public UnityEvent onBonfireLit;
    
    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Initialize UI
        if (savePromptUI != null)
        {
            savePromptUI.SetActive(false);
        }
        
        // Randomize the flicker offset for variety
        timeOffset = Random.Range(0f, 100f);
        
        // Initialize light state
        if (bonfireLight != null)
        {
            bonfireLight.intensity = 0f;
            bonfireLight.enabled = false;
        }
        
        if (fireParticles != null)
        {
            var emission = fireParticles.emission;
            emission.enabled = false;
        }
    }
    
    private void Update()
    {
        if (isLit && bonfireLight != null)
        {
            // Create a flickering effect for the bonfire light
            float noise = Mathf.PerlinNoise(Time.time * flickerSpeed + timeOffset, 0f);
            bonfireLight.intensity = Mathf.Lerp(minIntensity, maxIntensity, noise);
        }
        
        // Handle save prompt timer
        if (showingSavePrompt)
        {
            savePromptTimer -= Time.deltaTime;
            if (savePromptTimer <= 0f)
            {
                HideSavePrompt();
            }
        }
        
        // Check for interaction input
        if (playerInRange && Input.GetKeyDown(KeyCode.S))
        {
            Interact(GameObject.FindGameObjectWithTag("Player"));
        }
    }
    
    public void Interact(GameObject interactor)
    {
        if (!isLit)
        {
            LightBonfire();
        }
        
        // Always save when interacting with a lit bonfire
        SaveGame(interactor.transform.position);
    }
    
    private void LightBonfire()
    {
        isLit = true;
        
        // Enable visual effects
        if (bonfireLight != null)
        {
            bonfireLight.enabled = true;
            bonfireLight.intensity = minIntensity;
        }
        
        if (fireParticles != null)
        {
            var emission = fireParticles.emission;
            emission.enabled = true;
            fireParticles.Play();
        }
        
        // Play sound
        if (litSound != null)
        {
            audioSource.PlayOneShot(litSound);
        }
        
        // Invoke the event
        onBonfireLit?.Invoke();
        
        Debug.Log("Bonfire has been lit!");
    }
    
    private void SaveGame(Vector3 playerPosition)
    {
        // Find the GameManager and call its SaveGame method
        var gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            // Heal the player to full health before saving
            var health = FindObjectOfType<HealthSystem>();
            if (health != null)
            {
                health.SetCurrentHealth(health.GetMaxHealth());
            }
            
            gameManager.SaveGame(playerPosition);
            
            // Play save sound if available
            if (saveSound != null)
            {
                audioSource.PlayOneShot(saveSound);
            }
            
            // Show save confirmation
            ShowSavePrompt();
            Debug.Log("Game saved at bonfire!");
        }
        else
        {
            Debug.LogWarning("GameManager not found in the scene!");
        }
    }
    
    private void ShowSavePrompt()
    {
        if (savePromptUI != null)
        {
            savePromptUI.SetActive(true);
            savePromptTimer = promptDisplayTime;
            showingSavePrompt = true;
        }
    }
    
    private void HideSavePrompt()
    {
        if (savePromptUI != null)
        {
            savePromptUI.SetActive(false);
            showingSavePrompt = false;
        }
    }
    
    public bool IsInRange(Vector3 playerPosition, out int priority)
    {
        float distance = Vector3.Distance(transform.position, playerPosition);
        bool inRange = distance <= interactionRange;
        
        if (inRange && !playerInRange)
        {
            playerInRange = true;
            // Show interaction prompt
        }
        else if (!inRange && playerInRange)
        {
            playerInRange = false;
            // Hide interaction prompt
        }
        
        priority = 0; // Set priority (lower number = higher priority)
        return inRange;
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }
}