using UnityEngine;
using System.Collections;
using Cinemachine;

public class ImprovedCameraShake : MonoBehaviour
{
    [Header("Shake Settings")]
    [SerializeField] private float playerShakeIntensity = 0.15f;
    [SerializeField] private float monsterShakeIntensity = 0.4f;
    [SerializeField] private float shakeDuration = 0.3f;
    [SerializeField] private float decreaseFactor = 1.5f;

    private CinemachineVirtualCamera virtualCamera;
    private CinemachineBasicMultiChannelPerlin noiseComponent;
    private float currentShakeIntensity = 0f;
    private float currentShakeDuration = 0f;
    private bool isShaking = false;
    private Coroutine shakeCoroutine;

    private void Awake()
    {
        Debug.Log("CameraShake: Initializing...");
        
        // Find the Cinemachine Virtual Camera in the scene
        virtualCamera = FindObjectOfType<CinemachineVirtualCamera>();
        if (virtualCamera == null)
        {
            Debug.LogError("No CinemachineVirtualCamera found in the scene!");
            return;
        }
        else
        {
            Debug.Log("CameraShake: Found CinemachineVirtualCamera");
        }

        // Get the noise component
        noiseComponent = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        if (noiseComponent == null)
        {
            noiseComponent = virtualCamera.AddCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
            
            // Set up default noise profile if needed
            if (noiseComponent.m_NoiseProfile == null)
            {
                Debug.LogWarning("No noise profile assigned to Cinemachine. Camera shake may not work properly.");
            }
            else
            {
                Debug.Log("CameraShake: Using noise profile: " + noiseComponent.m_NoiseProfile.name);
            }
        }
        else
        {
            Debug.Log("CameraShake: Found existing noise component");
        }

        // Ensure the noise is turned off initially
        noiseComponent.m_AmplitudeGain = 0f;
        noiseComponent.m_FrequencyGain = 0f;

        // Reset shake state
        currentShakeIntensity = 0f;
        currentShakeDuration = 0f;
        isShaking = false;
    }

    private void Start()
    {
        Debug.Log("CameraShake: Starting...");
        
        // Reset noise values at start
        if (noiseComponent != null)
        {
            noiseComponent.m_AmplitudeGain = 0f;
            noiseComponent.m_FrequencyGain = 0f;
        }

        // Find all health systems in the scene and subscribe to their events
        HealthSystem[] healthSystems = FindObjectsOfType<HealthSystem>();
        foreach (HealthSystem health in healthSystems)
        {
            SubscribeToHealthSystem(health);
        }
    }

    public void TriggerCameraShake(GameObject entity)
    {
        Debug.Log("CameraShake: Triggering shake for entity: " + entity.name + ", Tag: " + entity.tag);
        
        // Determine shake intensity based on entity type
        float shakeIntensity = 0f;
        if (entity.CompareTag("Player"))
        {
            shakeIntensity = playerShakeIntensity;
        }
        else if (entity.CompareTag("Monster"))
        {
            shakeIntensity = monsterShakeIntensity;
        }
        else
        {
            // Default intensity for other entities
            shakeIntensity = playerShakeIntensity;
        }
        
        // Use Cinemachine's noise system if available
        if (noiseComponent != null)
        {
            Debug.Log("CameraShake: Starting shake with intensity: " + shakeIntensity);
            
            // Use the higher intensity if already shaking
            if (isShaking)
            {
                currentShakeIntensity = Mathf.Max(currentShakeIntensity, shakeIntensity);
            }
            else
            {
                currentShakeIntensity = shakeIntensity;
            }
            
            currentShakeDuration = shakeDuration;
            isShaking = true;
            
            // Start the shake coroutine if not already running
            if (shakeCoroutine != null)
            {
                StopCoroutine(shakeCoroutine);
            }
            shakeCoroutine = StartCoroutine(CinemachineShake());
        }
        else
        {
            Debug.LogError("CameraShake: Cinemachine components not found. Camera shake will not work.");
        }
    }

private IEnumerator CinemachineShake()
{
    // Store original camera position
    Vector3 originalPos = virtualCamera.transform.position;
    
    while (currentShakeDuration > 0f)
    {
        if (noiseComponent != null)
        {
            noiseComponent.m_AmplitudeGain = currentShakeIntensity;
            noiseComponent.m_FrequencyGain = currentShakeIntensity * 0.5f;
        }
        
        // Lock Z-position after each frame's shake calculation
        virtualCamera.transform.position = new Vector3(
            virtualCamera.transform.position.x,
            virtualCamera.transform.position.y,
            originalPos.z
        );
        
        currentShakeDuration -= Time.deltaTime;
        yield return null;
    }

    if (noiseComponent != null)
    {
        noiseComponent.m_AmplitudeGain = 0f;
        noiseComponent.m_FrequencyGain = 0f;
    }
    
    // Ensure final position is correct
    virtualCamera.transform.position = originalPos;
    isShaking = false;
    currentShakeIntensity = 0f;
}
    private void SubscribeToHealthSystem(HealthSystem healthSystem)
    {
        Debug.Log("CameraShake: Subscribing to health system: " + healthSystem.gameObject.name);
        
        if (healthSystem != null)
        {
            // Monitor health changes to detect damage
            healthSystem.OnHealthChanged += (current, previous) => 
            {
                Debug.Log("CameraShake: Health changed from " + previous + " to " + current);
                
                // Check if damage was taken
                if (current < previous)
                {
                    Debug.Log("CameraShake: Damage detected");
                    OnEntityDamaged(healthSystem);
                }
            };
        }
    }

    private void OnEntityDamaged(HealthSystem healthSystem)
    {
        Debug.Log("CameraShake: Entity damaged: " + healthSystem.gameObject.name + ", Tag: " + healthSystem.gameObject.tag);
        
        if (healthSystem.gameObject.CompareTag("Player"))
        {
            Debug.Log("CameraShake: Player damage detected");
            ShakeCamera(playerShakeIntensity);
        }
        else if (healthSystem.gameObject.CompareTag("Monster"))
        {
            Debug.Log("CameraShake: Monster damage detected");
            ShakeCamera(monsterShakeIntensity);
        }
    }

    public void ShakeCamera(float intensity)
    {
        Debug.Log("CameraShake: ShakeCamera called with intensity: " + intensity);
        
        if (noiseComponent != null)
        {
            currentShakeIntensity = intensity;
            currentShakeDuration = shakeDuration;
            isShaking = true;
            shakeCoroutine = StartCoroutine(CinemachineShake());
        }
    }
}