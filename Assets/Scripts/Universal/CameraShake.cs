using UnityEngine;
using System.Collections;
using Cinemachine;

public class ImprovedCameraShake : MonoBehaviour
{
    [Header("Shake Settings")]
    [SerializeField] private float playerShakeIntensity = 0.15f;
    [SerializeField] private float monsterShakeIntensity = 0.3f;
    [SerializeField] private float shakeDuration = 0.3f;
    [SerializeField] private float decreaseFactor = 1.5f;
    [SerializeField] private NoiseSettings noiseProfile2D; // Assign a 2D-specific noise profile

    private CinemachineVirtualCamera virtualCamera;
    private CinemachineBasicMultiChannelPerlin noiseComponent;
    private float currentShakeIntensity = 0f;
    private float currentShakeDuration = 0f;
    private bool isShaking = false;
    private Coroutine shakeCoroutine;
    private Vector3 originalCameraPosition;

    private void Awake()
    {
        virtualCamera = FindObjectOfType<CinemachineVirtualCamera>();
        if (virtualCamera == null)
        {
            Debug.LogError("No CinemachineVirtualCamera found in the scene!");
            return;
        }

        // Force 2D camera settings
        virtualCamera.m_Lens.Orthographic = true;
        virtualCamera.m_Lens.NearClipPlane = -100;
        virtualCamera.m_Lens.FarClipPlane = 100;

        // Get or add noise component
        noiseComponent = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        if (noiseComponent == null)
        {
            noiseComponent = virtualCamera.AddCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        }

        // Apply 2D noise profile
        if (noiseProfile2D != null)
        {
            noiseComponent.m_NoiseProfile = noiseProfile2D;
        }
        else
        {
            Debug.LogWarning("No 2D noise profile assigned. Using default (may cause Z-axis issues).");
        }

        // Store original position
        originalCameraPosition = virtualCamera.transform.position;

        // Reset shake state
        ResetNoise();
    }

    private void Start()
    {
        // Reset noise values at start
        ResetNoise();

        // Find all health systems in the scene and subscribe to their events
        HealthSystem[] healthSystems = FindObjectsOfType<HealthSystem>();
        foreach (HealthSystem health in healthSystems)
        {
            SubscribeToHealthSystem(health);
        }
    }

    private void ResetNoise()
    {
        if (noiseComponent != null)
        {
            noiseComponent.m_AmplitudeGain = 0f;
            noiseComponent.m_FrequencyGain = 0f;
        }
        virtualCamera.transform.position = originalCameraPosition;
    }

    public void TriggerCameraShake(GameObject entity)
    {
        float shakeIntensity = entity.CompareTag("Player") ? playerShakeIntensity :
                             entity.CompareTag("Monster") ? monsterShakeIntensity :
                             playerShakeIntensity;

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

        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
        }
        shakeCoroutine = StartCoroutine(Perform2DShake());
    }

    private IEnumerator Perform2DShake()
    {
        while (currentShakeDuration > 0f)
        {
            if (noiseComponent != null)
            {
                // Apply shake only to X/Y axes
                noiseComponent.m_AmplitudeGain = currentShakeIntensity;
                noiseComponent.m_FrequencyGain = currentShakeIntensity * 0.5f;
            }

            // Lock Z position every frame to prevent depth issues
            Vector3 currentPos = virtualCamera.transform.position;
            virtualCamera.transform.position = new Vector3(currentPos.x, currentPos.y, originalCameraPosition.z);

            currentShakeDuration -= Time.deltaTime * decreaseFactor;
            yield return null;
        }

        ResetNoise();
        isShaking = false;
        currentShakeIntensity = 0f;
    }

    private void SubscribeToHealthSystem(HealthSystem healthSystem)
    {
        if (healthSystem != null)
        {
            healthSystem.OnHealthChanged += (current, previous) =>
            {
                if (current < previous)
                {
                    OnEntityDamaged(healthSystem);
                }
            };
        }
    }

    private void OnEntityDamaged(HealthSystem healthSystem)
    {
        if (healthSystem.gameObject.CompareTag("Player"))
        {
            ShakeCamera(playerShakeIntensity);
        }
        else if (healthSystem.gameObject.CompareTag("Monster"))
        {
            ShakeCamera(monsterShakeIntensity);
        }
    }

    public void ShakeCamera(float intensity)
    {
        currentShakeIntensity = intensity;
        currentShakeDuration = shakeDuration;
        isShaking = true;

        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
        }
        shakeCoroutine = StartCoroutine(Perform2DShake());
    }
}