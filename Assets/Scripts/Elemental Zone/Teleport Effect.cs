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

    private TeleportEffectType currentEffectType;
    private float elapsedTime;

    public void SetType(TeleportEffectType type)
    {
        currentEffectType = type;
        InitializeEffect();
    }

    private void InitializeEffect()
    {
        // Set up different effects based on type
        if (currentEffectType == TeleportEffectType.Origin)
        {
            // Origin effects (fade out)
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
        }
        else
        {
            // Destination effects (fade in)
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
        }

        // Start the destroy timer
        StartCoroutine(DestroyEffect());
    }

    private IEnumerator DestroyEffect()
    {
        yield return new WaitForSeconds(duration);
        Destroy(gameObject);
    }
}