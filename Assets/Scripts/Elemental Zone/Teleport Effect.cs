using UnityEngine;
using System.Collections;

public class TeleportEffect : MonoBehaviour
{
    [System.Serializable]
    public enum TeleportEffectType
    {
        Origin,
        Destination
    }

    [Header("Effect Settings")]
    [SerializeField] private float duration = 1f;
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private ParticleSystem[] particleEffects;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private SpriteRenderer screenRenderer;
    [SerializeField] private Material staticMaterial;
    [SerializeField] private Material scanlineMaterial;
    [SerializeField] private float staticIntensity = 1f;
    [SerializeField] private float scanlineSpeed = 2f;

    private TeleportEffectType currentEffectType;
    private float elapsedTime;
    private Color originalColor;
    private bool isPlaying = false;

    public void SetType(TeleportEffectType type)
    {
        currentEffectType = type;
        InitializeEffect();
    }

    private void InitializeEffect()
    {
        if (currentEffectType == TeleportEffectType.Origin)
        {
            if (screenRenderer != null)
            {
                originalColor = screenRenderer.color;
                screenRenderer.material = staticMaterial;
            }
            
            // Play particle effects and audio
            foreach (ParticleSystem ps in particleEffects)
            {
                if (ps != null)
                {
                    ps.Play();
                }
            }
            if (audioSource != null)
            {
                audioSource.Play();
            }
            
            isPlaying = true;
            StartCoroutine(TVShutdownEffect());
        }
        else
        {
            // Reset to normal state for destination
            if (screenRenderer != null)
            {
                screenRenderer.material = null;
                screenRenderer.color = originalColor;
            }
        }
    }

    private IEnumerator TVShutdownEffect()
    {
        float elapsedTime = 0f;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            
            // Update static intensity
            if (screenRenderer != null && staticMaterial != null)
            {
                float intensity = Mathf.Lerp(staticIntensity, 0f, elapsedTime / duration);
                staticMaterial.SetFloat("_StaticIntensity", intensity);
                
                // Add scanline effect
                float scanlineOffset = Mathf.Sin(elapsedTime * scanlineSpeed) * 0.1f;
                staticMaterial.SetFloat("_ScanlineOffset", scanlineOffset);
            }
            
            yield return null;
        }

        // Final fade out
        float fadeTime = 0f;
        while (fadeTime < fadeDuration)
        {
            fadeTime += Time.deltaTime;
            
            if (screenRenderer != null)
            {
                screenRenderer.color = new Color(
                    originalColor.r,
                    originalColor.g,
                    originalColor.b,
                    Mathf.Lerp(1f, 0f, fadeTime / fadeDuration)
                );
            }
            
            yield return null;
        }

        // Reset everything
        if (screenRenderer != null)
        {
            screenRenderer.material = null;
            screenRenderer.color = originalColor;
        }

        isPlaying = false;
    }

    private void Update()
    {
        if (isPlaying)
        {
            // Keep updating the effect while playing
            if (screenRenderer != null && staticMaterial != null)
            {
                staticMaterial.SetFloat("_Time", Time.time);
            }
        }
    }
}