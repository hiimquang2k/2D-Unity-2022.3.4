using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways] // Works in both edit mode and play mode
public class GridSnapCollider : MonoBehaviour
{
    public float gridSize = 1f; // Match this to your grid size
    
    void Update()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            SnapToGrid();
        }
#endif
    }
    
    void SnapToGrid()
    {
        PolygonCollider2D collider = GetComponent<PolygonCollider2D>();
        if (collider == null) return;
        
        Vector2[] points = collider.points;
        bool changed = false;
        
        for (int i = 0; i < points.Length; i++)
        {
            Vector2 snappedPoint = new Vector2(
                Mathf.Round(points[i].x / gridSize) * gridSize,
                Mathf.Round(points[i].y / gridSize) * gridSize
            );
            
            if (points[i] != snappedPoint)
            {
                points[i] = snappedPoint;
                changed = true;
            }
        }
        
        if (changed)
        {
            collider.SetPath(0, points);
#if UNITY_EDITOR
            EditorUtility.SetDirty(this); // Mark as changed
#endif
        }
    }
}