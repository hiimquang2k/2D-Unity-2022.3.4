using UnityEngine;
using UnityEngine.VFX;
using System.Collections;

[RequireComponent(typeof(Renderer))]
public class ElementalSword : MonoBehaviour
{
    public enum Element { None, Fire, Lightning }
    public Element CurrentElement { get; private set; } = Element.None;

    [Header("Visual Settings")]
    [SerializeField, ColorUsage(true, true)] private Color fireColor = new Color(2f, 0.8f, 0.2f);
    [SerializeField, ColorUsage(true, true)] private Color lightningColor = new Color(0.4f, 1.6f, 2f);
    [SerializeField] private float maxIntensity = 5f;
    [SerializeField] private string glowColorProperty = "_GlowColor";
    [SerializeField] private string glowIntensityProperty = "_GlowIntensity";

    [Header("Fire Settings")]
    [SerializeField] private float fireStatusDuration = 3f;
    [SerializeField] private GameObject fireExplosionPrefab;
    [SerializeField] private int fireExplosionDamage = 20;

    [Header("Lightning Settings")] 
    [SerializeField] private float lightningStatusDuration = 2f;
    [SerializeField] private GameObject lightningStrikePrefab;
    [SerializeField] private int lightningStrikeDamage = 30;
    [SerializeField] private float chainRadius = 3f;
    [SerializeField] private int maxChainJumps = 2;
    [SerializeField] private LayerMask enemyLayer;

    [Header("Cooldown")]
    [SerializeField] private float skillCooldown = 5f;

    private Material swordMaterial;
    private StatusEffectManager statusManager;
    private float cooldownTimer;
    public bool isOnCooldown;

    private void Awake()
    {
        swordMaterial = GetComponent<Renderer>().material;
        statusManager = FindObjectOfType<StatusEffectManager>();
        SetElement(Element.None);
        if (lightningStrikePrefab == null)
        {
            Debug.LogError("LightningStrikePrefab is not assigned in ElementalSword. Please assign a prefab with ParticleSystem + AudioSource.");
        }
        if (fireExplosionPrefab == null)
        {
            Debug.LogError("FireExplosionPrefab is not assigned in ElementalSword. Please assign a prefab with ParticleSystem + AudioSource.");
        }
    }

    private void Update()
    {
        if (isOnCooldown)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0) isOnCooldown = false;
        }
    }

    public void SetElement(Element newElement)
    {
        CurrentElement = newElement;
        
        switch (CurrentElement)
        {
            case Element.Fire:
                SetGlowProperties(fireColor, maxIntensity);
                break;
            case Element.Lightning:
                SetGlowProperties(lightningColor, maxIntensity);
                break;
            default:
                SetGlowProperties(Color.black, 0f);
                break;
        }
    }

    private void SetGlowProperties(Color color, float intensity)
    {
        swordMaterial.SetColor(glowColorProperty, color);
        swordMaterial.SetFloat(glowIntensityProperty, intensity);
    }

    public void ApplyElementalEffect(GameObject enemy)
    {
        if (CurrentElement == Element.None || enemy == null) return;

        switch (CurrentElement)
        {
            case Element.Fire:
                statusManager?.ApplyStatus(enemy, DamageType.Fire);
                ApplyFireDoT(enemy);
                break;
            case Element.Lightning:
                statusManager?.ApplyStatus(enemy, DamageType.Lightning);
                break;
        }
    }

    private void ApplyFireDoT(GameObject enemy)
    {
        var dot = new DoTEffect()
        {
            type = DamageType.Fire,
            damagePerTick = 5f,
            tickInterval = 0.5f,
            duration = fireStatusDuration
        };

        var system = enemy.GetComponent<DoTSystem>() ?? enemy.AddComponent<DoTSystem>();
        system.ApplyDoT(dot);
    }

    public void UseElementalSkill()
    {
        if (isOnCooldown || CurrentElement == Element.None) return;

        switch (CurrentElement)
        {
            case Element.Fire:
                TriggerFireExplosion();
                break;
            case Element.Lightning:
                TriggerLightningStrike();
                break;
        }

        isOnCooldown = true;
        cooldownTimer = skillCooldown;
    }

    private void TriggerFireExplosion()
    {
        GameObject[] burningEnemies = GameObject.FindGameObjectsWithTag("Burning");
        foreach (var enemy in burningEnemies)
        {
            Instantiate(fireExplosionPrefab, enemy.transform.position, Quaternion.identity);
            
            var health = enemy.GetComponent<HealthSystem>();
            if (health) health.TakeDamage(fireExplosionDamage, DamageType.Fire);
            
            statusManager?.RemoveStatus(enemy);
        }
    }

    private void TriggerLightningStrike()
    {
        GameObject target = FindClosestTaggedEnemy("Electrified");
        if (target != null)
        {
            StartCoroutine(ExecuteLightningStrike(target, lightningStrikeDamage, maxChainJumps));
        }
    }

    private IEnumerator ExecuteLightningStrike(GameObject target, int damage, int remainingJumps)
{
    if (lightningStrikePrefab == null || target == null) yield break;

    // Instantiate VFX
    GameObject vfxInstance = Instantiate(
        lightningStrikePrefab,
        target.transform.position + new Vector3(0, 5f, 0),
        Quaternion.Euler(-90f, 0f, 0f)
    );

    // Get VisualEffect component
    VisualEffect vfx = vfxInstance.GetComponent<VisualEffect>();
    AudioSource audioSource = vfxInstance.GetComponent<AudioSource>();

    // Play effects
    if (vfx != null) 
    {
        vfx.Play();
        //vfx.SetVector3("TargetPosition", target.transform.position);
    }

    if (audioSource != null && audioSource.clip != null)
    {
        audioSource.Play();
    }

    // Apply damage
    HealthSystem health = target.GetComponent<HealthSystem>();
    if (health != null)
    {
        health.TakeDamage(damage, DamageType.Lightning);
    }

    // Chain lightning logic
    if (remainingJumps > 0)
    {
        yield return new WaitForSeconds(0.3f); // Delay between jumps
        
        Collider2D[] enemies = Physics2D.OverlapCircleAll(
            target.transform.position,
            chainRadius,
            enemyLayer
        );

        foreach (var enemy in enemies)
        {
            if (enemy.CompareTag("Electrified") && enemy.gameObject != target)
            {
                StartCoroutine(ExecuteLightningStrike(
                    enemy.gameObject,
                    Mathf.RoundToInt(damage * 0.7f), // Reduce damage with each jump
                    remainingJumps - 1
                ));
                yield break; // Only chain to one enemy at a time
            }
        }
    }

    // Wait for VFX to complete before destroying
    yield return new WaitForSeconds(GetVFXDuration(vfx));
    Destroy(vfxInstance);
}
private float GetVFXDuration(VisualEffect vfx)
{
    if (vfx == null) return 1f; // Default duration
    
    // Check if VFX has a duration parameter
    if (vfx.HasFloat("Duration"))
    {
        return vfx.GetFloat("Duration");
    }
    return 2f; // Fallback duration
}
    private GameObject FindClosestTaggedEnemy(string tag)
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(tag);
        if (enemies.Length == 0) return null;

        GameObject closest = null;
        float minDistance = Mathf.Infinity;

        foreach (var enemy in enemies)
        {
            float distance = Vector2.Distance(transform.position, enemy.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closest = enemy;
            }
        }

        return closest;
    }

    public void TriggerHitEffect(float multiplier = 2f, float duration = 0.15f)
    {
        StartCoroutine(HitEffectRoutine(multiplier, duration));
    }

    private IEnumerator HitEffectRoutine(float multiplier, float duration)
    {
        float original = swordMaterial.GetFloat(glowIntensityProperty);
        float target = original * multiplier;

        // Ramp up
        float timer = 0;
        while (timer < duration/2)
        {
            timer += Time.deltaTime;
            swordMaterial.SetFloat(glowIntensityProperty, Mathf.Lerp(original, target, timer/(duration/2)));
            yield return null;
        }

        // Ramp down
        timer = 0;
        while (timer < duration/2)
        {
            timer += Time.deltaTime;
            swordMaterial.SetFloat(glowIntensityProperty, Mathf.Lerp(target, original, timer/(duration/2)));
            yield return null;
        }

        swordMaterial.SetFloat(glowIntensityProperty, original);
    }
    public bool IsSkillReady()
{
    return !isOnCooldown && CurrentElement != Element.None;
}
}