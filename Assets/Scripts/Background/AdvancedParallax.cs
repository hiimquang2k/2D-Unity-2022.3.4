using UnityEngine;
using System.Collections.Generic;

public class NonOverlappingParallax : MonoBehaviour
{
    [System.Serializable]
    public class ParallaxLayer
    {
        public GameObject layerObject; // The original background object
        public Vector2 parallaxFactor; // How fast this layer moves relative to camera
        public bool infiniteHorizontal; // Whether to tile horizontally
        public bool infiniteVertical; // Whether to tile vertically
        public float scale = 1f; // Scale factor for this layer

        [HideInInspector]
        public List<GameObject> instances = new List<GameObject>(); // All instances of this background

        [HideInInspector]
        public float exactWidth; // The exact width of the sprite in world units

        [HideInInspector]
        public float exactHeight; // The exact height of the sprite in world units
    }

    [SerializeField] private ParallaxLayer[] layers;
    [SerializeField] private bool debugMode = false; // Toggle to show debug info

    private Transform cameraTransform;
    private Vector3 previousCameraPosition;

    private void Start()
    {
        cameraTransform = Camera.main.transform;
        previousCameraPosition = cameraTransform.position;

        // Initialize each layer
        for (int i = 0; i < layers.Length; i++)
        {
            if (layers[i].layerObject == null) continue;

            SpriteRenderer sr = layers[i].layerObject.GetComponent<SpriteRenderer>();
            if (sr == null) continue;

            // Calculate exact width and height
            layers[i].exactWidth = sr.sprite.rect.width / sr.sprite.pixelsPerUnit;
            layers[i].exactHeight = sr.sprite.rect.height / sr.sprite.pixelsPerUnit;

            // Apply scale factor to dimensions
            layers[i].exactWidth *= layers[i].scale;
            layers[i].exactHeight *= layers[i].scale;

            // Apply scale to the original object
            layers[i].layerObject.transform.localScale = Vector3.one * layers[i].scale;

            if (debugMode)
            {
                Debug.Log($"Layer {layers[i].layerObject.name} - Width: {layers[i].exactWidth}, Height: {layers[i].exactHeight}");
            }

            // Add original to instances list
            layers[i].instances.Clear();
            layers[i].instances.Add(layers[i].layerObject);

            // Create initial instances for infinite scrolling
            if (layers[i].infiniteHorizontal)
            {
                SetupInitialInstances(i);
            }
        }
    }

    private void SetupInitialInstances(int layerIndex)
    {
        ParallaxLayer layer = layers[layerIndex];

        // Calculate how many instances we need based on screen width
        float viewportWidth = Camera.main.orthographicSize * 2 * Camera.main.aspect;
        int requiredInstances = Mathf.CeilToInt(viewportWidth / layer.exactWidth) + 2; // +2 for buffer

        if (debugMode)
        {
            Debug.Log($"Setting up {requiredInstances} instances for layer {layer.layerObject.name}");
        }

        // Create left and right instances
        for (int i = 1; i <= requiredInstances / 2; i++)
        {
            // Right instance - exactly placed at width intervals
            GameObject rightInstance = Instantiate(layer.layerObject, transform);
            rightInstance.name = $"{layer.layerObject.name}_right{i}";
            rightInstance.transform.localScale = Vector3.one * layer.scale;
            rightInstance.transform.position = new Vector3(
                layer.layerObject.transform.position.x + (layer.exactWidth * i),
                layer.layerObject.transform.position.y,
                layer.layerObject.transform.position.z
            );
            layer.instances.Add(rightInstance);

            // Left instance - exactly placed at width intervals
            GameObject leftInstance = Instantiate(layer.layerObject, transform);
            leftInstance.name = $"{layer.layerObject.name}_left{i}";
            leftInstance.transform.localScale = Vector3.one * layer.scale;
            leftInstance.transform.position = new Vector3(
                layer.layerObject.transform.position.x - (layer.exactWidth * i),
                layer.layerObject.transform.position.y,
                layer.layerObject.transform.position.z
            );
            layer.instances.Add(leftInstance);
        }
    }

    private void LateUpdate()
    {
        // Calculate camera movement
        Vector3 deltaMovement = cameraTransform.position - previousCameraPosition;

        // Process each layer
        for (int i = 0; i < layers.Length; i++)
        {
            ParallaxLayer layer = layers[i];
            if (layer.layerObject == null) continue;

            // Move all instances based on parallax factor
            foreach (GameObject instance in layer.instances)
            {
                Vector3 parallaxMovement = new Vector3(
                    deltaMovement.x * layer.parallaxFactor.x,
                    deltaMovement.y * layer.parallaxFactor.y,
                    0
                );
                instance.transform.position += parallaxMovement;
            }

            // Handle repositioning if needed
            if (layer.infiniteHorizontal)
            {
                RepositionInstances(i);
            }
        }

        // Update previous camera position
        previousCameraPosition = cameraTransform.position;
    }

    private void RepositionInstances(int layerIndex)
    {
        ParallaxLayer layer = layers[layerIndex];
        float cameraX = cameraTransform.position.x;
        float viewportWidth = Camera.main.orthographicSize * 2 * Camera.main.aspect;

        // Find leftmost and rightmost instances
        GameObject leftmost = null;
        GameObject rightmost = null;
        float leftmostX = float.MaxValue;
        float rightmostX = float.MinValue;

        foreach (GameObject instance in layer.instances)
        {
            float posX = instance.transform.position.x;

            if (posX < leftmostX)
            {
                leftmostX = posX;
                leftmost = instance;
            }

            if (posX > rightmostX)
            {
                rightmostX = posX;
                rightmost = instance;
            }
        }

        if (leftmost == null || rightmost == null) return;

        // Define screen boundaries with some padding
        float leftScreenEdge = cameraX - (viewportWidth / 2) - (layer.exactWidth / 2);
        float rightScreenEdge = cameraX + (viewportWidth / 2) + (layer.exactWidth / 2);

        // Check if rightmost instance is too far left (off-screen)
        if (rightmost.transform.position.x + (layer.exactWidth / 2) < leftScreenEdge)
        {
            // Move to the right of the leftmost
            rightmost.transform.position = new Vector3(
                leftmost.transform.position.x + layer.exactWidth,
                rightmost.transform.position.y,
                rightmost.transform.position.z
            );

            if (debugMode)
            {
                Debug.Log($"Repositioned {rightmost.name} to right side at {rightmost.transform.position.x}");
            }
        }

        // Check if leftmost instance is too far right (off-screen)
        if (leftmost.transform.position.x - (layer.exactWidth / 2) > rightScreenEdge)
        {
            // Move to the left of the rightmost
            leftmost.transform.position = new Vector3(
                rightmost.transform.position.x - layer.exactWidth,
                leftmost.transform.position.y,
                leftmost.transform.position.z
            );

            if (debugMode)
            {
                Debug.Log($"Repositioned {leftmost.name} to left side at {leftmost.transform.position.x}");
            }
        }

        // Check if we need additional instances on the right
        if (rightmost.transform.position.x < rightScreenEdge)
        {
            GameObject newInstance = Instantiate(layer.layerObject, transform);
            newInstance.name = $"{layer.layerObject.name}_newRight";
            newInstance.transform.localScale = Vector3.one * layer.scale;
            newInstance.transform.position = new Vector3(
                rightmost.transform.position.x + layer.exactWidth,
                rightmost.transform.position.y,
                rightmost.transform.position.z
            );
            layer.instances.Add(newInstance);

            if (debugMode)
            {
                Debug.Log($"Created new right instance at {newInstance.transform.position.x}");
            }
        }

        // Check if we need additional instances on the left
        if (leftmost.transform.position.x > leftScreenEdge)
        {
            GameObject newInstance = Instantiate(layer.layerObject, transform);
            newInstance.name = $"{layer.layerObject.name}_newLeft";
            newInstance.transform.localScale = Vector3.one * layer.scale;
            newInstance.transform.position = new Vector3(
                leftmost.transform.position.x - layer.exactWidth,
                leftmost.transform.position.y,
                leftmost.transform.position.z
            );
            layer.instances.Add(newInstance);

            if (debugMode)
            {
                Debug.Log($"Created new left instance at {newInstance.transform.position.x}");
            }
        }

        // Clean up instances that are far outside the view
        List<GameObject> instancesToRemove = new List<GameObject>();
        foreach (GameObject instance in layer.instances)
        {
            // Don't remove the original
            if (instance == layer.layerObject) continue;

            // If instance is far outside camera view (with extra padding)
            float posX = instance.transform.position.x;
            if (posX < leftScreenEdge - layer.exactWidth * 2 ||
                posX > rightScreenEdge + layer.exactWidth * 2)
            {
                instancesToRemove.Add(instance);
            }
        }

        // Remove and destroy far away instances
        foreach (GameObject instance in instancesToRemove)
        {
            layer.instances.Remove(instance);
            Destroy(instance);

            if (debugMode)
            {
                Debug.Log($"Removed far away instance {instance.name}");
            }
        }
    }

    // Visual debugging in Scene view
    private void OnDrawGizmos()
    {
        if (!debugMode) return;

        // Draw camera boundaries
        if (Camera.main != null)
        {
            float verticalSize = Camera.main.orthographicSize;
            float horizontalSize = verticalSize * Camera.main.aspect;

            Gizmos.color = Color.yellow;
            Vector3 cameraPos = Camera.main.transform.position;
            Vector3 topLeft = new Vector3(cameraPos.x - horizontalSize, cameraPos.y + verticalSize, cameraPos.z);
            Vector3 topRight = new Vector3(cameraPos.x + horizontalSize, cameraPos.y + verticalSize, cameraPos.z);
            Vector3 bottomLeft = new Vector3(cameraPos.x - horizontalSize, cameraPos.y - verticalSize, cameraPos.z);
            Vector3 bottomRight = new Vector3(cameraPos.x + horizontalSize, cameraPos.y - verticalSize, cameraPos.z);

            Gizmos.DrawLine(topLeft, topRight);
            Gizmos.DrawLine(topRight, bottomRight);
            Gizmos.DrawLine(bottomRight, bottomLeft);
            Gizmos.DrawLine(bottomLeft, topLeft);
        }
    }
}