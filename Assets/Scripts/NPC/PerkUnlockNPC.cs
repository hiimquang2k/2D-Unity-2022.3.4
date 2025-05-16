using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PerkUnlockNPC : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private string npcName = "Mentor";
    [SerializeField] private string dialogText = "I can teach you the way of the warrior. Accept my guidance?";
    [SerializeField] private string acceptResponse = "You feel a surge of power as you learn new combat techniques!";
    [SerializeField] private string declineResponse = "Very well. Return when you're ready to learn.";
    [SerializeField] private string alreadyUnlockedText = "I have taught you all I can. Go forth and fight!";
    
    [Header("UI")]
    [SerializeField] private GameObject dialogBox;
    [SerializeField] private TextMeshProUGUI dialogTextUI;
    [SerializeField] private TextMeshProUGUI npcNameText;
    [SerializeField] private Button acceptButton;
    [SerializeField] private Button declineButton;
    
    private bool hasUnlockedPerk = false;
    private bool isInRange = false;
    
    private void Start()
    {
        if (dialogBox != null)
            dialogBox.SetActive(false);
            
        if (acceptButton != null)
            acceptButton.onClick.AddListener(OnAcceptClick);
            
        if (declineButton != null)
            declineButton.onClick.AddListener(OnDeclineClick);
    }
    
    private void Update()
    {
        if (isInRange && Input.GetKeyDown(KeyCode.E))
        {
            ShowDialog();
        }
    }
    
    private void ShowDialog()
    {
        if (dialogBox == null || dialogTextUI == null) return;
        
        dialogBox.SetActive(true);
        npcNameText.text = npcName;
        
        if (hasUnlockedPerk)
        {
            dialogTextUI.text = alreadyUnlockedText;
            acceptButton.gameObject.SetActive(false);
            declineButton.GetComponentInChildren<TextMeshProUGUI>().text = "Close";
        }
        else
        {
            dialogTextUI.text = dialogText;
            acceptButton.gameObject.SetActive(true);
            declineButton.GetComponentInChildren<TextMeshProUGUI>().text = "Not now";
        }
        
        // Pause game while dialog is open
        Time.timeScale = 0f;
    }
    
    private void HideDialog()
    {
        if (dialogBox != null)
            dialogBox.SetActive(false);
            
        // Resume game
        Time.timeScale = 1f;
    }
    
    private void OnAcceptClick()
    {
        if (!hasUnlockedPerk)
        {
            // Unlock the first damage perk
            PerkSystem.Instance.UnlockFirstDamagePerk();
            dialogTextUI.text = acceptResponse;
            hasUnlockedPerk = true;
            acceptButton.gameObject.SetActive(false);
            declineButton.GetComponentInChildren<TextMeshProUGUI>().text = "Thank you!";
        }
    }
    
    private void OnDeclineClick()
    {
        if (hasUnlockedPerk)
        {
            HideDialog();
        }
        else
        {
            dialogTextUI.text = declineResponse;
            acceptButton.gameObject.SetActive(false);
            declineButton.GetComponentInChildren<TextMeshProUGUI>().text = "Goodbye";
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isInRange = true;
            // Show "Press E to talk" prompt here if you have one
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isInRange = false;
            HideDialog();
            // Hide "Press E to talk" prompt here if you have one
        }
    }
}
