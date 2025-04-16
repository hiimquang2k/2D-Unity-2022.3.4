// ElementManager.cs
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

        // Activate skill
        if (Input.GetButtonDown("Skill"))
        {
            switch (activeElement)
            {
                case Element.Lightning: 
                    lightningSkill.Activate();
                    break;
                case Element.Fire:
                    fireSkill.Activate();
                    break;
                case Element.Water:
                    waterSkill.Activate();
                    break;
                case Element.Earth:
                    earthSkill.Activate();
                    break;
            }
        }
    }

    void UpdateVisuals()
    {
        // Glow 2D color change
        var main = swordGlow.main;
        main.startColor = activeElement switch
        {
            Element.Lightning => Color.blue,
            Element.Fire => Color.red,
            Element.Water => Color.cyan,
            Element.Earth => Color.green,
            _ => Color.white
        };

        // Play SFX
        AudioSource.PlayClipAtPoint(switchSFX[elementIndex], transform.position);
    }
}