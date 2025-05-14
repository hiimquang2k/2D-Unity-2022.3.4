using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(MoonLightController))]
public class MoonVisualEffect : MonoBehaviour
{
    [Header("Visual Settings")]
    [SerializeField] private bool createMoonSprite = true;
    [SerializeField] private float moonSpriteSize = 2f;
    [SerializeField] private Color moonTint = new Color(0.9f, 0.95f, 1f);
    [SerializeField] private float moonGlowSize = 3f;

    [Header("Atmospheric Effects")]
    [SerializeField] private bool createStars = true;
    [SerializeField] private int starCount = 50;
    [SerializeField] private float starFieldRadius = 20f;
    [SerializeField] private float starTwinkleSpeed = 1f;
    
    // References
    private MoonLightController moonController;
    private SpriteRenderer moonSprite;
    private SpriteRenderer moonGlow;
    private List<SpriteRenderer> stars = new List<SpriteRenderer>();
    
    // Parent transform for organizing effects
    private Transform effectsContainer;

    private void Start()
    {
        // Get controller reference
        moonController = GetComponent<MoonLightController>();
        
        // Create container for effects
        effectsContainer = new GameObject("Moon Effects").transform;
        effectsContainer.SetParent(transform);
        effectsContainer.localPosition = Vector3.zero;
        
        // Create visual elements
        if (createMoonSprite)
        {
            CreateMoonVisuals();
        }
        
        if (createStars)
        {
            CreateStarField();
        }
    }
    
    private void CreateMoonVisuals()
    {
        // Create moon sprite
        GameObject moonObj = new GameObject("Moon Sprite");
        moonObj.transform.SetParent(effectsContainer);
        moonObj.transform.localPosition = Vector3.zero;
        
        moonSprite = moonObj.AddComponent<SpriteRenderer>();
        moonSprite.sprite = CreateCircleSprite(32, Color.white);
        moonSprite.color = moonTint;
        moonSprite.sortingLayerName = "Background";
        moonSprite.sortingOrder = 10;
        moonSprite.transform.localScale = Vector3.one * moonSpriteSize;
        
        // Create moon glow
        GameObject glowObj = new GameObject("Moon Glow");
        glowObj.transform.SetParent(effectsContainer);
        glowObj.transform.localPosition = Vector3.zero;
        
        moonGlow = glowObj.AddComponent<SpriteRenderer>();
        moonGlow.sprite = CreateCircleSprite(32, new Color(1f, 1f, 1f, 0.5f));
        moonGlow.color = new Color(moonTint.r, moonTint.g, moonTint.b, 0.4f);
        moonGlow.sortingLayerName = "Background";
        moonGlow.sortingOrder = 9;
        moonGlow.transform.localScale = Vector3.one * moonGlowSize;
        
        // Start pulsing glow effect
        StartCoroutine(PulseGlow());
    }
    
    private void CreateStarField()
    {
        GameObject starsContainer = new GameObject("Stars");
        starsContainer.transform.SetParent(effectsContainer);
        starsContainer.transform.localPosition = Vector3.zero;
        
        for (int i = 0; i < starCount; i++)
        {
            // Random position around moon
            Vector2 randomDir = Random.insideUnitCircle.normalized * Random.Range(moonSpriteSize, starFieldRadius);
            Vector3 position = new Vector3(randomDir.x, randomDir.y, 0);
            
            // Create star
            GameObject star = new GameObject($"Star_{i}");
            star.transform.SetParent(starsContainer.transform);
            star.transform.localPosition = position;
            
            // Add sprite
            SpriteRenderer starSprite = star.AddComponent<SpriteRenderer>();
            starSprite.sprite = CreateCircleSprite(8, Color.white);
            
            // Random size
            float starSize = Random.Range(0.5f, 1f);
            starSprite.transform.localScale = Vector3.one * starSize;
            
            // Random brightness
            float brightness = Random.Range(0.5f, 1.0f);
            starSprite.color = new Color(1f, 1f, 1f, brightness);
            
            // Sorting
            starSprite.sortingLayerName = "Background";
            starSprite.sortingOrder = 8;
            
            // Add to list for animation
            stars.Add(starSprite);
            
            // Start twinkling
            StartCoroutine(TwinkleStar(starSprite, Random.Range(0.5f, 2f)));
        }
    }
    
    // Utility to create simple circle sprites
    private Sprite CreateCircleSprite(int resolution, Color color)
    {
        Texture2D texture = new Texture2D(resolution, resolution);
        Vector2 center = new Vector2(resolution / 2, resolution / 2);
        float radius = resolution / 2;
        
        // Create circle
        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                
                if (distance <= radius)
                {
                    // Calculate alpha based on distance from center
                    float alpha = 1f;
                    if (distance > radius * 0.8f)
                    {
                        alpha = 1f - ((distance - radius * 0.8f) / (radius * 0.2f));
                    }
                    
                    texture.SetPixel(x, y, new Color(color.r, color.g, color.b, alpha * color.a));
                }
                else
                {
                    texture.SetPixel(x, y, Color.clear);
                }
            }
        }
        
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, resolution, resolution), new Vector2(0.5f, 0.5f), 100);
    }
    
    // Animation coroutines
    private IEnumerator PulseGlow()
    {
        while (true)
        {
            float pulseTime = 0;
            float pulseDuration = Random.Range(2f, 4f);
            
            while (pulseTime < pulseDuration)
            {
                pulseTime += Time.deltaTime;
                float t = pulseTime / pulseDuration;
                
                // Sine wave for smooth pulsing
                float pulse = Mathf.Sin(t * Mathf.PI) * 0.2f + 0.8f;
                
                // Apply to glow scale
                moonGlow.transform.localScale = Vector3.one * moonGlowSize * pulse;
                
                yield return null;
            }
            
            yield return new WaitForSeconds(Random.Range(0.5f, 2f));
        }
    }
    
    private IEnumerator TwinkleStar(SpriteRenderer star, float speed)
    {
        float baseAlpha = star.color.a;
        float time = Random.Range(0f, 10f); // Randomize starting point
        
        while (true)
        {
            time += Time.deltaTime * speed * starTwinkleSpeed;
            
            // Calculate alpha with sine wave
            float alpha = baseAlpha * (0.6f + 0.4f * Mathf.Sin(time));
            
            // Apply to star color
            Color c = star.color;
            star.color = new Color(c.r, c.g, c.b, alpha);
            
            yield return null;
        }
    }
}