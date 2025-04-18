// BurnableTilemap.cs
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;

[RequireComponent(typeof(TilemapCollider2D))]
public class BurnableTilemap : MonoBehaviour 
{
    [Header("Tiles")]
    public TileBase normalTile; 
    public AnimatedTile fireTile;
    public TileBase burnedTile;

    [Header("Settings")]
    public float burnTime = 2f;
    public bool destroyWhenBurned = true;

    private Tilemap tilemap;

    void Awake() {
        tilemap = GetComponent<Tilemap>();
    }

    public void IgniteTile(Vector2 worldPos) {
        Vector3Int cell = tilemap.WorldToCell(worldPos);
        if (tilemap.GetTile(cell) == normalTile) {
            tilemap.SetTile(cell, fireTile);
            StartCoroutine(BurnTile(cell));
        }
    }

    IEnumerator BurnTile(Vector3Int cell) {
        yield return new WaitForSeconds(burnTime);
        tilemap.SetTile(cell, destroyWhenBurned ? null : burnedTile);
        if (destroyWhenBurned) tilemap.SetColliderType(cell, Tile.ColliderType.None);
    }
}