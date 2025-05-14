using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;

public class BlackHoleBoss : MonoBehaviour
{
        [Header("References")]
    [SerializeField] private BlackHoleBossData bossData;
    [SerializeField] private HealthSystem healthSystem;
    [SerializeField] private Animator bossAnimator;
    private AudioSource audioSource;
    private ImprovedCameraShake cameraShake;
    private Transform playerTarget;
    private bool isActionInProgress;
    private Vector3 originalPosition;
    private bool attackImpactTriggered;

    private void Start()
    {
        healthSystem.Initialize(bossData.maxHealth);
        cameraShake = FindObjectOfType<ImprovedCameraShake>();
        playerTarget = GameObject.FindGameObjectWithTag("Player").transform;
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        healthSystem.OnHealthChanged += (current, max) => UpdateHealthUI();
        healthSystem.OnDeath += ShowDeathUI;
        StartCoroutine(BlackHoleSpawnCycle());
        StartCoroutine(TeleportRoutine());
        StartCoroutine(AttackRoutine());
        StartCoroutine(BackgroundMeteorShower());
    }

    private IEnumerator BackgroundMeteorShower()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(3f, 5f));

            int numberOfMeteors = Random.Range(3, 6); // Spawn between 3 to 5 meteors
            for (int i = 0; i < numberOfMeteors; i++)
            {
                SpawnSingleMeteor();
                yield return new WaitForSeconds(0.2f); // Delay between each meteor spawn
            }
        }
    }

    private void SpawnSingleMeteor()
    {
        Vector2 spawnPos = (Vector2)playerTarget.position + Random.insideUnitCircle * 10f;
        spawnPos.y += bossData.meteorSpawnHeight;
        
        GameObject meteor = Instantiate(bossData.meteorPrefab, spawnPos, Quaternion.identity);
        Meteor meteorScript = meteor.GetComponent<Meteor>();
        meteorScript.Initialize(bossData.baseMeteorFallSpeed, bossData.baseMeteorDamage, bossData.baseMeteorImpactRadius);
    }

    private IEnumerator AttackRoutine()
    {
        while(true)
        {
            yield return new WaitForSeconds(bossData.attackCooldown);
            if(!isActionInProgress) StartCoroutine(PerformAttack());
        }
    }

    private IEnumerator PerformAttack()
    {
        isActionInProgress = true;
        Vector3 originalPosition = transform.position;

        // 1. Pre-Teleport at original position
        FacePlayerDirection();
        bossAnimator.SetTrigger(bossData.preTeleportTrigger);
        yield return new WaitForSeconds(1.1f); // Match PreTeleport animation length

        // 2. Instant move to player position
        Vector3 attackPosition = playerTarget.position;
        transform.position = attackPosition;

        // 3. Play AttackAnim with jump-down animation
        bossAnimator.SetTrigger(bossData.attackTrigger);
        attackImpactTriggered = false;
        
        yield return new WaitForSeconds(1.25f);
        audioSource.PlayOneShot(bossData.attackAudio);
        // 4. Wait for attack impact event from animation
        yield return new WaitUntil(() => attackImpactTriggered);

        isActionInProgress = false;
    }

    // Animation Event called during strike frame
    public void OnAttackImpact()
    {
        Debug.Log("Attack Impact Triggered"); // Check if this logs
        ApplyAoEDamage();
        attackImpactTriggered = true;
        if (cameraShake == null) Debug.LogError("CameraShake reference is missing!");
        else cameraShake.ShakeCamera(0.25f);
    }

    private void ApplyAoEDamage()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, bossData.attackImpactRadius, bossData.playerLayer);
        foreach(Collider2D hit in hits)
        {
            if(hit.CompareTag("Player"))
            {
                hit.GetComponent<DamageSystem>()?.ApplyDamage(bossData.attackDamage, DamageType.Environmental, transform.position);
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
            yield return new WaitForSeconds(bossData.teleportCooldown);
            if (!isActionInProgress) StartCoroutine(PerformTeleport());
        }
    }

    private IEnumerator PerformTeleport()
    {
        isActionInProgress = true;
        originalPosition = transform.position;

        // Phase 1: Pre-teleport animation
        bossAnimator.SetTrigger(bossData.preTeleportTrigger);
        yield return new WaitForSeconds(0.5f);

        // Phase 2: Calculate escape position
        Vector2 escapeDirection = (originalPosition - playerTarget.position).normalized;
        Vector2 teleportPosition = originalPosition + 
            (Vector3)(escapeDirection * Random.Range(bossData.minTeleportDistance, bossData.maxTeleportDistance));

        // Phase 3: Teleport animation at new position
        transform.position = teleportPosition;
        bossAnimator.SetTrigger(bossData.teleportTrigger);
        yield return new WaitForSeconds(0.75f);

        isActionInProgress = false;
    }

    private IEnumerator BlackHoleSpawnCycle()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(bossData.minSpawnInterval, bossData.maxSpawnInterval));
            
            // Trigger spawn animation
            if (bossAnimator != null)
            {
                bossAnimator.SetTrigger(bossData.spawnAnimTrigger);
            }

            // Wait for animation to reach casting point
            yield return new WaitForSeconds(0.5f);

            SpawnBlackHole();
        }
    }

    private void SpawnBlackHole()
    {
        if (bossData.blackHolePrefab == null) return;

        Vector2 spawnPos = GetSpawnPositionAroundPlayer();
        Instantiate(bossData.blackHolePrefab, spawnPos, Quaternion.identity);
    }

    private Vector2 GetSpawnPositionAroundPlayer()
    {
        Vector2 randomDirection = Random.insideUnitCircle.normalized;
        return (Vector2)playerTarget.position + (randomDirection * bossData.spawnDistanceFromBoss);
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
    private void UpdateHealthUI()
    {
        // Optional: Add any boss-specific health update logic
    }

    private void ShowDeathUI()
    {
        // Optional: Add boss death UI effects
    }
}
