using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireflySpawner : MonoBehaviour
{
    [SerializeField] private GameObject fireflyPrefab;
    [SerializeField] private int fireflyCount = 15;
    [SerializeField] private Vector2 spawnArea = new Vector2(10f, 8f);
    [SerializeField] private float yOffset = 0f;
    
    private void Start()
    {
        // If prefab is not set, create a default one
        if (fireflyPrefab == null)
        {
            fireflyPrefab = new GameObject("Firefly");
            fireflyPrefab.AddComponent<Firefly>();
        }
        
        // Spawn fireflies
        SpawnFireflies();
    }
    
    private void SpawnFireflies()
    {
        for (int i = 0; i < fireflyCount; i++)
        {
            // Calculate random position
            Vector3 position = transform.position + new Vector3(
                Random.Range(-spawnArea.x/2, spawnArea.x/2),
                Random.Range(-spawnArea.y/2, spawnArea.y/2) + yOffset,
                0
            );
            
            // Instantiate firefly
            GameObject firefly = Instantiate(fireflyPrefab, position, Quaternion.identity);
            firefly.transform.SetParent(transform);
            
            // Randomize some properties
            Firefly fireflyComponent = firefly.GetComponent<Firefly>();
            if (fireflyComponent != null)
            {
                // Adjust properties here if needed
            }
        }
    }
    
    // Draw gizmo to show spawn area in editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 1f, 0f, 0.2f);
        Gizmos.DrawCube(transform.position + new Vector3(0, yOffset, 0), new Vector3(spawnArea.x, spawnArea.y, 0.1f));
    }
}