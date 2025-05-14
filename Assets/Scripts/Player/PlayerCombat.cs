using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class PlayerCombat : MonoBehaviour
{
    [Header("Dependencies")]
    public DamageSystem damageSystem;
    public ImprovedCameraShake cameraShake;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private CooldownSystem cooldownSystem;
    [SerializeField] private DirectionManager directionManager;
    [SerializeField] private ElementalSword swordGlow;
    [SerializeField] private StatusEffectManager statusManager;
    [SerializeField] private PlayerData playerData;
    [SerializeField] private HealthSystem healthSystem;
    [SerializeField] private Transform attackOrigin;
    private int comboCount = 0;
    private float lastAttackTime = 0f;
    private bool canAttack = true;
    private Animator anim;
    private bool comboEnabled = false;
    private AudioSource audioSource;

    private void Awake()
    {
        playerData.attackOrigin = attackOrigin;
        healthSystem = GetComponent<HealthSystem>();
        healthSystem.Initialize(playerData.maxHealth);
        anim = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        swordGlow = GetComponentInChildren<ElementalSword>();
        statusManager = FindObjectOfType<StatusEffectManager>();

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        if (cooldownSystem == null)
        {
            cooldownSystem = gameObject.AddComponent<CooldownSystem>();
        }

        if (cameraShake == null)
        {
            cameraShake = FindObjectOfType<ImprovedCameraShake>();
        }

        if (playerData == null)
        {
            playerData = Resources.Load<PlayerData>("PlayerData");
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
        cooldownSystem.UpdateCooldown();
        canAttack = !cooldownSystem.IsOnCooldown("Attack");

        if (Time.time - lastAttackTime > playerData.comboWindow && comboCount > 0)
        {
            ResetCombo();
        }

        if (Input.GetKeyDown(KeyCode.C) && canAttack)
        {
            Attack();
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            CycleElement();
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            Debug.Log("Q pressed - Current Element: " + swordGlow.CurrentElement);
            if (!swordGlow.isOnCooldown && swordGlow.CurrentElement == ElementalSword.Element.Lightning)
            {
                Debug.Log("Attempting lightning strike...");
                UseElementalSkill();
            }
        }
    }

    private void CycleElement()
    {
        if (swordGlow == null) return;
        
        int nextElement = ((int)swordGlow.CurrentElement + 1) % 3;
        swordGlow.SetElement((ElementalSword.Element)nextElement);
    }

    private void UseElementalSkill()
    {
        if (swordGlow == null) return;
    
        swordGlow.UseElementalSkill();
    }

    void Attack()
    {
        if (comboCount == 1 && !comboEnabled)
            return;

        comboCount++;
        lastAttackTime = Time.time;

        if (comboCount == 1)
        {
            anim.Play("Attack1");
        }
        else if (comboCount == 2)
        {
            anim.Play("Attack2");
            cooldownSystem.StartCooldown(playerData.attackCooldown, "Attack");
            ResetCombo();
        }
    }

    public void EnableCombo()
    {
        comboEnabled = true;
    }

    public void PerformAttack()
    {
        if (directionManager == null || playerData.attackOrigin == null)
        {
            Debug.LogError("Required components not assigned!");
            return;
        }

        Vector2 attackDirection = directionManager.GetDirection();
        Vector2 attackStartPos = (Vector2)playerData.attackOrigin.position;

        HashSet<HealthSystem> hitEnemies = new HashSet<HealthSystem>();

        // Combined detection using circle and box casts
        DetectEnemies(attackStartPos, attackDirection, hitEnemies);

        bool hitAnyEnemy = ProcessHits(hitEnemies);
        
        // Always play hit effects, regardless of whether an enemy was hit
        ApplyHitEffects();
    }

    private void DetectEnemies(Vector2 origin, Vector2 direction, HashSet<HealthSystem> enemies)
    {
        LayerMask combinedLayer = playerData.enemyLayer | playerData.bossLayer;
        // Circle detection for close range
        Collider2D[] closeHits = Physics2D.OverlapCircleAll(origin, 0.5f, combinedLayer);
        foreach (Collider2D collider in closeHits)
        {
            TryAddEnemy(collider, enemies);
        }

        // Box cast for primary attack range
        RaycastHit2D[] boxHits = Physics2D.BoxCastAll(
            origin,
            new Vector2(0.8f, 1.2f),
            0f,
            direction,
            playerData.attackDistance,
            combinedLayer
        );
        foreach (RaycastHit2D hit in boxHits)
        {
            if (hit.collider != null)
            {
                TryAddEnemy(hit.collider, enemies);
            }
        }
    }

    private bool ProcessHits(HashSet<HealthSystem> enemies)
    {
        bool hitAny = false;
        foreach (HealthSystem enemyHealth in enemies)
        {
            if (enemyHealth != null)
            {
                int damage = (comboCount == 1) ? playerData.attackDamage1 : playerData.attackDamage2;
                enemyHealth.TakeDamage(damage, DamageType.Physical);
                
                // Apply elemental effects
                if (swordGlow != null)
                {
                    swordGlow.ApplyElementalEffect(enemyHealth.gameObject);
                    swordGlow.TriggerHitEffect();
                }

                if (playerData.hitEffectPrefab != null)
                {
                    Instantiate(playerData.hitEffectPrefab, enemyHealth.transform.position, Quaternion.identity);
                }

                hitAny = true;
            }
        }
        return hitAny;
    }

    private void ApplyHitEffects()
    {
        StartCoroutine(ApplyHitstop(playerData.hitstopDuration, playerData.hitstopTimeScale));
        PlayHitSound();
        
        if (cameraShake != null)
        {
            cameraShake.TriggerCameraShake(gameObject);
        }
    }

    private void TryAddEnemy(Collider2D collider, HashSet<HealthSystem> enemySet)
    {
        HealthSystem health = collider.GetComponent<HealthSystem>();
        if (health != null)
        {
            enemySet.Add(health);
        }
    }

    private void PlayHitSound()
    {
        if (audioSource == null) return;

        AudioClip clipToPlay = (comboCount == 1) ? playerData.hitSound1 : playerData.hitSound2;
        if (clipToPlay != null)
        {
            audioSource.pitch = Random.Range(0.95f, 1.05f);
            audioSource.PlayOneShot(clipToPlay);
        }
    }

    private IEnumerator ApplyHitstop(float duration, float timeScale)
    {
        float originalTimeScale = Time.timeScale;
        Time.timeScale = timeScale;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = originalTimeScale;
    }

    public void AttackComplete()
    {
        if (comboCount == 1)
        {
            canAttack = true;
        }
    }

    public void ComboComplete()
    {
        canAttack = true;
    }

    private void ResetCombo()
    {
        comboCount = 0;
        comboEnabled = false;
    }
}