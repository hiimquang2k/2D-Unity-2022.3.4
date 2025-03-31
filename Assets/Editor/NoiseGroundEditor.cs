using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;

#if UNITY_EDITOR
[CustomEditor(typeof(NoiseGroundTile))]
public class NoiseGroundTileEditor : RuleTileEditor
{
    private NoiseGroundTile NoiseTile { get { return target as NoiseGroundTile; } }
    private SerializedProperty noiseScaleProp;
    private SerializedProperty seedProp;
    private SerializedProperty middleVariantsProp;
    
    public override void OnEnable()
    {
        base.OnEnable();
        noiseScaleProp = serializedObject.FindProperty("noiseScale");
        seedProp = serializedObject.FindProperty("seed");
        middleVariantsProp = serializedObject.FindProperty("middleVariants");
    }
    
    public override void OnInspectorGUI()
    {
        // Update the serialized object
        serializedObject.Update();
        
        // Create a nice header for noise settings
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Noise Settings", EditorStyles.boldLabel);
        
        // Regular base rule tile properties (inherits from RuleTileEditor)
        base.OnInspectorGUI();
        
        // Noise specific properties
        EditorGUI.BeginChangeCheck();
        
        // Nice slider for noise scale
        EditorGUILayout.Slider(noiseScaleProp, 0.01f, 1f, new GUIContent("Noise Scale", "Controls the scale of the noise pattern. Lower values create larger patterns."));
        
        // Seed field with random button
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(seedProp);
        if(GUILayout.Button("Randomize", GUILayout.Width(80)))
        {
            seedProp.intValue = Random.Range(0, 99999);
            GUI.changed = true;
        }
        EditorGUILayout.EndHorizontal();
        
        // Preview texture generation
        if (EditorGUI.EndChangeCheck() || GUILayout.Button("Update Preview"))
        {
            // Force the tile to update
            serializedObject.ApplyModifiedProperties();
            GeneratePreview();
        }
        
        EditorGUILayout.Space();
        
        // Draw the preview texture
        DrawNoisePreview();
        
        // Middle variants section
        EditorGUILayout.PropertyField(middleVariantsProp, true);
        
        // Apply any changes to the serialized object
        serializedObject.ApplyModifiedProperties();
    }
    
    private Texture2D previewTexture;
    private void GeneratePreview()
    {
        // Create a preview texture to show the noise pattern
        if (previewTexture == null || previewTexture.width != 256)
        {
            previewTexture = new Texture2D(256, 256);
        }
        
        // Generate the noise preview
        for (int y = 0; y < 256; y++)
        {
            for (int x = 0; x < 256; x++)
            {
                float noiseValue = Mathf.PerlinNoise(
                    (x + NoiseTile.seed) * NoiseTile.noiseScale / 4f, 
                    (y + NoiseTile.seed) * NoiseTile.noiseScale / 4f
                );
                
                // Map the noise to a grayscale color
                Color color = new Color(noiseValue, noiseValue, noiseValue, 1f);
                previewTexture.SetPixel(x, y, color);
            }
        }
        
        // Apply the changes to the texture
        previewTexture.Apply();
    }
    
    private void DrawNoisePreview()
    {
        if (previewTexture == null)
        {
            GeneratePreview();
        }
        
        // Calculate the rect for the preview
        Rect previewRect = GUILayoutUtility.GetRect(256, 256);
        
        // Draw the preview texture
        EditorGUI.DrawPreviewTexture(previewRect, previewTexture);
        
        // Overlay some visual indicators to show tile distribution
        if (NoiseTile.middleVariants != null && NoiseTile.middleVariants.Length > 0)
        {
            int variantCount = NoiseTile.middleVariants.Length;
            for (int i = 0; i < variantCount; i++)
            {
                float threshold = (float)(i + 1) / variantCount;
                
                // Draw threshold lines
                Handles.color = new Color(1f, 0f, 0f, 0.7f);
                Handles.DrawLine(
                    new Vector3(previewRect.x, previewRect.y + previewRect.height * (1f - threshold), 0),
                    new Vector3(previewRect.x + previewRect.width, previewRect.y + previewRect.height * (1f - threshold), 0)
                );
                
                // Draw variant indicator labels
                if (NoiseTile.middleVariants[i] != null)
                {
                    Rect labelRect = new Rect(
                        previewRect.x + 5, 
                        previewRect.y + previewRect.height * (1f - (threshold - 0.5f / variantCount)), 
                        100, 
                        20
                    );
                    
                    GUI.Label(labelRect, "Variant " + i, EditorStyles.boldLabel);
                }
            }
        }
    }
}
#endif