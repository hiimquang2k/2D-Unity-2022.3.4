using System.Collections;
using UnityEngine;
using TMPro;

public class FloatingDialogue : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI dialogueText;
    
    [Header("Settings")]
    [SerializeField] private float displayTime = 3f;
    [SerializeField] private float characterTypingSpeed = 0.05f;
    [SerializeField] private Vector3 dialogueOffset = new Vector3(0, 1.5f, 0);
    
    private Transform targetCharacter;
    private Coroutine typingCoroutine;
    private Coroutine displayCoroutine;
    
    private void Awake()
    {
        dialoguePanel.SetActive(false);
    }
    
    public void SetTargetCharacter(Transform character)
    {
        targetCharacter = character;
    }
    
    public void ShowDialogue(string message)
    {
        if (targetCharacter == null)
        {
            Debug.LogError("Target character not set for dialogue!");
            return;
        }
        
        // Stop any ongoing dialogue
        StopAllCoroutines();
        
        // Position dialogue panel above character
        transform.position = targetCharacter.position + dialogueOffset;
        
        // Show dialogue
        dialoguePanel.SetActive(true);
        
        // Start typing effect
        typingCoroutine = StartCoroutine(TypeDialogue(message));
        
        // Start display timer
        displayCoroutine = StartCoroutine(HideDialogueAfterDelay());
    }
    
    private IEnumerator TypeDialogue(string message)
    {
        dialogueText.text = "";
        
        foreach (char letter in message.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(characterTypingSpeed);
        }
    }
    
    private IEnumerator HideDialogueAfterDelay()
    {
        yield return new WaitForSeconds(displayTime);
        dialoguePanel.SetActive(false);
    }
    
    private void LateUpdate()
    {
        // Keep dialogue above character if target exists
        if (targetCharacter != null && dialoguePanel.activeSelf)
        {
            transform.position = targetCharacter.position + dialogueOffset;
        }
    }
    
    // Call this to hide dialogue immediately
    public void HideDialogue()
    {
        StopAllCoroutines();
        dialoguePanel.SetActive(false);
    }
}