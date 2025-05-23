using UnityEngine;
using UnityEngine.SceneManagement;

public class Teleporter : MonoBehaviour
{
    [SerializeField] private string targetScene;
    [SerializeField] private Vector2 targetPosition;
    [SerializeField] private KeyCode activationKey = KeyCode.W;
    [SerializeField] private GameObject interactionPrompt;

    private bool isPlayerInRange = false;

    private void Update()
    {
        if (isPlayerInRange && Input.GetKeyDown(activationKey))
        {
            Teleport();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(false);
            }
        }
    }

    private void Teleport()
    {
        if (!string.IsNullOrEmpty(targetScene))
        {
            // Store the target position in a persistent object or use PlayerPrefs
            PlayerPrefs.SetFloat("TargetPosX", targetPosition.x);
            PlayerPrefs.SetFloat("TargetPosY", targetPosition.y);
            PlayerPrefs.Save();
            
            // Load the target scene
            SceneManager.LoadScene(targetScene);
        }
        else
        {
            Debug.LogWarning("No target scene assigned to the teleporter!");
        }
    }
}
