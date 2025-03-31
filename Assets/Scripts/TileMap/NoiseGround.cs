using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;

[CreateAssetMenu(fileName = "NoiseGroundTile", menuName = "2D/Tiles/Noise Ground Tile")]
public class NoiseGroundTile : RuleTile {
    [Header("Noise Settings")]
    public Sprite[] middleVariants;
    public float noiseScale = 0.1f;
    public int seed = 42;
    [Range(0, 1)]
    public float threshold = 0.5f; // When to use noise vs regular rules
    
    public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData) {
        // First get the normal rule-based tile
        base.GetTileData(position, tilemap, ref tileData);
        
        // Store the original sprite from rule matching
        Sprite originalSprite = tileData.sprite;
        
        // Only modify tiles that match our "middle" condition
        if (ShouldBeRandomMiddle(position, tilemap)) {
            // Generate deterministic noise value based on position and seed
            float noiseValue = GetNoise(position.x, position.y);
            
            // Map noise value to sprite index
            int spriteIndex = Mathf.FloorToInt(noiseValue * middleVariants.Length);
            spriteIndex = Mathf.Clamp(spriteIndex, 0, middleVariants.Length - 1);
            
            // Assign the selected middle variant
            tileData.sprite = middleVariants[spriteIndex];
        }
    }
    
    private float GetNoise(float x, float y) {
        // Deterministic noise based on position and seed
        return Mathf.PerlinNoise(
            (x + seed) * noiseScale, 
            (y + seed) * noiseScale
        );
    }
    
    private bool ShouldBeRandomMiddle(Vector3Int position, ITilemap tilemap) {
        // Count how many adjacent tiles are of the same type
        int sameNeighbors = 0;
        foreach (var neighbor in new Vector3Int[] {
            new Vector3Int(position.x+1, position.y, 0),
            new Vector3Int(position.x-1, position.y, 0),
            new Vector3Int(position.x, position.y+1, 0),
            new Vector3Int(position.x, position.y-1, 0),
            new Vector3Int(position.x+1, position.y+1, 0),
            new Vector3Int(position.x-1, position.y+1, 0),
            new Vector3Int(position.x+1, position.y-1, 0),
            new Vector3Int(position.x-1, position.y-1, 0)
        }) {
            if (tilemap.GetTile(neighbor) == this) {
                sameNeighbors++;
            }
        }
        
        // If surrounded by similar tiles (or nearly surrounded), 
        // this is likely a middle ground tile
        return sameNeighbors >= 7;
    }
}