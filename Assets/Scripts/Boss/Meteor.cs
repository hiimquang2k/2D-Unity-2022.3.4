using UnityEngine;

public class Meteor : MonoBehaviour
{
    public float fallSpeed = 5f;
    public int damage = 10;
    public float impactRadius = 1f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private float destroyTimer = 3f;
    private TrailRenderer trail;
    void Start()
{
    trail = GetComponent<TrailRenderer>();
    // Optional: Randomize trail appearance
    trail.time = Random.Range(0.4f, 0.7f);
    trail.startWidth = 0.5f;
    trail.endWidth = 0f;
}
    public void Initialize(float speed, int dmg, float radius)
    {
        fallSpeed = speed;
        damage = dmg;
        impactRadius = radius;
        rb = GetComponent<Rigidbody2D>();
        rb.velocity = Vector2.down * fallSpeed;
    }

    private void Update()
    {
        // Decrease timer
        destroyTimer -= Time.deltaTime;
        
        // Destroy if timer runs out
        if (destroyTimer <= 0f)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if we hit the ground
        if(((1 << collision.gameObject.layer) & groundLayer) != 0)
        {
            // Store the ground position
            Vector2 groundPosition = collision.ClosestPoint(transform.position);
            
            // Move meteor to ground position
            transform.position = new Vector3(groundPosition.x, groundPosition.y, transform.position.z);
            
            // Apply area damage
            ApplyAreaDamage();
            
            // Destroy meteor
            Destroy(gameObject);
        }
    }

    private void ApplyAreaDamage()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, impactRadius);
        foreach(Collider2D hit in hits)
        {
            if(hit.CompareTag("Player"))
            {
                hit.GetComponent<DamageSystem>()?.ApplyDamage(damage, DamageType.Environmental, transform.position);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, impactRadius);
    }
}