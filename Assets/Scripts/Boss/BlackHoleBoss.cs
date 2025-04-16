using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;

public class BlackHoleBoss : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private HealthSystem healthSystem;

    [Header("Meteor Settings")]
    [SerializeField] private GameObject meteorPrefab;
    [SerializeField] private float meteorSpawnHeight = 5f;
    [SerializeField] private float baseMeteorFallSpeed = 5f;
    [SerializeField] private int baseMeteorDamage = 10;
    [SerializeField] private float baseMeteorImpactRadius = 1f;

    [Header("Destruction Settings")]
    [SerializeField] private Tilemap destructibleTilemap;
    [SerializeField] private NonOverlappingParallax parallaxSystem;
    [SerializeField] private float[] phaseThresholds = { 0.66f, 0.33f, 0f };
    [SerializeField] private float[] destructionWidths = { 0.33f, 0.66f, 1f };
    
    private int currentPhase = 0;
    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
        healthSystem.OnHealthChanged += HandleHealthChanged;
    }

    private void HandleHealthChanged(int current, int max)
    {
        CheckPhaseTransition((float)current / max);
    }

    private void CheckPhaseTransition(float healthPercent)
    {
        for (int i = currentPhase; i < phaseThresholds.Length; i++)
        {
            if (healthPercent <= phaseThresholds[i])
            {
                currentPhase = i + 1;
                StartCoroutine(TriggerDestructionWave());
                break;
            }
        }
    }

    private IEnumerator TriggerDestructionWave()
    {
        int meteorsToSpawn = 5 + (currentPhase * 3);
        float delayBetween = 0.2f;
        
        for (int i = 0; i < meteorsToSpawn; i++)
        {
            SpawnDestructionMeteor();
            yield return new WaitForSeconds(delayBetween);
        }
    }

    private void SpawnDestructionMeteor()
    {
        if (meteorPrefab == null) return;

        Vector2 viewportMin = mainCamera.ViewportToWorldPoint(Vector2.zero);
        Vector2 viewportMax = mainCamera.ViewportToWorldPoint(Vector2.one);
        
        Vector2 spawnPos = new Vector2(
            Random.Range(viewportMin.x, viewportMax.x),
            viewportMax.y + meteorSpawnHeight
        );

        GameObject meteor = Instantiate(meteorPrefab, spawnPos, Quaternion.identity);
        Meteor meteorScript = meteor.GetComponent<Meteor>();
        
        if (meteorScript != null)
        {
            float phaseMultiplier = currentPhase + 1;
            float width = (viewportMax.x - viewportMin.x) * destructionWidths[currentPhase];
            
            meteorScript.Initialize(
    fallSpeed: baseMeteorFallSpeed * phaseMultiplier,
    damage: Mathf.RoundToInt(baseMeteorDamage * phaseMultiplier),
    radius: baseMeteorImpactRadius * phaseMultiplier, // Changed parameter name
    width: width,
    chance: 0.7f, // Changed parameter name
    tilemap: destructibleTilemap, // Changed parameter name
    parallax: parallaxSystem // Changed parameter name
);

        }
    }

    public void ProcessIncomingDamage(int rawDamage, DamageType type)
    {
        int finalDamage = CalculateDamage(rawDamage, type);
        healthSystem.TakeDamage(finalDamage, type);
    }

    private int CalculateDamage(int rawDamage, DamageType type)
    {
        float multiplier = 1f;
        
        switch(type)
        {
            case DamageType.Fire:
                multiplier = 0.8f;
                break;
            case DamageType.Ice:
                multiplier = 1.2f;
                break;
        }
        
        return Mathf.RoundToInt(rawDamage * multiplier);
    }
}
