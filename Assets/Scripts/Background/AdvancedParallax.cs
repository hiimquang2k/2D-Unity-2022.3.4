using UnityEngine;
using System.Collections.Generic;

public class NonOverlappingParallax : MonoBehaviour
{
    [System.Serializable]
    public class ParallaxLayer
    {
        public GameObject layerObject;
        public Vector2 parallaxFactor;
        public bool infiniteHorizontal;
        public bool infiniteVertical;
        public float scale = 1f;

        [HideInInspector]
        public List<GameObject> instances = new List<GameObject>();

        [HideInInspector]
        public float exactWidth;

        [HideInInspector]
        public float exactHeight;
    }

    [SerializeField] private ParallaxLayer[] layers;
    [SerializeField] private bool debugMode = false;

    private Transform cameraTransform;
    private Vector3 previousCameraPosition;

    private void Start()
    {
        cameraTransform = Camera.main.transform;
        previousCameraPosition = cameraTransform.position;

        for (int i = 0; i < layers.Length; i++)
        {
            if (layers[i].layerObject == null) continue;

            SpriteRenderer sr = layers[i].layerObject.GetComponent<SpriteRenderer>();
            if (sr == null) continue;

            layers[i].exactWidth = sr.sprite.rect.width / sr.sprite.pixelsPerUnit * layers[i].scale;
            layers[i].exactHeight = sr.sprite.rect.height / sr.sprite.pixelsPerUnit * layers[i].scale;
            layers[i].layerObject.transform.localScale = Vector3.one * layers[i].scale;

            layers[i].instances.Clear();
            layers[i].instances.Add(layers[i].layerObject);

            if (layers[i].infiniteHorizontal)
            {
                SetupInitialInstances(i);
            }
        }
    }

    private void SetupInitialInstances(int layerIndex)
    {
        ParallaxLayer layer = layers[layerIndex];
        float viewportWidth = Camera.main.orthographicSize * 2 * Camera.main.aspect;
        int requiredInstances = Mathf.CeilToInt(viewportWidth / layer.exactWidth) + 2;

        for (int i = 1; i <= requiredInstances / 2; i++)
        {
            GameObject rightInstance = Instantiate(layer.layerObject, transform);
            rightInstance.transform.localScale = Vector3.one * layer.scale;
            rightInstance.transform.position = new Vector3(
                layer.layerObject.transform.position.x + (layer.exactWidth * i),
                layer.layerObject.transform.position.y,
                layer.layerObject.transform.position.z
            );
            layer.instances.Add(rightInstance);

            GameObject leftInstance = Instantiate(layer.layerObject, transform);
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
        Vector3 deltaMovement = cameraTransform.position - previousCameraPosition;

        for (int i = 0; i < layers.Length; i++)
        {
            ParallaxLayer layer = layers[i];
            if (layer.layerObject == null) continue;

            foreach (GameObject instance in layer.instances)
            {
                Vector3 parallaxMovement = new Vector3(
                    deltaMovement.x * layer.parallaxFactor.x,
                    deltaMovement.y * layer.parallaxFactor.y,
                    0
                );
                instance.transform.position += parallaxMovement;
            }

            if (layer.infiniteHorizontal)
            {
                RepositionInstances(i);
            }
        }

        previousCameraPosition = cameraTransform.position;
    }

    private void RepositionInstances(int layerIndex)
    {
        ParallaxLayer layer = layers[layerIndex];
        float cameraX = cameraTransform.position.x;
        float viewportWidth = Camera.main.orthographicSize * 2 * Camera.main.aspect;

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

        float leftScreenEdge = cameraX - (viewportWidth / 2) - (layer.exactWidth / 2);
        float rightScreenEdge = cameraX + (viewportWidth / 2) + (layer.exactWidth / 2);

        if (rightmost.transform.position.x + (layer.exactWidth / 2) < leftScreenEdge)
        {
            rightmost.transform.position = new Vector3(
                leftmost.transform.position.x + layer.exactWidth,
                rightmost.transform.position.y,
                rightmost.transform.position.z
            );
        }

        if (leftmost.transform.position.x - (layer.exactWidth / 2) > rightScreenEdge)
        {
            leftmost.transform.position = new Vector3(
                rightmost.transform.position.x - layer.exactWidth,
                leftmost.transform.position.y,
                leftmost.transform.position.z
            );
        }

        List<GameObject> instancesToRemove = new List<GameObject>();
        foreach (GameObject instance in layer.instances)
        {
            if (instance == layer.layerObject) continue;

            float posX = instance.transform.position.x;
            if (posX < leftScreenEdge - layer.exactWidth * 2 ||
                posX > rightScreenEdge + layer.exactWidth * 2)
            {
                instancesToRemove.Add(instance);
            }
        }

        foreach (GameObject instance in instancesToRemove)
        {
            layer.instances.Remove(instance);
            Destroy(instance);
        }
    }

    private void OnDrawGizmos()
    {
        if (!debugMode) return;

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

    // ======== NEW DESTRUCTION METHOD ========
    public void DestroyInArea(float leftBound, float rightBound, float destructionChance)
    {
        foreach (ParallaxLayer layer in layers)
        {
            List<GameObject> toRemove = new List<GameObject>();
            
            foreach (GameObject instance in layer.instances)
            {
                if (instance == layer.layerObject) continue;

                float xPos = instance.transform.position.x;
                if (xPos >= leftBound && xPos <= rightBound)
                {
                    if (Random.value < destructionChance)
                    {
                        toRemove.Add(instance);
                    }
                }
            }

            foreach (GameObject instance in toRemove)
            {
                layer.instances.Remove(instance);
                Destroy(instance);
            }
        }
    }
}
