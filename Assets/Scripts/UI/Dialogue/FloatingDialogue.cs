using System.Collections;
using UnityEngine;
using TMPro;

public class FloatingDialogue : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI dialogueText;
    
    [Header("Settings")]
    [SerializeField] private KeyCode progressKey = KeyCode.X;
    [SerializeField] private float characterTypingSpeed = 0.05f;
    [SerializeField] private Vector3 dialogueOffset = new Vector3(0, 1.5f, 0);
    
    private Transform targetCharacter;
    private Coroutine typingCoroutine;
    private string[] currentLines;
    private int currentLineIndex;
    private bool isTyping;

    public bool IsDialogueActive() => dialoguePanel.activeSelf;

    private void Awake()
    {
        dialoguePanel.SetActive(false);
    }

    public void StartDialogue(string[] lines)
    {
        StopAllCoroutines();
        currentLines = lines;
        currentLineIndex = 0;
        ShowNextLine();
    }

    private void ShowNextLine()
    {
        if (currentLineIndex >= currentLines.Length)
        {
            HideDialogue();
            return;
        }

        transform.position = targetCharacter.position + dialogueOffset;
        dialoguePanel.SetActive(true);
        typingCoroutine = StartCoroutine(TypeDialogue(currentLines[currentLineIndex]));
    }

    private IEnumerator TypeDialogue(string message)
    {
        isTyping = true;
        dialogueText.text = "";
        
        foreach (char letter in message.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(characterTypingSpeed);
        }
        isTyping = false;
    }

    private void Update()
    {
        if (!IsDialogueActive()) return;

        if (Input.GetKeyDown(progressKey))
        {
            if (isTyping)
            {
                // Skip typing animation
                StopCoroutine(typingCoroutine);
                dialogueText.text = currentLines[currentLineIndex];
                isTyping = false;
            }
            else
            {
                currentLineIndex++;
                ShowNextLine();
            }
        }
    }

    public void HideDialogue()
    {
        StopAllCoroutines();
        dialoguePanel.SetActive(false);
        currentLines = null;
        currentLineIndex = 0;
    }

    private void LateUpdate()
    {
        if (targetCharacter != null && IsDialogueActive())
        {
            transform.position = targetCharacter.position + dialogueOffset;
        }
    }
    public void SetTargetCharacter(Transform character)
    {
        targetCharacter = character;
    }
}