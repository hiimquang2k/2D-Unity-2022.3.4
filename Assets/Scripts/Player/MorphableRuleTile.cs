// MorphableRuleTile.cs
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "NewMorphableTile", menuName = "2D/Tiles/Morphable Rule Tile")]
public class MorphableRuleTile : RuleTile {
    [Header("Elemental Properties")]
    public bool isFlammable;      // Can be burned by Fire
    public bool isMeltable;       // Can be melted by Fire/frozen by Water
    public bool isConductive;     // Conducts electricity
    public bool isPetrifyable;    // Can be turned to stone by Earth
    public bool isCrumbling;      // Can be destroyed by Earth stomp
}