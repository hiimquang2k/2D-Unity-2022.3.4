using UnityEngine;

/**
 * AdvancedParallax is a script that allows for parallax scrolling in Unity.
 * It can be used to create a sense of depth in 2D games by moving background layers at different speeds.
 * 
 * To use this script, attach it to a GameObject in your scene and set up the ParallaxLayer array in the Inspector.
 * Each ParallaxLayer represents a layer in your parallax effect, and you can adjust its properties to control how it moves.
 * 
 * The script uses the main camera's position to calculate the movement of each layer, so make sure to set the main camera in the Camera.main property.
 */
public class AdvancedParallax : MonoBehaviour
{
    /**
     * A ParallaxLayer represents a single layer in the parallax effect.
     * It contains properties to control how the layer moves and whether it should scroll infinitely.
     */
    [System.Serializable]
    public class ParallaxLayer
    {
        public GameObject layerObject; // The GameObject representing the parallax layer
        public Vector2 parallaxFactor; // The factor by which the layer moves relative to the camera
        public bool infiniteHorizontal; // Whether the layer should scroll infinitely horizontally
        public bool infiniteVertical; // Whether the layer should scroll infinitely vertically
    }

    [SerializeField] private ParallaxLayer[] layers; // Array of parallax layers
    [SerializeField] private float smoothing = 1f; // Smoothing factor for movement

    private Transform cameraTransform; // Reference to the main camera's transform
    private Vector3 previousCameraPosition; // Store the previous position of the camera
    private float[] textureUnitSizeX; // Array to store texture sizes for horizontal scrolling
    private float[] textureUnitSizeY; // Array to store texture sizes for vertical scrolling

    /**
     * Called when the script is initialized.
     * Sets up the camera transform and initializes the texture size arrays.
     */
    private void Start()
    {
        cameraTransform = Camera.main.transform; // Get the main camera's transform
        previousCameraPosition = cameraTransform.position; // Initialize previous camera position
        
        textureUnitSizeX = new float[layers.Length]; // Initialize texture size arrays
        textureUnitSizeY = new float[layers.Length];
        
        // Calculate texture sizes for each layer
        for (int i = 0; i < layers.Length; i++)
        {
            if (layers[i].layerObject != null)
            {
                SpriteRenderer sr = layers[i].layerObject.GetComponent<SpriteRenderer>(); // Get the SpriteRenderer component
                if (sr != null)
                {
                    textureUnitSizeX[i] = sr.sprite.texture.width / sr.sprite.pixelsPerUnit; // Calculate width in world units
                    textureUnitSizeY[i] = sr.sprite.texture.height / sr.sprite.pixelsPerUnit; // Calculate height in world units
                }
            }
        }
    }

    /**
     * Called every frame after all other updates.
     * Calculates the movement of each layer based on the camera's position and applies it.
     */
    private void LateUpdate()
    {
        Vector3 deltaMovement = cameraTransform.position - previousCameraPosition; // Calculate camera movement since last frame
        
        for (int i = 0; i < layers.Length; i++)
        {
            if (layers[i].layerObject != null)
            {
                // Calculate parallax position
                Vector3 parallaxPosition = new Vector3(
                    deltaMovement.x * layers[i].parallaxFactor.x,
                    deltaMovement.y * layers[i].parallaxFactor.y,
                    0
                );
                
                // Apply movement
                layers[i].layerObject.transform.position += parallaxPosition; // Move the layer based on parallax
                
                // Handle infinite scrolling if enabled
                if (layers[i].infiniteHorizontal)
                {
                    float distanceX = Mathf.Abs(cameraTransform.position.x - layers[i].layerObject.transform.position.x);
                    if (distanceX >= textureUnitSizeX[i])
                    {
                        float offsetX = (cameraTransform.position.x - layers[i].layerObject.transform.position.x) % textureUnitSizeX[i];
                        layers[i].layerObject.transform.position = new Vector3(
                            cameraTransform.position.x + offsetX,
                            layers[i].layerObject.transform.position.y,
                            layers[i].layerObject.transform.position.z
                        );
                    }
                }
                
                if (layers[i].infiniteVertical)
                {
                    float distanceY = Mathf.Abs(cameraTransform.position.y - layers[i].layerObject.transform.position.y);
                    if (distanceY >= textureUnitSizeY[i])
                    {
                        float offsetY = (cameraTransform.position.y - layers[i].layerObject.transform.position.y) % textureUnitSizeY[i];
                        layers[i].layerObject.transform.position = new Vector3(
                            layers[i].layerObject.transform.position.x,
                            cameraTransform.position.y + offsetY,
                            layers[i].layerObject.transform.position.z
                        );
                    }
                }
            }
        }
        
        previousCameraPosition = cameraTransform.position; // Update the previous camera position
    }
}
