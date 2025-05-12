using UnityEngine;
 // For 2D lights

public class Bonfire : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private KeyCode interactKey = KeyCode.S;

    [Header("References")]
    [SerializeField] private PlayerData playerData;
    [SerializeField] private MobSpawner mobSpawner;

    private bool playerInRange;
    [Header("Light Settings")]
    [SerializeField] private UnityEngine.Rendering.Universal.Light2D fireLight;
    [SerializeField] private float minIntensity = 0.8f;
    [SerializeField] private float maxIntensity = 1.2f;
    [SerializeField] private float flickerSpeed = 5f;
    [SerializeField] private float lightPulseAmount = 0.2f;

    private float baseIntensity;
    private float baseRadius;

    void Start()
    {
        InitializeLightComponents();
    }

    private void InitializeLightComponents()
    {
        if (fireLight == null)
        {
            // Create light child object if not set up in inspector
            GameObject lightObj = new GameObject("FireLight");
            lightObj.transform.SetParent(transform);
            lightObj.transform.localPosition = Vector3.zero;
            
            fireLight = lightObj.AddComponent<UnityEngine.Rendering.Universal.Light2D>();
            fireLight.lightType = UnityEngine.Rendering.Universal.Light2D.LightType.Point;
            fireLight.color = new Color(1f, 0.5f, 0.3f); // Orange-red
            fireLight.intensity = 1f;
            fireLight.pointLightOuterRadius = 5f;
            fireLight.pointLightInnerRadius = 1f;
            fireLight.pointLightInnerAngle = 360f;
            fireLight.pointLightOuterAngle = 360f;
        }

        baseIntensity = fireLight.intensity;
        baseRadius = fireLight.pointLightOuterRadius;
        GetComponent<CircleCollider2D>().radius = baseRadius;
    }

    private void AnimateFireLight()
    {
        // Flicker intensity
        float intensityVariation = Mathf.PerlinNoise(Time.time * flickerSpeed, 0);
        fireLight.intensity = baseIntensity + intensityVariation * lightPulseAmount;

        // Pulse radius
        float radiusVariation = Mathf.Sin(Time.time * flickerSpeed) * 0.1f;
        fireLight.pointLightOuterRadius = baseRadius + radiusVariation;
    }


    void Update()
    {
        if (fireLight != null)
        {
            AnimateFireLight();
        }
        if (playerInRange && Input.GetKeyDown(interactKey))
        {
            SaveAtBonfire();
        }
    }

    private void SaveAtBonfire()
    {
        // Get player reference
        var player = FindObjectOfType<PlayerMovement>();
        var health = player.GetComponent<HealthSystem>();

        // Full heal before saving
        health.SetCurrentHealth(health.GetMaxHealth());

        // Save game at bonfire position
        GameManager.Instance.SaveGame(transform.position);
        mobSpawner.ToggleSpawning(false);

        // Visual feedback
        GetComponent<Animator>().SetTrigger("Activate");
        Debug.Log("Game saved at bonfire!");
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            mobSpawner.ToggleSpawning(false);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            mobSpawner.ToggleSpawning(true);
        }
    }
}