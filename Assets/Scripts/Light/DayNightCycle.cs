using UnityEngine;


public class DayNightCycle : MonoBehaviour
{
    [Header("Cycle Settings")]
    [Range(1, 60)] public float cycleDurationMinutes = 5f;
    [Range(0, 1)] public float timeOfDay = 0f; // 0 = midnight, 0.25 = sunrise, 0.5 = noon, 0.75 = sunset
    public bool autoUpdate = true;
    
    [Header("Lighting References")]
    public UnityEngine.Rendering.Universal.Light2D globalLight;
    public UnityEngine.Rendering.Universal.Light2D moonLight;
    public Color dayColor = Color.white;
    public Color nightColor = new Color(0.2f, 0.2f, 0.5f);
    public Color sunsetColor = new Color(1f, 0.6f, 0.3f);
    
    [Header("Intensity Settings")]
    public float dayIntensity = 1f;
    public float nightIntensity = 0.2f;
    public float moonIntensity = 0.3f;
    
    private float cycleTimeInSeconds;
    
    void Start()
    {
        cycleTimeInSeconds = cycleDurationMinutes * 60f;
        UpdateLighting();
    }
    
    void Update()
    {
        if (autoUpdate)
        {
            timeOfDay += Time.deltaTime / cycleTimeInSeconds;
            
            if (timeOfDay >= 1f)
                timeOfDay -= 1f;
        }
        
        UpdateLighting();
    }
    
    void UpdateLighting()
    {
        // Calculate color and intensity based on time of day
        Color lightColor;
        float lightIntensity;
        
        // Day/night transition colors
        if (timeOfDay < 0.25f) // Night to sunrise (0.0 to 0.25)
        {
            float t = timeOfDay * 4f;
            lightColor = Color.Lerp(nightColor, sunsetColor, t);
            lightIntensity = Mathf.Lerp(nightIntensity, dayIntensity, t);
        }
        else if (timeOfDay < 0.5f) // Sunrise to noon (0.25 to 0.5)
        {
            float t = (timeOfDay - 0.25f) * 4f;
            lightColor = Color.Lerp(sunsetColor, dayColor, t);
            lightIntensity = dayIntensity;
        }
        else if (timeOfDay < 0.75f) // Noon to sunset (0.5 to 0.75)
        {
            float t = (timeOfDay - 0.5f) * 4f;
            lightColor = Color.Lerp(dayColor, sunsetColor, t);
            lightIntensity = dayIntensity;
        }
        else // Sunset to night (0.75 to 1.0)
        {
            float t = (timeOfDay - 0.75f) * 4f;
            lightColor = Color.Lerp(sunsetColor, nightColor, t);
            lightIntensity = Mathf.Lerp(dayIntensity, nightIntensity, t);
        }
        
        // Set global light properties
        if (globalLight != null)
        {
            globalLight.color = lightColor;
            globalLight.intensity = lightIntensity;
        }
        
        // Set moon light properties - only visible at night
        if (moonLight != null)
        {
            float moonVisibility = (timeOfDay > 0.7f || timeOfDay < 0.3f) ? 1f : 0f;
            if (timeOfDay > 0.6f && timeOfDay <= 0.7f)
                moonVisibility = (timeOfDay - 0.6f) * 10f;
            else if (timeOfDay >= 0.3f && timeOfDay < 0.4f)
                moonVisibility = 1f - ((timeOfDay - 0.3f) * 10f);
                
            moonLight.intensity = moonIntensity * moonVisibility;
        }
    }
}