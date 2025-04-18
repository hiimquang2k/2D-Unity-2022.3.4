using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu]
public class ElementRule : ScriptableObject {
    public Element element;
    public MorphableRuleTile[] affectedTiles;
    public MorphableRuleTile resultTile;
    public float morphRadius = 3f;
    public bool requiresLineOfSight = true;
}