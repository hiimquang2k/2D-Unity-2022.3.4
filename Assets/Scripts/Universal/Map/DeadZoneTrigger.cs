using UnityEngine;
using UnityEngine.SceneManagement; // Required for scene reloading

public class DeathZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the colliding object is the player
        if (other.CompareTag("Player"))
        {
            // Call the player's death function and handle game state
            Destroy(other.gameObject); // Destroys the player
            if (GameManager.Instance.playerData.saveState.hasSave)
            {
                GameManager.Instance.LoadGame(); // Load saved game if save exists
            }
            else
            {
                GameManager.Instance.StartNewGame(); // Start new game if no save exists
            }
        }
    }
}