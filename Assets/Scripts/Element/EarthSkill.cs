using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;

public class EarthSkill : MonoBehaviour
{
    [Header("Tilemaps")]
    public Tilemap groundTilemap;
    public Tilemap pillarTilemap;

    [Header("Tiles")]
    public RuleTile pillarTile;
    public TileBase fragileTile;

    [Header("Settings")]
    public float pillarDuration = 5f;
    public float stompRadius = 3f;

    // Called by ElementManager
    public void Activate()
    {
        Vector3Int playerCell = groundTilemap.WorldToCell(transform.position);
        SpawnPillar(playerCell);
        QuakeStomp(playerCell);
    }

    void SpawnPillar(Vector3Int cell)
    {
        if (groundTilemap.HasTile(cell))
        {
            // Replace ground with pillar
            pillarTilemap.SetTile(cell, pillarTile);
            StartCoroutine(RemovePillarAfterDelay(cell));
        }
    }

    IEnumerator RemovePillarAfterDelay(Vector3Int cell)
    {
        yield return new WaitForSeconds(pillarDuration);
        pillarTilemap.SetTile(cell, null);
    }

    void QuakeStomp(Vector3Int centerCell)
    {
        // Destroy fragile tiles in radius
        for (int x = -2; x <= 2; x++)
        {
            for (int y = -2; y <= 2; y++)
            {
                Vector3Int cell = centerCell + new Vector3Int(x, y, 0);
                if (groundTilemap.GetTile(cell) == fragileTile)
                {
                    groundTilemap.SetTile(cell, null); // Remove tile
                }
            }
        }

        // Stun enemies (pseudocode)
        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, stompRadius);
        foreach (Collider2D enemy in enemies)
        {
            enemy.GetComponent<Enemy>().Stun(2f);
        }
    }
}