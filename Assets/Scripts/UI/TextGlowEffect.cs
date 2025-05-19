using UnityEngine;
using TMPro;

public class TextGlowEffect : MonoBehaviour
{
    [Header("Glow Properties")]
    public float glowSpeed = 1f;
    public Color glowColor = Color.white;
    public float glowIntensity = 1f;
    public float glowPulseSpeed = 1f;
    public float glowPulseAmount = 0.5f;

    [Header("Particle Effects")]
    public GameObject particlePrefab;
    public float particleSpawnRate = 0.5f;
    public float particleLifetime = 2f;

    private TMP_Text text;
    private float particleTimer;

    void Start()
    {
        text = GetComponent<TMP_Text>();
    }

    void Update()
    {
        // Update glow effect
        float pulse = Mathf.Sin(Time.time * glowPulseSpeed) * glowPulseAmount + 1f;
        text.material.SetFloat("_GlowPower", glowIntensity * pulse);
        text.material.SetColor("_GlowColor", glowColor);

        // Handle particle effects
        particleTimer += Time.deltaTime;
        if (particleTimer >= particleSpawnRate)
        {
            SpawnParticle();
            particleTimer = 0f;
        }
    }

    void SpawnParticle()
    {
        if (particlePrefab == null) return;

        // Get random character position
        var textInfo = text.textInfo;
        int charIndex = Random.Range(0, textInfo.characterCount);
        
        if (charIndex < textInfo.characterCount)
        {
            var character = textInfo.characterInfo[charIndex];
            if (character.isVisible)
            {
                var pos = text.transform.TransformPoint(character.bottomLeft);
                Instantiate(particlePrefab, pos, Quaternion.identity);
            }
        }
    }
}
