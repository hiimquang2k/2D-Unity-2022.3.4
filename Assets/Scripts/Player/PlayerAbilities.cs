using UnityEngine;
using System.Collections;

public class PlayerAbilities : MonoBehaviour
{
    [Header("Lightning Strike Settings")]
    [SerializeField] private GameObject lightningPrefab;
    [SerializeField] private float lightningCooldown = 2f;
    [SerializeField] private float lightningRange = 10f;
    
    [Header("Teleport Settings")]
    [SerializeField] private GameObject teleportEffectPrefab;
    [SerializeField] private float teleportCooldown = 3f;
    [SerializeField] private LayerMask groundLayer;

    [Header("References")]
    [SerializeField] private ElementalZoneManager elementalZoneManager;
    [SerializeField] private DirectionManager directionManager;

    private CooldownSystem cooldownSystem;
    private PlayerMovement playerMovement;
    private Transform playerTransform;
    private Collider2D characterCollider;

    private void Awake()
    {
        playerTransform = transform;
        characterCollider = GetComponent<Collider2D>();
        if (cooldownSystem == null)
        {
            cooldownSystem = gameObject.AddComponent<CooldownSystem>();
        }
        
    }

    private void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
        directionManager = GetComponent<DirectionManager>();
        elementalZoneManager = GameObject.Find("ElementalZone").GetComponent<ElementalZoneManager>();
    }

    private void Update()
    {
        HandleLightningStrike();
        HandleTeleport();
        cooldownSystem.UpdateCooldown();
    }

    private void HandleLightningStrike()
    {
        if (Input.GetKeyDown(KeyCode.L) && !cooldownSystem.IsOnCooldown("Lightning"))
        {
            Vector2 targetPosition = GetTargetPosition();
            if (targetPosition != Vector2.zero)
            {
                SummonLightningStrike(targetPosition);
                cooldownSystem.StartCooldown(lightningCooldown, "Lightning");
            }
        }
    }

    private void HandleTeleport()
    {
        if (Input.GetKeyDown(KeyCode.T) && !cooldownSystem.IsOnCooldown("Teleport"))
        {
            Debug.Log("Pressed T");
            Transform targetZone = GetTargetZone();
            if (targetZone != null)
            {
                TeleportToZone(targetZone);
                cooldownSystem.StartCooldown(teleportCooldown, "Teleport");
            }
        }
    }

     private Transform GetTargetZone()
    {
        // Get player's current position
        Vector2 playerPosition = playerTransform.position;

        // Initialize variables to track the closest zone
        Transform closestZone = null;
        float closestDistance = float.MaxValue;

        // Check all elemental zones
        foreach (Transform zone in elementalZoneManager.electricZones)
        {
            // Get zone position and convert to Vector2
            Vector2 zonePosition = zone.position;
            
            // Calculate direction from player to zone
            Vector2 directionToZone = (zonePosition - playerPosition).normalized;
            
            // Calculate angle between player's direction and direction to zone
            float angle = Vector2.Angle(directionManager.IsFacingRight ? Vector2.right : Vector2.left, directionToZone);
            
            // Only consider zones within a certain angle (e.g., 90 degrees)
            if (angle <= 45f)  // Adjust this value to change the cone width
            {
                // Calculate distance to zone
                float distance = Vector2.Distance(playerPosition, zonePosition);
                
                // Check if this is the closest zone within the cone
                if (distance < closestDistance)
                {
                    closestZone = zone;
                    closestDistance = distance;
                }
            }
        }

        return closestZone;
    }

    private Vector2 GetTargetPosition()
    {
        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, lightningRange, groundLayer);
        if (hit)
        {
            return hit.point;
        }
        return Vector2.zero;
    }

    private void TeleportToZone(Transform targetZone)
    {
        if (playerMovement == null) return;

        // Store original position
        Vector2 originalPosition = playerTransform.position;

        // Create teleport effect at origin
        CreateTeleportEffect(originalPosition, true);

        // Calculate target position within the zone
        Vector2 targetPosition = GetSafePositionInZone(targetZone);
        if (targetPosition != Vector2.zero)
        {
            // Move player to target position
            playerTransform.position = targetPosition;

            // Create teleport effect at destination
            CreateTeleportEffect(targetPosition, false);

            // Add a small delay to prevent immediate movement
            StartCoroutine(PreventMovementAfterTeleport());
        }
    }

    private Vector2 GetSafePositionInZone(Transform zone)
    {
        // Calculate a position within the zone's radius
        float radius = elementalZoneManager.zoneRadius;
        Vector2 center = zone.position;

        // Try to find a valid position
        for (int i = 0; i < 10; i++)  // Try multiple times to find a valid position
        {
            // Generate a random position within the circle
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float distance = Random.Range(0f, radius);
            Vector2 position = center + new Vector2(Mathf.Cos(angle) * distance, Mathf.Sin(angle) * distance);

            // Check if position is valid (on ground)
            if (characterCollider != null)
            {
                // Get the character's collider bounds
                Bounds bounds = characterCollider.bounds;
                
                // Check ground below the character's feet
                RaycastHit2D hit = Physics2D.Raycast(position + new Vector2(0, bounds.extents.y), Vector2.down, bounds.extents.y * 2f, groundLayer);
                
                // Check if there's enough space for the character
                Collider2D[] colliders = Physics2D.OverlapBoxAll(position, bounds.size, 0, groundLayer);
                
                if (hit && colliders.Length == 0) // Ground hit and no obstacles
                {
                    return position;
                }
            }
            else
            {
                // Fallback to simple raycast if no collider
                RaycastHit2D hit = Physics2D.Raycast(position, Vector2.down, 0.5f, groundLayer);
                if (hit)
                {
                    return position;
                }
            }
        }

        // If no valid position found, return center of zone
        return center;
    }

    private void SummonLightningStrike(Vector2 targetPosition)
    {
        if (lightningPrefab != null)
        {
            Instantiate(lightningPrefab, targetPosition, Quaternion.identity);
            // Optional: Add damage logic here
        }
    }

    private void Teleport(Vector2 targetPosition)
    {
        if (playerMovement == null) return;

        // Store original position
        Vector2 originalPosition = playerTransform.position;

        // Create teleport effect at origin
        CreateTeleportEffect(originalPosition, true);

        // Move player instantly
        playerTransform.position = targetPosition;

        // Create teleport effect at destination
        CreateTeleportEffect(targetPosition, false);

        // Add a small delay to prevent immediate movement
        StartCoroutine(PreventMovementAfterTeleport());
    }

    private void CreateTeleportEffect(Vector2 position, bool isOrigin)
{
    if (teleportEffectPrefab == null)
    {
        Debug.LogError("Teleport effect prefab is not assigned!");
        return;
    }
    // Create a particle effect or visual effect at the position
    // You can create a prefab for this effect
    GameObject effect = Instantiate(teleportEffectPrefab, position, Quaternion.identity);
    
    // Destroy the effect after a short duration
    Destroy(effect, 0.5f);
}

    private IEnumerator PreventMovementAfterTeleport()
    {
        // Prevent movement for a short duration after teleport
        playerMovement.enabled = false;
        yield return new WaitForSeconds(0.1f);
        playerMovement.enabled = true;
    }
}