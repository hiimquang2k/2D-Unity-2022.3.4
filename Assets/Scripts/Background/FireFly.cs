using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Firefly : MonoBehaviour
{
    [Header("Appearance")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Light2D fireflyLight;
    [SerializeField] private Color baseColor = new Color(0.9f, 1f, 0.5f);
    [SerializeField] private float spriteSize = 0.15f;
    [SerializeField] private float lightRadius = 1.2f;
    [SerializeField] private float maxLightIntensity = 1.2f;
    
    [Header("Behavior")]
    [SerializeField] private float moveSpeed = 1.0f;
    [SerializeField] private float wanderRadius = 5.0f;
    [SerializeField] private float changeDirectionInterval = 2.0f;
    [SerializeField] private float pulseSpeed = 2.0f;
    [SerializeField] private float pulseIntensity = 0.5f;
    
    // Movement
    private Vector3 targetPosition;
    private Vector3 startPosition;
    private float directionTimer;
    
    // Pulse effect
    private float pulseTimer;

    private void OnEnable()
    {
        // Make sure we have the required components
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            
            // If still null, create one
            if (spriteRenderer == null)
            {
                spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = CreateCircleSprite();
                spriteRenderer.color = baseColor;
                spriteRenderer.sortingLayerName = "Default";
                spriteRenderer.sortingOrder = 5;
            }
        }
        
        if (fireflyLight == null)
        {
            fireflyLight = GetComponent<Light2D>();
            
            // If still null, create one
            if (fireflyLight == null)
            {
                fireflyLight = gameObject.AddComponent<Light2D>();
                fireflyLight.lightType = Light2D.LightType.Point;
                fireflyLight.color = baseColor;
                fireflyLight.intensity = maxLightIntensity;
                fireflyLight.pointLightOuterRadius = lightRadius;
                fireflyLight.pointLightInnerRadius = lightRadius * 0.2f;
            }
        }
        
        // Set initial properties
        spriteRenderer.transform.localScale = Vector3.one * spriteSize;
        fireflyLight.color = baseColor;
        
        // Set initial position
        startPosition = transform.position;
        SetNewTargetPosition();
        
        // Initialize timers
        directionTimer = changeDirectionInterval;
        pulseTimer = Random.Range(0f, Mathf.PI * 2); // Randomize pulse start point
    }
    
    private void Update()
    {
        // Handle movement
        UpdateMovement();
        
        // Handle light pulsing
        UpdateLightPulse();
    }
    
    private void UpdateMovement()
    {
        // Update direction timer
        directionTimer -= Time.deltaTime;
        if (directionTimer <= 0)
        {
            SetNewTargetPosition();
            directionTimer = changeDirectionInterval + Random.Range(-0.5f, 0.5f); // Add variance
        }
        
        // Move toward target
        transform.position = Vector3.MoveTowards(
            transform.position, 
            targetPosition, 
            moveSpeed * Time.deltaTime
        );
        
        // If we reached the target, get a new one
        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            SetNewTargetPosition();
        }
    }
    
    private void UpdateLightPulse()
    {
        // Increment pulse timer
        pulseTimer += Time.deltaTime * pulseSpeed;
        
        // Calculate pulse value (0 to 1)
        float pulseValue = (Mathf.Sin(pulseTimer) + 1f) * 0.5f;
        
        // Apply to light
        fireflyLight.intensity = Mathf.Lerp(
            maxLightIntensity - pulseIntensity, 
            maxLightIntensity, 
            pulseValue
        );
        
        // Apply slight color variation
        Color currentColor = baseColor * (0.8f + pulseValue * 0.2f);
        fireflyLight.color = currentColor;
        spriteRenderer.color = currentColor;
    }
    
    private void SetNewTargetPosition()
    {
        // Get random position within wander radius
        Vector2 randomDirection = Random.insideUnitCircle.normalized;
        float randomDistance = Random.Range(wanderRadius * 0.3f, wanderRadius);
        
        targetPosition = startPosition + new Vector3(
            randomDirection.x * randomDistance,
            randomDirection.y * randomDistance,
            0
        );
    }
    
    // Create a simple circle sprite procedurally
    private Sprite CreateCircleSprite()
    {
        // Create a small texture for the circle
        int textureSize = 32;
        Texture2D texture = new Texture2D(textureSize, textureSize);
        
        // Generate circle pixels
        Color[] colors = new Color[textureSize * textureSize];
        Color circleColor = Color.white;
        
        for (int y = 0; y < textureSize; y++)
        {
            for (int x = 0; x < textureSize; x++)
            {
                // Calculate distance from center
                float distX = x - textureSize / 2;
                float distY = y - textureSize / 2;
                float dist = Mathf.Sqrt(distX * distX + distY * distY);
                
                int index = y * textureSize + x;
                if (dist <= textureSize / 2)
                {
                    // Smooth edges
                    float alpha = 1f;
                    if (dist > textureSize / 2 - 2)
                    {
                        alpha = 1f - (dist - (textureSize / 2 - 2)) / 2f;
                    }
                    
                    colors[index] = new Color(1f, 1f, 1f, alpha);
                }
                else
                {
                    colors[index] = Color.clear;
                }
            }
        }
        
        texture.SetPixels(colors);
        texture.Apply();
        
        // Create sprite from texture
        return Sprite.Create(
            texture, 
            new Rect(0, 0, textureSize, textureSize), 
            new Vector2(0.5f, 0.5f), 
            100f
        );
    }
}