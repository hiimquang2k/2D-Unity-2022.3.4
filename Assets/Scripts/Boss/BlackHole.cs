using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BlackHole : MonoBehaviour
{
    [Header("Spawn Sequence")]
    [SerializeField] private GameObject indicatorPrefab;
    [SerializeField] private float spawnDelay = 2f;
    [SerializeField] private float flickerInterval = 0.1f;
    [SerializeField] private float spawnUptime = 5f;
     
    [Header("Pull Settings")]
    [SerializeField] private float pullForce = 10f;
    [SerializeField] private float pullRadius = 5f;
    
    [Header("Damage Settings")]
    [SerializeField] private float damageRadius = 1f;
    [SerializeField] private int damagePerTick = 10;
    [SerializeField] private float tickInterval = 0.5f;

    [Header("SFX")]
    [SerializeField] private AudioClip warningSound;
    [SerializeField] private AudioClip activationSound;

    [Header("Distortion Effects")]
    [SerializeField] private float distortionUpdateInterval = 0.1f;

    private float initialPullRadius;
    private float initialDamageRadius;
    private Vector3 initialScale;
    private List<SpriteRenderer> distortionRenderers = new List<SpriteRenderer>();
    private List<Behaviour> distortionComponents = new List<Behaviour>();
    private GameObject activeIndicator;
    private HashSet<DamageSystem> affectedPlayers = new HashSet<DamageSystem>();
    private Coroutine damageCoroutine;
    private Collider2D blackHoleCollider;
    private SpriteRenderer spriteRenderer;
    private bool isActive;

    private void Awake()
    {
        blackHoleCollider = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        GetChildDistortionComponents();
        SetActiveState(false);

        // Store initial values for shrinking
        initialPullRadius = pullRadius;
        initialDamageRadius = damageRadius;
        initialScale = transform.localScale;
    }

    private void Start()
    {
        StartCoroutine(SpawnSequence());
    }

    private void Update()
    {
        if (!isActive) return;

        // Moved physics/damage logic here
        HandleBlackHoleEffects();
    }

    private void HandleBlackHoleEffects()
    {
        Collider2D[] nearbyObjects = Physics2D.OverlapCircleAll(transform.position, pullRadius);
        foreach (Collider2D obj in nearbyObjects)
        {
            if (!obj.CompareTag("Player")) continue;

            // Physics pull
            Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 direction = (transform.position - obj.transform.position).normalized;
                rb.AddForce(direction * pullForce, ForceMode2D.Force);
            }

            // Damage handling
            HandleDamage(obj);
        }

        affectedPlayers.RemoveWhere(x => x == null);
    }

    private void HandleDamage(Collider2D obj)
    {
        DamageSystem damageSystem = obj.GetComponent<DamageSystem>();
        if (damageSystem != null)
        {
            float distance = Vector2.Distance(transform.position, obj.transform.position);
            bool inDamageZone = distance < damageRadius;

            if (inDamageZone && !affectedPlayers.Contains(damageSystem))
            {
                affectedPlayers.Add(damageSystem);
                damageSystem.SetKnockbackable(false);
                // Apply initial damage
                damageSystem.ApplyDamage(damagePerTick, DamageType.DoT, (Vector2)transform.position);
                // Start DoT effect
                DoTEffect dotEffect = new DoTEffect
                {
                    type = DamageType.DoT,
                    damagePerTick = damagePerTick,
                    tickInterval = tickInterval,
                    duration = spawnUptime,
                    timeRemaining = spawnUptime,
                    nextTickTime = Time.time + tickInterval
                };
                damageSystem.ApplyDoT(dotEffect);
            }
            else if (!inDamageZone && affectedPlayers.Contains(damageSystem))
            {
                affectedPlayers.Remove(damageSystem);
                damageSystem.SetKnockbackable(true);
            }
        }
    }
    private void GetChildDistortionComponents()
    {
        distortionRenderers.Clear();
        distortionComponents.Clear();

        foreach (Transform child in transform)
        {
            // Get SpriteRenderers for visual effects
            var sr = child.GetComponent<SpriteRenderer>();
            if (sr != null) distortionRenderers.Add(sr);

            // Get other components excluding Transform
            var components = child.GetComponents<Behaviour>();
            foreach (var comp in components)
            {
                // Remove Transform check since Behaviour components don't include Transform
                distortionComponents.Add(comp);
            }
        }
    }
        private IEnumerator UpdateDistortionEffects()
    {
        while (isActive)
        {
            // Update distortion effects relative to main hole
            foreach (var renderer in distortionRenderers)
            {
                if (renderer != null)
                {
                    // Example: Match main renderer properties
                    renderer.color = spriteRenderer.color;
                    renderer.sortingOrder = spriteRenderer.sortingOrder + 1;
                }
            }
            yield return new WaitForSeconds(distortionUpdateInterval);
        }
    }

    private IEnumerator SpawnSequence()
    {
        isActive = false;
        SetActiveState(false);

        if (indicatorPrefab == null)
        {
            Debug.LogError("Missing indicator prefab!");
            yield break;
        }

        // Create and initialize indicator
        activeIndicator = Instantiate(indicatorPrefab, transform.position, Quaternion.identity);
        SpriteRenderer indicatorRenderer = activeIndicator.GetComponent<SpriteRenderer>();
        
        if (indicatorRenderer == null)
        {
            Debug.LogError("Indicator prefab missing SpriteRenderer!");
            yield break;
        }

        // Start flicker effect
        Coroutine flickerRoutine = StartCoroutine(FlickerEffect(indicatorRenderer));
        
        // Play warning sound
        if (warningSound != null)
        {
            Debug.Log($"Playing warning sound: {warningSound.name}");
            AudioSource.PlayClipAtPoint(warningSound, transform.position, 3.0f);
        }
        else
        {
            Debug.LogWarning("Warning sound is not assigned in the inspector!");
        }

        // Wait for spawn delay
        yield return new WaitForSeconds(spawnDelay);

        // Cleanup
        StopCoroutine(flickerRoutine);
        indicatorRenderer.enabled = true;
        Destroy(activeIndicator);

        // Activate black hole
        if (activationSound != null)
        {
            Debug.Log($"Playing activation sound: {activationSound.name}");
            AudioSource.PlayClipAtPoint(activationSound, transform.position);
        }
        else
        {
            Debug.LogWarning("Activation sound is not assigned in the inspector!");
        }
        SetActiveState(true);

        // Start shrinking effect and wait for it to complete
        yield return StartCoroutine(ShrinkOverTime(spawnUptime));
        Destroy(gameObject);
    }
        
    private IEnumerator FlickerEffect(SpriteRenderer renderer)
    {
        while (true)
        {
            renderer.enabled = !renderer.enabled;
            yield return new WaitForSeconds(flickerInterval);
        }
    }
    private IEnumerator ShrinkOverTime(float duration)
    {
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float progress = timer / duration;
            float scaleFactor = 1 - progress;

            // Update visual scale
            transform.localScale = initialScale * scaleFactor;
            
            // Update functional radii
            pullRadius = initialPullRadius * scaleFactor;
            damageRadius = initialDamageRadius * scaleFactor;

            yield return null;
        }

        // Ensure final state
        transform.localScale = Vector3.zero;
        pullRadius = 0f;
        damageRadius = 0f;
    }
    private void SetActiveState(bool isActive)
    {
        this.isActive = isActive;
        blackHoleCollider.enabled = isActive;
        spriteRenderer.enabled = isActive;

        foreach (var renderer in distortionRenderers)
        {
            if (renderer != null) renderer.enabled = isActive;
        }
        
        foreach (var component in distortionComponents)
        {
            if (component != null) component.enabled = isActive;
        }
    }


    private void OnEnable()
    {
        damageCoroutine = StartCoroutine(ApplyDoTDamage());
    }

    private void OnDisable()
    {
        if (damageCoroutine != null)
        {
            StopCoroutine(damageCoroutine);
        }

        // Reset all affected players
        foreach (var player in affectedPlayers)
        {
            if (player != null)
            {
                player.SetKnockbackable(true);
            }
        }
        affectedPlayers.Clear();
    }

    private IEnumerator ApplyDoTDamage()
    {
        WaitForSeconds wait = new WaitForSeconds(tickInterval);

        while (true)
        {
            foreach (var player in affectedPlayers)
            {
                if (player != null)
                {
                    player.ApplyDamage(damagePerTick, DamageType.DoT);
                }
            }
            yield return wait;
        }
    }

    public float GetPullRadius() => pullRadius;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, pullRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, damageRadius);
    }
}