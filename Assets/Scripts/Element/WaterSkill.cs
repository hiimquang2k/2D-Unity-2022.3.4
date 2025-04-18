using UnityEngine;

public class WaterSkill : MonoBehaviour
{
    [Header("References")]
    public Transform waterOrigin; // Sword tip position
    public GameObject whirlpoolPrefab;
    public GameObject waterSplashFX;

    [Header("Settings")]
    public float pullRadius = 3f;
    public float pullForce = 5f;
    public float floodRadius = 2f;

    public void Activate(bool isHolding)
    {
        if (isHolding)
        {
            FloodTerrain();
        }
        else
        {
            CreateWhirlpool();
        }
    }

    void FloodTerrain()
    {
    Collider2D[] hits = Physics2D.OverlapCircleAll(waterOrigin.position, floodRadius);
    foreach (var hit in hits)
    {
        ElementStatus status = hit.GetComponent<ElementStatus>();
        if (status != null) status.ApplyElement(Element.Water);
        
        // Existing fire extinguishing
        Burnable b = hit.GetComponent<Burnable>();
        if (b != null) b.Extinguish();
    }
    }

    void CreateWhirlpool()
    {
        GameObject whirlpool = Instantiate(whirlpoolPrefab, waterOrigin.position, Quaternion.identity);
        whirlpool.GetComponent<Whirlpool>().Initialize(pullRadius, pullForce);
    }
}