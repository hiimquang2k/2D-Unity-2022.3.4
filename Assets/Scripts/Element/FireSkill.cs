using UnityEngine;
using System.Collections;
using UnityEngine.Tilemaps;

public class FireSkill : MonoBehaviour
{
    [Header("Respawn Settings")]
    public float respawnDuration = 3f;
    public ParticleSystem flameTrail;
    public AudioClip igniteSFX;

    [Header("Tile Burning")]
    public float burnRadius = 2f;
    public LayerMask burnableLayers;
    public GameObject fireIgniteVFX;
    public int burnDamage = 5;

    private HealthSystem healthSystem;

    void Awake()
    {
        healthSystem = GetComponent<HealthSystem>();
    }

    public void Activate()
    {
        StartCoroutine(RespawnRoutine());
        BurnNearbyTiles();
    }

    IEnumerator RespawnRoutine()
    {
        Vector3 respawnPoint = transform.position;
        healthSystem.isInvulnerable = true;
        
        // Visual/Audio
        flameTrail.Play();
        AudioSource.PlayClipAtPoint(igniteSFX, transform.position);

        yield return new WaitForSeconds(respawnDuration);
        healthSystem.isInvulnerable = false;
    }

    void BurnNearbyTiles()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, burnRadius, burnableLayers);
        foreach (Collider2D hit in hits)
        {
            // Burn tiles
            BurnableTilemap btm = hit.GetComponent<BurnableTilemap>();
            if (btm != null)
            {
                Vector2 hitPoint = hit.ClosestPoint(transform.position);
                btm.IgniteTile(hitPoint);
                Instantiate(fireIgniteVFX, hitPoint, Quaternion.identity);
            }

            // Damage burnable enemies
            DamageSystem enemyDamage = hit.GetComponent<DamageSystem>();
            if (enemyDamage != null && hit.gameObject != gameObject)
            {
                enemyDamage.ApplyDamage(burnDamage, DamageType.Fire);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1, 0.5f, 0, 0.4f);
        Gizmos.DrawWireSphere(transform.position, burnRadius);
    }
}