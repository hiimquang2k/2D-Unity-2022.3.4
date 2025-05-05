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

    [Header("Black Hole Spawn Settings")]
    [SerializeField] private GameObject blackHolePrefab;
    [SerializeField] private float minSpawnInterval = 10f;
    [SerializeField] private float maxSpawnInterval = 15f;
    [SerializeField] private float spawnDistanceFromBoss = 5f;
    
    [Header("Teleport Settings")]
    [SerializeField] private float teleportCooldown = 8f;
    [SerializeField] private float minTeleportDistance = 5f;
    [SerializeField] private float maxTeleportDistance = 10f;

    [Header("Attack Settings")]
    [SerializeField] private float attackCooldown = 10f;
    [SerializeField] private float attackImpactRadius = 3f;
    [SerializeField] private int attackDamage = 20;
    [SerializeField] private LayerMask playerLayer;
    [Header("Audio")]
    [SerializeField] private AudioClip attackAudio;
    private AudioSource audioSource;

    [Header("Animations")]
    [SerializeField] private Animator bossAnimator;
    [SerializeField] private string spawnAnimTrigger = "Cast";
    [SerializeField] private string preTeleportTrigger = "PreTeleport";
    [SerializeField] private string teleportTrigger = "Teleport";
    [SerializeField] private string attackTrigger = "Attack";

    private ImprovedCameraShake cameraShake;
    private Transform playerTarget;
    private bool isActionInProgress;
    private Vector3 originalPosition;
    private bool attackImpactTriggered;

    private void Start()
    {
        cameraShake = Camera.main.GetComponent<ImprovedCameraShake>();
        playerTarget = GameObject.FindGameObjectWithTag("Player").transform;
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        StartCoroutine(BlackHoleSpawnCycle());
        StartCoroutine(TeleportRoutine());
        StartCoroutine(AttackRoutine());
        StartCoroutine(BackgroundMeteorShower());
    }

    private IEnumerator BackgroundMeteorShower()
    {
        while(true)
        {
            yield return new WaitForSeconds(Random.Range(3f, 5f));
            SpawnSingleMeteor();
        }
    }

    private void SpawnSingleMeteor()
    {
        Vector2 spawnPos = (Vector2)playerTarget.position + Random.insideUnitCircle * 10f;
        spawnPos.y += meteorSpawnHeight;
        
        GameObject meteor = Instantiate(meteorPrefab, spawnPos, Quaternion.identity);
        Meteor meteorScript = meteor.GetComponent<Meteor>();
        meteorScript.Initialize(baseMeteorFallSpeed, baseMeteorDamage, baseMeteorImpactRadius);
    }

    private IEnumerator AttackRoutine()
    {
        while(true)
        {
            yield return new WaitForSeconds(attackCooldown);
            if(!isActionInProgress) StartCoroutine(PerformAttack());
        }
    }

    private IEnumerator PerformAttack()
    {
        isActionInProgress = true;
        Vector3 originalPosition = transform.position;

        // 1. Pre-Teleport at original position
        FacePlayerDirection();
        bossAnimator.SetTrigger(preTeleportTrigger);
        yield return new WaitForSeconds(1.1f); // Match PreTeleport animation length

        // 2. Instant move to player position
        Vector3 attackPosition = playerTarget.position;
        transform.position = attackPosition;

        // 3. Play AttackAnim with jump-down animation
        bossAnimator.SetTrigger(attackTrigger);
        attackImpactTriggered = false;
        
        yield return new WaitForSeconds(1.25f);
        audioSource.PlayOneShot(attackAudio);
        // 4. Wait for attack impact event from animation
        yield return new WaitUntil(() => attackImpactTriggered);

        isActionInProgress = false;
    }

    // Animation Event called during strike frame
    public void OnAttackImpact()
    {
        ApplyAoEDamage();
        attackImpactTriggered = true;
        cameraShake?.ShakeCamera(2f);
    }

    private void ApplyAoEDamage()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackImpactRadius, playerLayer);
        foreach(Collider2D hit in hits)
        {
            if(hit.CompareTag("Player"))
            {
                hit.GetComponent<DamageSystem>()?.ApplyDamage(attackDamage, DamageType.Environmental, transform.position);
            }
        }
    }

    private void FacePlayerDirection()
    {
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * Mathf.Sign(playerTarget.position.x - transform.position.x);
        transform.localScale = scale;
    }

    private IEnumerator TeleportRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(teleportCooldown);
            if (!isActionInProgress) StartCoroutine(PerformTeleport());
        }
    }

    private IEnumerator PerformTeleport()
    {
        isActionInProgress = true;
        originalPosition = transform.position;

        // Phase 1: Pre-teleport animation
        bossAnimator.SetTrigger(preTeleportTrigger);
        yield return new WaitForSeconds(0.5f);

        // Phase 2: Calculate escape position
        Vector2 escapeDirection = (originalPosition - playerTarget.position).normalized;
        Vector2 teleportPosition = originalPosition + 
            (Vector3)(escapeDirection * Random.Range(minTeleportDistance, maxTeleportDistance));

        // Phase 3: Teleport animation at new position
        transform.position = teleportPosition;
        bossAnimator.SetTrigger(teleportTrigger);
        yield return new WaitForSeconds(0.75f);

        isActionInProgress = false;
    }

    private IEnumerator BlackHoleSpawnCycle()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(minSpawnInterval, maxSpawnInterval));
            
            // Trigger spawn animation
            if (bossAnimator != null)
            {
                bossAnimator.SetTrigger(spawnAnimTrigger);
            }

            // Wait for animation to reach casting point
            yield return new WaitForSeconds(0.5f);

            SpawnBlackHole();
        }
    }

    private void SpawnBlackHole()
    {
        if (blackHolePrefab == null) return;

        Vector2 spawnPos = GetSpawnPositionAroundPlayer();
        Instantiate(blackHolePrefab, spawnPos, Quaternion.identity);
    }

    private Vector2 GetSpawnPositionAroundPlayer()
    {
        Vector2 randomDirection = Random.insideUnitCircle.normalized;
        return (Vector2)playerTarget.position + (randomDirection * spawnDistanceFromBoss);
    }

    // Add this to handle animation events if needed
    public void OnCastAnimationComplete()
    {
        // Additional logic if needed
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
