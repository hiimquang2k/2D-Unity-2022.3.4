using UnityEngine;
using System.Collections;

public class PlayerCombat : MonoBehaviour
{
    public DamageSystem damageSystem;

    [Header("Attack Settings")]
    [SerializeField] private int attackDamage1 = 20;
    [SerializeField] private int attackDamage2 = 30;
    [SerializeField] private float attackDistance = 1.5f;
    [SerializeField] private float attackCooldown = 0.5f;
    [SerializeField] private float comboWindow = 0.8f;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private Transform attackOrigin;

    [Header("References")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private CooldownSystem cooldownSystem;
    [SerializeField] private DirectionManager directionManager;

    [Header("Hitstop Settings")]
    [SerializeField] private float hitstopDuration = 0.1f;
    [SerializeField] private float hitstopTimeScale = 0.05f;

    [Header("Effects")]
    [SerializeField] private GameObject hitEffectPrefab;
    [SerializeField] private AudioClip hitSound1;
    [SerializeField] private AudioClip hitSound2;
    
    private int comboCount = 0;
    private float lastAttackTime = 0f;
    private bool canAttack = true;
    private Animator anim;
    private bool comboEnabled = false;
    private AudioSource audioSource;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        if (cooldownSystem == null)
        {
            cooldownSystem = gameObject.AddComponent<CooldownSystem>();
        }
    }

    private void Start()
    {
        if (playerMovement == null)
            playerMovement = GetComponent<PlayerMovement>();
        if (directionManager == null)
            directionManager = GetComponent<DirectionManager>();
    }

    void Update()
    {
        // Manage cooldown
        cooldownSystem.UpdateCooldown();
        canAttack = !cooldownSystem.IsOnCooldown("Attack");

        // Reset combo if window expires
        if (Time.time - lastAttackTime > comboWindow && comboCount > 0)
        {
            ResetCombo();
        }

        // Check for attack input
        if (Input.GetKeyDown(KeyCode.C) && canAttack)
        {
            Attack();
        }
    }

    void Attack()
    {
        // Can only perform the next combo attack during the window
        if (comboCount == 1 && !comboEnabled)
            return;

        comboCount++;
        lastAttackTime = Time.time;

        if (comboCount == 1)
        {
            anim.Play("Attack1");
            // First attack initiated, wait for animation event to enable combo
        }
        else if (comboCount == 2)
        {
            anim.Play("Attack2");
            cooldownSystem.StartCooldown(attackCooldown, "Attack");
            ResetCombo();
        }
    }

    // Called by animation event in Attack1 animation
    public void EnableCombo()
    {
        comboEnabled = true;
    }

    // Called by animation event in both animations when attack point is reached
    public void PerformAttack()
    {
        if (directionManager == null)
        {
            Debug.LogError("DirectionManager is not assigned!");
            return;
        }

        if (attackOrigin == null)
        {
            Debug.LogError("AttackOrigin is not assigned!");
            return;
        }

        Vector2 direction = directionManager.GetDirection();
        RaycastHit2D hit = Physics2D.Raycast(attackOrigin.position, direction, attackDistance);

        Debug.DrawRay(attackOrigin.position, direction * attackDistance, Color.red, 0.2f);

        if (hit.collider != null && hit.collider.CompareTag("Monster"))
        {
            Debug.Log("Hit enemy: " + hit.collider.name);

            // Get hit position for VFX
            Vector2 hitPosition = hit.point;

            HealthSystem enemyHealth = hit.collider.GetComponent<HealthSystem>();
            if (enemyHealth != null)
            {
                // Apply different damage based on which attack in the combo
                int damageToApply = (comboCount == 1) ? attackDamage1 : attackDamage2;
                enemyHealth.TakeDamage(damageToApply, DamageType.Physical);
                
                // Apply hitstop
                StartCoroutine(ApplyHitstop(hitstopDuration, hitstopTimeScale));
                
                // Play hit VFX at contact point
                if (hitEffectPrefab != null)
                {
                    Instantiate(hitEffectPrefab, hitPosition, Quaternion.identity);
                }
                
                // Play hit sound
                PlayHitSound();
            }
        }
    }

    // Play appropriate hit sound based on combo count
    private void PlayHitSound()
    {
        if (audioSource != null)
        {
            AudioClip clipToPlay = (comboCount == 1) ? hitSound1 : hitSound2;
            
            if (clipToPlay != null)
            {
                audioSource.pitch = Random.Range(0.95f, 1.05f); // Slight variation
                audioSource.PlayOneShot(clipToPlay);
            }
        }
    }

    // Hitstop coroutine
    private IEnumerator ApplyHitstop(float duration, float timeScale)
    {
        // Store original time scale
        float originalTimeScale = Time.timeScale;
        
        // Apply hitstop (slow time)
        Time.timeScale = timeScale;
        
        // Wait for the duration (in real time, not game time)
        yield return new WaitForSecondsRealtime(duration);
        
        // Restore original time scale
        Time.timeScale = originalTimeScale;
    }

    // Called by animation event at the end of Attack1
    public void AttackComplete()
    {
        // If we didn't start another attack, reset after a short delay
        if (comboCount == 1)
        {
            canAttack = true;
        }
    }

    // Called by animation event at the end of Attack2
    public void ComboComplete()
    {
        canAttack = true;
    }

    private void ResetCombo()
    {
        comboCount = 0;
        comboEnabled = false;
    }

    // For debugging in editor
    private void OnDrawGizmosSelected()
    {
        if (attackOrigin != null && directionManager != null)
        {
            Gizmos.color = Color.red;
            Vector2 direction = directionManager.GetDirection();
            Gizmos.DrawLine(attackOrigin.position, attackOrigin.position + (Vector3)(direction * attackDistance));
        }
    }
}