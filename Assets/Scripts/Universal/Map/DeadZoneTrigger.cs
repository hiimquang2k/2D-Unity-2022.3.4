using UnityEngine;
using UnityEngine.SceneManagement; // Required for scene reloading

public class DeathZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the colliding object is the player
        if (other.CompareTag("Player"))
        {
            // Call the player's death function or reload the scene
            Destroy(other.gameObject); // Destroys the player
            SceneManager.LoadScene(SceneManager.GetActiveScene().name); // Reload current scene
        }
    }
}