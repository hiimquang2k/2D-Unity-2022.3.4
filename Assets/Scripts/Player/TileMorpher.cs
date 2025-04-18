// TileMorpher.cs
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileMorpher : MonoBehaviour {
    [Header("References")]
    public ElementRule currentElement;
    public Tilemap groundTilemap;
    public Tilemap destructibleTilemap;

    [Header("VFX")]
    public ParticleSystem morphParticles;

    void Update() {
        if (Input.GetKeyDown(KeyCode.Q)) {
            MorphTiles();
        }
    }

    public void MorphTiles() {
        Vector3Int playerPos = groundTilemap.WorldToCell(transform.position);
        BoundsInt area = new BoundsInt(
            playerPos - Vector3Int.one * (int)currentElement.morphRadius,
            Vector3Int.one * (int)(currentElement.morphRadius * 2)
        );

        Tilemap[] allTilemaps = { groundTilemap, destructibleTilemap };

        foreach (Tilemap tilemap in allTilemaps) {
            foreach (Vector3Int cell in area.allPositionsWithin) {
                if (!tilemap.HasTile(cell)) continue;
                
                MorphableTile tile = tilemap.GetTile<MorphableTile>(cell);
                if (tile == null) continue;

                if (IsTileAffected(tile) && HasLineOfSight(cell)) {
                    tilemap.SetTile(cell, currentElement.resultTile);
                    PlayEffects(cell);
                }
            }
        }
    }

    bool IsTileAffected(MorphableRuleTile tile) {
        switch (currentElement.element) {
            case Element.Fire: return tile.isFlammable;
            case Element.Earth: return tile.isMorphableByEarth;
            default: return false;
        }
    }

    bool HasLineOfSight(Vector3Int cell) {
        Vector3 worldPos = groundTilemap.CellToWorld(cell);
        return !Physics2D.Linecast(transform.position, worldPos, LayerMask.GetMask("Walls"));
    }

    void PlayEffects(Vector3Int cell) {
        Vector3 worldPos = groundTilemap.CellToWorld(cell) + groundTilemap.cellSize/2;
        Instantiate(morphParticles, worldPos, Quaternion.identity);
    }
}