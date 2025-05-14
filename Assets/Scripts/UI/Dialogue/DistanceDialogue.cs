using UnityEngine;

public class DistanceDialogueTrigger : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform playerTransform; // Can be left empty
    [SerializeField] private FloatingDialogue dialogueSystem;
    
    [Header("Settings")]
    [SerializeField] private float dialogueDistance = 5f; 
    [SerializeField] private string[] dialogueLines;
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
        if (playerTransform == null || dialogueSystem == null) return;
        if (showOnce && hasShownDialogue) return;
        
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        
        if (distanceToPlayer <= dialogueDistance)
        {
            dialogueSystem.SetTargetCharacter(transform);
            
            // Only start dialogue if not already showing
            if (!dialogueSystem.IsDialogueActive())
            {
                dialogueSystem.StartDialogue(dialogueLines);
                hasShownDialogue = true;
            }
        }
    }
    
    // Optional: visualize the dialogue trigger range in the editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, dialogueDistance);
    }
}