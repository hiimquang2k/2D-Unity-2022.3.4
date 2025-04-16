using UnityEngine;
using UnityEngine.Tilemaps;

public class Meteor : MonoBehaviour
{
    [Header("Impact Settings")]
    [SerializeField] private GameObject impactEffect;
    [SerializeField] private LayerMask collisionLayers;

    private float speed;
    private int damage;
    private float impactRadius;
    private float destructionWidth;
    private float destructionChance;
    private Tilemap targetTilemap;
    private NonOverlappingParallax parallaxSystem;

    public void Initialize(float fallSpeed, int damage, float radius, float width, 
                         float chance, Tilemap tilemap, NonOverlappingParallax parallax)
    {
        speed = fallSpeed;
        this.damage = damage;
        impactRadius = radius;
        destructionWidth = width;
        destructionChance = chance;
        targetTilemap = tilemap;
        parallaxSystem = parallax;
    }

    private void Update()
    {
        transform.Translate(Vector2.down * speed * Time.deltaTime);
        
        if (transform.position.y < -10f)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if ((collisionLayers.value & (1 << collision.gameObject.layer)) != 0)
        {
            Impact();
        }
    }

    private void Impact()
    {
        ApplyDamage();
        DestroyEnvironment();
        SpawnEffects();
        Destroy(gameObject);
    }

    private void ApplyDamage()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, impactRadius);
        foreach (Collider2D hit in hits)
        {
            if (hit.TryGetComponent<DamageSystem>(out var damageSystem))
            {
                damageSystem.ApplyDamage(
                    damage,
                    DamageType.Environmental,
                    transform.position
                );
            }
        }
    }

    private void DestroyEnvironment()
    {
        DestroyTiles();
        DestroyBackground();
    }

    private void DestroyTiles()
    {
        if (targetTilemap == null) return;

        Vector3Int cellPosition = targetTilemap.WorldToCell(transform.position);
        int radius = Mathf.CeilToInt(impactRadius);
        int width = Mathf.CeilToInt(destructionWidth);

        for (int x = -width; x <= width; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                if (Random.value < destructionChance)
                {
                    Vector3Int tilePos = new Vector3Int(
                        cellPosition.x + x,
                        cellPosition.y + y,
                        cellPosition.z
                    );
                    targetTilemap.SetTile(tilePos, null);
                }
            }
        }
    }

    private void DestroyBackground()
    {
        if (parallaxSystem == null) return;

        float centerX = transform.position.x;
        float left = centerX - destructionWidth / 2;
        float right = centerX + destructionWidth / 2;
        
        parallaxSystem.DestroyInArea(left, right, destructionChance);
    }

    private void SpawnEffects()
    {
        if (impactEffect != null)
        {
            Instantiate(impactEffect, transform.position, Quaternion.identity);
        }
    }
}
