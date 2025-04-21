using UnityEngine;

public class Whirlpool : MonoBehaviour
{
    [Header("Settings")]
    public float lifetime = 4f;
    public float damagePerSecond = 5f;
    public float stunDuration = 1.5f;

    private float pullRadius;
    private float pullForce;
    private ParticleSystem vortexVFX;

    void Awake()
    {
        vortexVFX = GetComponent<ParticleSystem>();
        Destroy(gameObject, lifetime); // Auto-destruct after duration
    }

    public void Initialize(float radius, float force)
    {
        pullRadius = radius;
        pullForce = force;

        // Scale VFX to match radius
        if (vortexVFX != null)
        {
            var shape = vortexVFX.shape;
            shape.radius = pullRadius * 0.85f; // Visual slightly smaller than effect
        }
    }

    void Update()
    {
        ApplyVortexEffect();
    }

    void ApplyVortexEffect()
    {
        Collider2D[] targets = Physics2D.OverlapCircleAll(
            transform.position,
            pullRadius,
            LayerMask.GetMask("Enemies", "PhysicsObjects")
        );

        foreach (Collider2D target in targets)
        {
            // Pull objects toward center
            Vector2 pullDirection = (transform.position - target.transform.position).normalized;
            Rigidbody2D rb = target.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.AddForce(pullDirection * pullForce * Time.deltaTime, ForceMode2D.Force);
            }

            // Damage and stun enemies
            if (target.CompareTag("Enemy"))
            {
                DamageSystem damageSystem = target.GetComponent<DamageSystem>();
                if (damageSystem != null)
                {
                    damageSystem.ApplyDamage(
                        Mathf.CeilToInt(damagePerSecond * Time.deltaTime),
                        DamageType.Water
                    );

                    // Stun if in direct center (20% of radius)
                    float distance = Vector2.Distance(transform.position, target.transform.position);
                    if (distance < pullRadius * 0.2f)
                    {
                        damageSystem.ApplyStun(stunDuration);
                    }
                }
            }
        }
    }

    // Visualize effect radius in editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 0.5f, 1f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, pullRadius);
    }
}