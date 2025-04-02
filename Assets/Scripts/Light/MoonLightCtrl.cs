using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class MoonLightController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Light2D moonLight;
    
    [Header("Position Settings")]
    [SerializeField] private float heightAbovePlayer = 10f;
    [SerializeField] private float initialXOffset = 5f;
    [SerializeField] private float movementScale = 0.3f; // How much the moon moves relative to player (smaller = slower)
    [SerializeField] private float smooth = 2f; // Smoothing factor for movement
    
    [Header("Light Settings")]
    [SerializeField] private Color moonColor = new Color(0.7f, 0.8f, 1f, 1f);
    [SerializeField] private float moonRadius = 15f;
    [SerializeField] private float moonIntensity = 0.8f;
    [SerializeField] private bool castShadows = true;
    
    // Internal variables
    private float targetXPosition;
    private float initialPlayerX;
    private Vector3 currentVelocity = Vector3.zero;

    private void Start()
    {
        // Find player if not assigned
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
            else
            {
                Debug.LogError("Player not found. Please assign player transform or use 'Player' tag.");
                enabled = false;
                return;
            }
        }
        
        // Create moon light if not assigned
        if (moonLight == null)
        {
            moonLight = GetComponent<Light2D>();
            if (moonLight == null)
            {
                moonLight = gameObject.AddComponent<Light2D>();
            }
        }
        
        // Configure moon light
        SetupMoonLight();
        
        // Initialize position
        initialPlayerX = playerTransform.position.x;
        transform.position = new Vector3(
            playerTransform.position.x + initialXOffset,
            playerTransform.position.y + heightAbovePlayer,
            transform.position.z
        );
    }
    
    private void SetupMoonLight()
    {
        moonLight.lightType = Light2D.LightType.Point;
        moonLight.color = moonColor;
        moonLight.intensity = moonIntensity;
        moonLight.pointLightOuterRadius = moonRadius;
        moonLight.pointLightInnerRadius = moonRadius * 0.2f;
        moonLight.shadowsEnabled = castShadows;
        
        // Set shadow parameters if enabled
        if (castShadows)
        {
            moonLight.shadowIntensity = 0.7f;
            moonLight.shadowVolumeIntensity = 0.5f;
        }
    }
    
    private void Update()
    {
        if (playerTransform == null) return;
        
        // Calculate how far player has moved from initial position
        float playerMovement = playerTransform.position.x - initialPlayerX;
        
        // Calculate target X position with slower movement
        targetXPosition = initialPlayerX + initialXOffset + (playerMovement * movementScale);
        
        // Smoothly move the moon
        Vector3 targetPosition = new Vector3(
            targetXPosition,
            playerTransform.position.y + heightAbovePlayer,
            transform.position.z
        );
        
        // Apply smooth movement
        transform.position = Vector3.SmoothDamp(
            transform.position, 
            targetPosition, 
            ref currentVelocity, 
            smooth * Time.deltaTime
        );
    }
    
    // Optional: Add a subtle pulsing effect to the moon
    public void EnableMoonPulsing(float pulseAmount = 0.1f, float pulseSpeed = 0.5f)
    {
        StartCoroutine(PulseMoonLight(pulseAmount, pulseSpeed));
    }
    
    private IEnumerator PulseMoonLight(float pulseAmount, float pulseSpeed)
    {
        float baseIntensity = moonIntensity;
        float time = 0;
        
        while (true)
        {
            time += Time.deltaTime * pulseSpeed;
            float pulseFactor = Mathf.Sin(time) * pulseAmount;
            moonLight.intensity = baseIntensity + pulseFactor;
            yield return null;
        }
    }
    
    // Public method to adjust moon properties at runtime
    public void AdjustMoonLight(Color? color = null, float? intensity = null, float? radius = null)
    {
        if (color.HasValue) moonLight.color = color.Value;
        if (intensity.HasValue) moonLight.intensity = intensity.Value;
        if (radius.HasValue)
        {
            moonLight.pointLightOuterRadius = radius.Value;
            moonLight.pointLightInnerRadius = radius.Value * 0.2f;
        }
    }
    
    // Public method to create moon flare effect
    public void CreateMoonFlare(float duration = 2f, float intensityMultiplier = 1.5f)
    {
        StartCoroutine(MoonFlareEffect(duration, intensityMultiplier));
    }
    
    private IEnumerator MoonFlareEffect(float duration, float intensityMultiplier)
    {
        float originalIntensity = moonLight.intensity;
        float originalRadius = moonLight.pointLightOuterRadius;
        
        // Increase intensity and radius
        moonLight.intensity *= intensityMultiplier;
        moonLight.pointLightOuterRadius *= 1.2f;
        
        // Hold for a moment
        yield return new WaitForSeconds(duration * 0.2f);
        
        // Gradually return to normal
        float elapsed = 0;
        while (elapsed < duration * 0.8f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (duration * 0.8f);
            
            moonLight.intensity = Mathf.Lerp(originalIntensity * intensityMultiplier, originalIntensity, t);
            moonLight.pointLightOuterRadius = Mathf.Lerp(originalRadius * 1.2f, originalRadius, t);
            
            yield return null;
        }
        
        // Ensure we're back to original values
        moonLight.intensity = originalIntensity;
        moonLight.pointLightOuterRadius = originalRadius;
    }
}