// TileMorphController.cs
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileMorphController : MonoBehaviour {
    [Header("References")]
    public Tilemap groundTilemap;
    public Tilemap destructibleTilemap;
    public MorphableRuleTile[] resultTiles; // Assign in inspector (Ash, Pillar, etc.)

    public void MorphTiles(Element activeElement) {
        Vector3Int playerCell = groundTilemap.WorldToCell(transform.position);
        int radius = GetElementRadius(activeElement);

        BoundsInt area = new BoundsInt(
            playerCell.x - radius, playerCell.y - radius, 0,
            radius * 2 + 1, radius * 2 + 1, 1
        );

        foreach (Vector3Int cell in area.allPositionsWithin) {
            MorphableRuleTile oldTile = groundTilemap.GetTile<MorphableRuleTile>(cell);
            if (oldTile == null) continue;

            MorphableRuleTile newTile = GetNewTile(oldTile, activeElement);
            if (newTile != null) {
                groundTilemap.SetTile(cell, newTile);
                UpdateColliders(cell);
            }
        }
    }
    int GetElementRadius(Element element) => element switch
    {
        Element.Fire => 2,
        Element.Water => 3,
        Element.Earth => 2,
        _ => 1
    };
    MorphableRuleTile GetNewTile(MorphableRuleTile oldTile, Element element) {
        switch (element) {
            case Element.Fire when oldTile.isFlammable:
                return resultTiles[0]; // Ash tile
            case Element.Earth when oldTile.isPetrifyable:
                return resultTiles[1]; // Pillar tile
            // Add other elements...
            default: return null;
        }
    }

    void UpdateColliders(Vector3Int cell) {
        groundTilemap.GetComponent<TilemapCollider2D>().ProcessTilemapChanges();
    }
}