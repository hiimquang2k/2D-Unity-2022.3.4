using UnityEngine;
using System.Collections.Generic;

public class ElementalZoneManager : MonoBehaviour
{
    [System.Serializable]
    public enum ElementType
    {
        Electric
    }

    [SerializeField] public List<Transform> electricZones;
    [SerializeField] public float zoneRadius = 5f;
    [SerializeField] private GameObject lightningPrefab;

    // Tagging Configuration
    [SerializeField] private float tagDuration = 5f;
    [SerializeField] private float lightningInterval = 1f;

    private List<GameObject> taggedMonsters = new List<GameObject>();
    private float lastLightningStrikeTime;

    void Update()
    {
        HandleMonsterTagging();
        HandleLightningStrikes();
        CleanupExpiredTags();
    }

    void HandleMonsterTagging()
    {
        foreach (Transform zoneTransform in electricZones)
        {
            // Find all colliders in the zone
            Collider2D[] colliders = Physics2D.OverlapCircleAll(
                zoneTransform.position, 
                zoneRadius
            );

            foreach (Collider2D collider in colliders)
            {
                // Specifically check for monsters by tag
                if (collider.gameObject.CompareTag("Monster"))
                {
                    TagMonster(collider.gameObject);
                }
            }
        }
    }

    void TagMonster(GameObject monster)
    {
        // Prevent duplicates
        if (!taggedMonsters.Contains(monster))
        {
            taggedMonsters.Add(monster);
            
            // Optional: Visual feedback for tagging
            ApplyTagEffects(monster);
        }
    }

    void HandleLightningStrikes()
    {
        if (Time.time - lastLightningStrikeTime >= lightningInterval)
        {
            foreach (GameObject taggedMonster in taggedMonsters)
            {
                if (taggedMonster != null)
                {
                    SummonLightningStrike(taggedMonster);
                }
            }
            lastLightningStrikeTime = Time.time;
        }
    }

    void SummonLightningStrike(GameObject taggedMonster)
    {
        if (lightningPrefab != null)
        {
            // Instantiate lightning effect
            Instantiate(lightningPrefab, taggedMonster.transform.position, Quaternion.identity);
            
            // Optional: Apply damage
            ApplyLightningDamage(taggedMonster);
        }
    }

    void CleanupExpiredTags()
    {
        // Remove null references
        taggedMonsters.RemoveAll(monster => monster == null);
    }

    void ApplyTagEffects(GameObject monster)
    {
        // Optional: Visual or audio feedback when tagging
        Debug.Log($"Monster tagged: {monster.name}");
    }

    void ApplyLightningDamage(GameObject monster)
    {
        // Basic damage application
        HealthSystem monsterHealth = monster.GetComponent<HealthSystem>();
        if (monsterHealth != null)
        {
            monsterHealth.TakeDamage(10, DamageType.Lightning);
        }
    }

    void OnDrawGizmosSelected()
    {
        // Visualize electric zones in scene view
        Gizmos.color = Color.yellow;
        foreach (Transform zoneTransform in electricZones)
        {
            Gizmos.DrawWireSphere(zoneTransform.position, zoneRadius);
        }
    }
}