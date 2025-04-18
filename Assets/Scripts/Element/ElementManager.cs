using UnityEngine;

public enum Element { Lightning, Fire, Water, Earth }

public class ElementManager : MonoBehaviour
{
    [Header("References")]
    public ParticleSystem swordGlow;
    public AudioClip[] switchSFX;
    public LightningSkill lightningSkill;
    public FireSkill fireSkill;
    public WaterSkill waterSkill;
    public EarthSkill earthSkill;
    public TileMorphController tileMorpher; // Add this reference

    [Header("Current State")]
    public Element activeElement;
    private int elementIndex = 0;

    void Update()
    {
        // Cycle elements
        if (Input.GetButtonDown("Enchant"))
        {
            elementIndex = (elementIndex + 1) % 4;
            activeElement = (Element)elementIndex;
            UpdateVisuals();
        }

        // Activate skill (with hold support for Water)
        if (Input.GetButtonDown("Skill"))
        {
            ActivateSkill(false); // Tap
        }
        if (Input.GetButton("Skill") && activeElement == Element.Water)
        {
            ActivateSkill(true); // Hold
        }
    }

    void ActivateSkill(bool isHolding)
    {
        switch (activeElement)
        {
            case Element.Lightning:
                lightningSkill.Activate();
                break;
            case Element.Fire:
                fireSkill.Activate();
                tileMorpher.MorphTiles(Element.Fire);
                break;
            case Element.Water:
                waterSkill.Activate(isHolding); // Pass hold state
                tileMorpher.MorphTiles(Element.Water);
                break;
            case Element.Earth:
                earthSkill.Activate();
                tileMorpher.MorphTiles(Element.Earth);
                break;
        }
    }

    void UpdateVisuals()
    {
        // Glow color change
        var main = swordGlow.main;
        main.startColor = activeElement switch
        {
            Element.Lightning => new Color(0.2f, 0.6f, 1f), // Electric blue
            Element.Fire => new Color(1f, 0.3f, 0f), // Orange-red
            Element.Water => new Color(0f, 0.8f, 1f), // Light cyan
            Element.Earth => new Color(0.5f, 0.3f, 0f), // Brown
            _ => Color.white
        };

        // Play SFX
        AudioSource.PlayClipAtPoint(switchSFX[elementIndex], transform.position);
    }
}