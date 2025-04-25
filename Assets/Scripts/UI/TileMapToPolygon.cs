using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(PolygonCollider2D))]
public class TilemapToPolygonCollider : MonoBehaviour
{
    public Tilemap tilemap;
    private PolygonCollider2D polyCollider;

    private void Start()
    {
        polyCollider = GetComponent<PolygonCollider2D>();
        UpdateColliderFromTilemap();
    }

    public void UpdateColliderFromTilemap()
    {
        if (tilemap == null || polyCollider == null) return;

        // Get all occupied tile positions
        var bounds = tilemap.cellBounds;
        var edgePositions = new System.Collections.Generic.List<Vector2>();

        // Loop through all tiles to find edges
        for (int x = bounds.xMin; x <= bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y <= bounds.yMax; y++)
            {
                Vector3Int cellPos = new Vector3Int(x, y, 0);
                if (tilemap.HasTile(cellPos))
                {
                    // Add tile corners as potential collider points
                    Vector3 worldPos = tilemap.GetCellCenterWorld(cellPos);
                    float halfCell = tilemap.cellSize.x / 2f;

                    edgePositions.Add(new Vector2(worldPos.x - halfCell, worldPos.y - halfCell)); // Bottom-left
                    edgePositions.Add(new Vector2(worldPos.x + halfCell, worldPos.y - halfCell)); // Bottom-right
                    edgePositions.Add(new Vector2(worldPos.x + halfCell, worldPos.y + halfCell)); // Top-right
                    edgePositions.Add(new Vector2(worldPos.x - halfCell, worldPos.y + halfCell)); // Top-left
                }
            }
        }

        // Simplify the polygon (remove duplicate points)
        var simplifiedPath = SimplifyEdgePoints(edgePositions);
        polyCollider.SetPath(0, simplifiedPath);
    }

    private List<Vector2> SimplifyEdgePoints(List<Vector2> points)
    {
        // Implement your simplification algorithm here
        // For example: remove duplicates, use convex hull, etc.
        return points.Distinct().ToList();
    }
}