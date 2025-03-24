using UnityEngine;

public class DistanceDialogueTrigger : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform playerTransform; // Can be left empty
    [SerializeField] private FloatingDialogue dialogueSystem;
    
    [Header("Settings")]
    [SerializeField] private float dialogueDistance = 5f; 
    [SerializeField] private string dialogueText = "Hey! Come over here!";
    [SerializeField] private bool showOnce = true;
    [SerializeField] private string playerTag = "Player"; // Set the tag your player uses
    
    private bool hasShownDialogue = false;
    
    private void Start()
    {
        // Find player if not assigned
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag(playerTag);
            if (player != null)
            {
                playerTransform = player.transform;
            }
            else
            {
                Debug.LogError("Player not found! Make sure your player has the correct tag.");
            }
        }
        
        // Find dialogue system if not assigned
        if (dialogueSystem == null)
        {
            dialogueSystem = FindObjectOfType<FloatingDialogue>();
            if (dialogueSystem == null)
            {
                Debug.LogError("FloatingDialogue system not found in scene!");
            }
        }
    }
    
    private void Update()
    {
        // Skip if any required reference is missing
        if (playerTransform == null || dialogueSystem == null) return;
        
        // Skip if we've already shown the dialogue and it's set to show only once
        if (showOnce && hasShownDialogue) return;
        
        // Calculate distance between NPC and player
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        
        // Show dialogue when player is within range
        if (distanceToPlayer <= dialogueDistance)
        {
            dialogueSystem.SetTargetCharacter(transform);
            dialogueSystem.ShowDialogue(dialogueText);
            
            hasShownDialogue = true;
        }
    }
    
    // Optional: visualize the dialogue trigger range in the editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, dialogueDistance);
    }
}