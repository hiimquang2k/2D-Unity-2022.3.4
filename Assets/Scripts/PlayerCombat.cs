using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    public DamageSystem damageSystem;
    public float attackDamage = 10f;

    [SerializeField] private int damage = 20;
    [SerializeField] private float attackDistance = 1.5f;
    [SerializeField] private float attackCooldown = 0.5f;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private Transform attackOrigin;

    private float cooldownTimer = 0f;
    private bool facingRight = true;

    // Method to perform a melee attack

    private void OnTriggerEnter(Collider other)
    {
        HealthSystem targetHealth = other.GetComponent<HealthSystem>();
        if (targetHealth != null)
        {
            // Apply damage to the target
            damageSystem.ApplyDamage((int)attackDamage, DamageType.Physical);
            Debug.Log(gameObject.name + " attacked " + targetHealth.gameObject.name + " for " + attackDamage + " damage.");
        }
    }

    void Update()
    {
        if (cooldownTimer > 0)
            cooldownTimer -= Time.deltaTime;
            
        if (Input.GetKeyDown(KeyCode.Space) && cooldownTimer <= 0)
        {
            PerformAttack();
            cooldownTimer = attackCooldown;
        }
    }
    
    void PerformAttack()
    {
        Vector2 direction = facingRight ? Vector2.right : Vector2.left;
        RaycastHit2D hit = Physics2D.Raycast(attackOrigin.position, direction, attackDistance, enemyLayer);
        
        Debug.DrawRay(attackOrigin.position, direction * attackDistance, Color.red, 0.2f);
        
        if (hit.collider != null)
        {
            // If we hit an enemy
            HealthSystem enemyHealth = hit.collider.GetComponent<HealthSystem>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damage, DamageType.Physical);
                Debug.Log("Hit enemy: " + hit.collider.name);
            }
        }
    }
}
