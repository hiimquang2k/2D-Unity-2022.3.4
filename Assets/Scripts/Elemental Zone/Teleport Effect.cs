using UnityEngine;
using System.Collections;

public class PixelTeleportEffect : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ParticleSystem teleportParticleSystem;
    [SerializeField] private Sprite particleSprite; // Assign a simple pixel art square/circle
    
    [Header("Teleport Settings")]
    [SerializeField] private float teleportDuration = 1.0f;
    [SerializeField] private Color disappearColor = new Color(0, 1, 1, 1); // Cyan
    [SerializeField] private Color appearColor = new Color(0, 0.5f, 1, 1); // Blue
    [SerializeField] private int particleCount = 20; // Keep this lower for pixel art style
    [SerializeField] private float particleSize = 1.0f; // In pixel units
    
    private SpriteRenderer spriteRenderer;
    private Vector3 originalScale;
    
    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalScale = transform.localScale;
        
        // Set up the particle system if not assigned
        if (teleportParticleSystem == null)
        {
            teleportParticleSystem = GetComponentInChildren<ParticleSystem>();
            if (teleportParticleSystem == null)
            {
                GameObject particleObj = new GameObject("PixelParticles");
                particleObj.transform.SetParent(transform);
                particleObj.transform.localPosition = Vector3.zero;
                teleportParticleSystem = particleObj.AddComponent<ParticleSystem>();
                SetupPixelParticleSystem();
            }
        }
        else
        {
            SetupPixelParticleSystem();
        }
    }
    
    private void SetupPixelParticleSystem()
    {
        // Get the particle system renderer
        ParticleSystemRenderer psRenderer = teleportParticleSystem.GetComponent<ParticleSystemRenderer>();
        
        // Configure for pixel art
        psRenderer.renderMode = ParticleSystemRenderMode.Billboard;
        
        // Disable stretching using the correct properties
        var renderer = teleportParticleSystem.GetComponent<ParticleSystemRenderer>();
        renderer.alignment = ParticleSystemRenderSpace.World;
        
        // Rotation module setup instead of using enableRotation
        var rotationModule = teleportParticleSystem.rotationOverLifetime;
        rotationModule.enabled = false; // Disable rotation for pixel-perfect look
        
        psRenderer.sortMode = ParticleSystemSortMode.None;
        
        // Use a material that maintains pixel perfect rendering
        Material particleMat = new Material(Shader.Find("Sprites/Default"));
        if (particleSprite != null)
        {
            particleMat.mainTexture = particleSprite.texture;
        }
        psRenderer.material = particleMat;
        
        // Configure the main module for pixel perfect particles
        var main = teleportParticleSystem.main;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.startLifetime = teleportDuration / 2;
        main.startSize = particleSize;
        main.startSpeed = 2f;
        main.startColor = disappearColor;
        main.gravityModifier = 0;
        
        // Turn off stretching with velocity
        var velocityModule = teleportParticleSystem.velocityOverLifetime;
        velocityModule.orbitalZ = 0;
    }
    
    private Mesh CreateQuadMesh()
    {
        // Create a simple quad mesh for pixel perfect particles
        Mesh mesh = new Mesh();
        
        Vector3[] vertices = new Vector3[4]
        {
            new Vector3(-0.5f, -0.5f, 0),
            new Vector3(0.5f, -0.5f, 0),
            new Vector3(-0.5f, 0.5f, 0),
            new Vector3(0.5f, 0.5f, 0)
        };
        
        int[] triangles = new int[6]
        {
            0, 2, 1,
            2, 3, 1
        };
        
        Vector2[] uv = new Vector2[4]
        {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(0, 1),
            new Vector2(1, 1)
        };
        
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;
        
        return mesh;
    }
    
    public void TriggerTeleport(Vector3 destination)
    {
        StartCoroutine(TeleportSequence(destination));
    }
    
    private IEnumerator TeleportSequence(Vector3 destination)
    {
        // Play disappear effect
        PlayDisappearEffect();
        
        // Wait for half of the teleport duration
        yield return new WaitForSeconds(teleportDuration / 2);
        
        // Move to destination
        transform.position = destination;
        
        // Play appear effect
        PlayAppearEffect();
    }
    
    private void PlayDisappearEffect()
    {
        // Stop any running particle system
        teleportParticleSystem.Stop();
        
        // Configure burst for pixel-style disappear
        var emission = teleportParticleSystem.emission;
        var burst = new ParticleSystem.Burst(0.0f, particleCount);
        emission.SetBurst(0, burst);
        
        // Set shape to match the sprite
        var shape = teleportParticleSystem.shape;
        shape.shapeType = ParticleSystemShapeType.Rectangle;
        if (spriteRenderer != null)
        {
            Vector2 spriteSize = spriteRenderer.sprite.bounds.size;
            shape.scale = new Vector3(spriteSize.x, spriteSize.y, 0.1f);
        }
        else
        {
            shape.scale = new Vector3(1, 1, 0.1f);
        }
        
        // Configure velocity for outward movement
        var velocity = teleportParticleSystem.velocityOverLifetime;
        velocity.enabled = true;
        velocity.space = ParticleSystemSimulationSpace.Local;
        velocity.radial = new ParticleSystem.MinMaxCurve(1f, 3f);
        
        // Main module setup for disappearing
        var main = teleportParticleSystem.main;
        main.startColor = disappearColor;
        
        // Start the particle system
        teleportParticleSystem.Play();
        
        // Pixel-perfect scaling down of the sprite
        if (spriteRenderer != null)
        {
            StartCoroutine(PixelScaleDown());
        }
    }
    
    private void PlayAppearEffect()
    {
        // Stop any running particle system
        teleportParticleSystem.Stop();
        
        // Configure the particle system for appear effect with pixel-art style
        var main = teleportParticleSystem.main;
        main.startColor = appearColor;
        
        // Configure burst for pixel-style appear
        var emission = teleportParticleSystem.emission;
        var burst = new ParticleSystem.Burst(0.0f, particleCount);
        emission.SetBurst(0, burst);
        
        // Configure velocity for inward movement
        var velocity = teleportParticleSystem.velocityOverLifetime;
        velocity.enabled = true;
        velocity.space = ParticleSystemSimulationSpace.Local;
        velocity.radial = new ParticleSystem.MinMaxCurve(-3f, -1f);
        
        // Start the particle system
        teleportParticleSystem.Play();
        
        // Pixel-perfect scaling up of the sprite
        if (spriteRenderer != null)
        {
            StartCoroutine(PixelScaleUp());
        }
    }
    
    private IEnumerator PixelScaleDown()
    {
        // Scale down in pixel-perfect steps
        int steps = 5; // Using fewer steps for a more noticeable pixel art effect
        float timePerStep = (teleportDuration / 2) / steps;
        
        for (int i = steps; i >= 0; i--)
        {
            float scaleFactor = (float)i / steps;
            transform.localScale = new Vector3(
                Mathf.Round(originalScale.x * scaleFactor * 4) / 4, 
                Mathf.Round(originalScale.y * scaleFactor * 4) / 4, 
                originalScale.z
            );
            
            // Also adjust alpha if we have a sprite renderer
            if (spriteRenderer != null)
            {
                Color c = spriteRenderer.color;
                c.a = scaleFactor;
                spriteRenderer.color = c;
            }
            
            yield return new WaitForSeconds(timePerStep);
        }
        
        // Hide completely at the end
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }
    }
    
    private IEnumerator PixelScaleUp()
    {
        // Re-enable the sprite renderer
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }
        
        // Scale up in pixel-perfect steps
        int steps = 5; // Using fewer steps for a more noticeable pixel art effect
        float timePerStep = (teleportDuration / 2) / steps;
        
        for (int i = 0; i <= steps; i++)
        {
            float scaleFactor = (float)i / steps;
            transform.localScale = new Vector3(
                Mathf.Round(originalScale.x * scaleFactor * 4) / 4, 
                Mathf.Round(originalScale.y * scaleFactor * 4) / 4, 
                originalScale.z
            );
            
            // Also adjust alpha if we have a sprite renderer
            if (spriteRenderer != null)
            {
                Color c = spriteRenderer.color;
                c.a = scaleFactor;
                spriteRenderer.color = c;
            }
            
            yield return new WaitForSeconds(timePerStep);
        }
        
        // Ensure we end with the exact original scale
        transform.localScale = originalScale;
    }
}