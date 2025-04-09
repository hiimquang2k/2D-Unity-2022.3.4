using UnityEngine;
using UnityEngine.VFX; // Add this namespace
using System.Collections.Generic;

public class ElementalZoneManager : MonoBehaviour
{
    [System.Serializable]
    public enum ElementType
    {
        Electric
    }

    public List<Transform> electricZones;
    public float zoneRadius = 5f;
    [SerializeField] private VisualEffect lightningVFX; // Changed from GameObject to VFX reference
    [SerializeField] private float vfxDuration = 0.5f;
    [SerializeField] private float lightningHeight = 4f;
 
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

    void HandleLightningStrikes()
    {
        if (Time.time - lastLightningStrikeTime >= lightningInterval && lightningVFX != null)
        {
            foreach (GameObject taggedMonster in taggedMonsters)
            {
                if (taggedMonster != null)
                {
                    // Create rotation (X:-90, Y/Z preserved)
                    Quaternion rotation = Quaternion.Euler(-90f, 0f, 0f);
                    
                    // Instantiate with rotation
                    VisualEffect newVFX = Instantiate(
                        lightningVFX,
                        taggedMonster.transform.position + Vector3.up * lightningHeight,
                        rotation
                    );

                    newVFX.Play();
                    Destroy(newVFX.gameObject, vfxDuration);

                    ApplyLightningDamage(taggedMonster);
                }
            }
            lastLightningStrikeTime = Time.time;
        }
    }

    void StopVFX()
    {
        if (lightningVFX != null)
            lightningVFX.Stop();
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