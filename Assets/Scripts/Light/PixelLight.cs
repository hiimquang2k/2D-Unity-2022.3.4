using UnityEngine;


[RequireComponent(typeof(UnityEngine.Rendering.Universal.Light2D))]
public class PixelPerfectLight2D : MonoBehaviour
{
    [Header("Flickering Settings")]
    public bool enableFlicker = false;
    public float flickerSpeed = 5f;
    public float flickerIntensity = 0.2f;
    
    [Header("Movement Settings")]
    public bool enableMovement = false;
    public Vector2 movementDirection = new Vector2(1, 0);
    public float movementSpeed = 1f;
    public float movementDistance = 1f;
    
    private UnityEngine.Rendering.Universal.Light2D light2D;
    private float baseIntensity;
    private Vector3 startPosition;
    
    void Start()
    {
        light2D = GetComponent<UnityEngine.Rendering.Universal.Light2D>();
        baseIntensity = light2D.intensity;
        startPosition = transform.position;
    }
    
    void Update()
    {
        if (enableFlicker)
        {
            // Add perlin noise based flickering
            float noise = Mathf.PerlinNoise(Time.time * flickerSpeed, 0) * 2 - 1;
            light2D.intensity = baseIntensity + (noise * flickerIntensity);
        }
        
        if (enableMovement)
        {
            // Simple sine wave movement
            float offset = Mathf.Sin(Time.time * movementSpeed) * movementDistance;
            Vector3 movement = new Vector3(
                movementDirection.x * offset,
                movementDirection.y * offset,
                0
            );
            transform.position = startPosition + movement;
        }
    }
}