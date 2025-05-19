using UnityEngine;
using TMPro;

public class JapaneseTitleEffect : MonoBehaviour
{
    [Header("Text Properties")]
    public TMP_Text titleText;
    public Color baseColor = new Color(1f, 0.8f, 0.8f, 1f); // Warm Japanese color
    public Color glowColor = new Color(1f, 0.6f, 0.6f, 1f); // Darker glow color
    public float glowIntensity = 0.8f;
    public float outlineThickness = 0.05f;
    public Color outlineBaseColor = new Color(0.1f, 0.1f, 0.1f, 1f); // Base outline color

    [Header("Animation Properties")]
    public float pulseSpeed = 0.5f;
    public float pulseAmount = 0.8f;
    public float cosmicEffectSpeed = 1f;
    public float cosmicEffectIntensity = 0.5f;

    [Header("Rainbow Outline")]
    public float rainbowSpeed = 0.5f; // Speed of rainbow cycling
    public float rainbowIntensity = 1f; // How strong the rainbow effect is
    public float rainbowOffset = 0.1f; // Offset between colors

    private float pulseValue = 0f;
    private float cosmicValue = 0f;
    private float rainbowTime = 0f;

    void Start()
    {
        if (titleText == null)
        {
            titleText = GetComponent<TMP_Text>();
            if (titleText == null)
            {
                Debug.LogError("No TMP_Text component found!");
                return;
            }
        }

        // Set initial properties
        UpdateTextProperties();
    }

    void Update()
    {
        // Update pulse effect
        pulseValue = Mathf.Sin(Time.time * pulseSpeed) * pulseAmount + 1f;
        cosmicValue = Mathf.Sin(Time.time * cosmicEffectSpeed) * cosmicEffectIntensity;

        // Update rainbow time
        rainbowTime += Time.deltaTime * rainbowSpeed;

        // Update text properties
        UpdateTextProperties();
    }

    void UpdateTextProperties()
    {
        if (titleText == null) return;

        // Calculate rainbow color using HSV
        float hue = (rainbowTime * rainbowSpeed) % 1f;
        Color rainbowColor = Color.HSVToRGB(hue, 1f, 1f);

        // Create gradient effect
        Color finalColor = Color.Lerp(outlineBaseColor, rainbowColor, pulseValue * rainbowIntensity);

        // Set face color with pulse
        titleText.color = Color.Lerp(baseColor, glowColor, pulseValue * glowIntensity);

        // Set outline with rainbow effect
        titleText.outlineWidth = outlineThickness;
        titleText.outlineColor = finalColor;

        // Add cosmic effect
        titleText.material.SetFloat("_GlowPower", cosmicValue * 3f);
        titleText.material.SetColor("_GlowColor", Color.Lerp(baseColor, Color.white, cosmicValue * 0.8f));
    }
}