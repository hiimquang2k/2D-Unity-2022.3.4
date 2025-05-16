using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class PerkUI : MonoBehaviour
{
    [Header("References")]
    public PerkSystem perkSystem;
    public GameObject perkPanel;
    
    [Header("XP Bar")]
    public Slider xpBar;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI xpText;
    
    [Header("Health Perk UI")]
    public Button healthButton;
    public Image[] healthPerkIcons;
    public TextMeshProUGUI healthPointsText;
    public Color activeColor = Color.white;
    public Color inactiveColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
    
    [Header("Damage Perk UI")]
    public Button damageButton;
    public Image[] damagePerkIcons;
    public TextMeshProUGUI damagePointsText;
    
    [Header("UI Elements")]
    public TextMeshProUGUI availablePointsText;

    private void Start()
    {
        // Initialize button listeners
        healthButton.onClick.AddListener(OnHealthButtonClicked);
        damageButton.onClick.AddListener(OnDamageButtonClicked);
        
        // Hide panel by default
        if (perkPanel != null)
            perkPanel.SetActive(false);
            
        // Subscribe to events
        if (perkSystem != null)
        {
            perkSystem.OnXpChanged.AddListener(UpdateXpUI);
            perkSystem.OnLevelUp.AddListener(OnLevelUp);
        }
            
        // Initial UI update
        UpdateUI();
    }


    private void OnDestroy()
    {
        // Unsubscribe from events
        if (perkSystem != null)
        {
            perkSystem.OnXpChanged.RemoveListener(UpdateXpUI);
            perkSystem.OnLevelUp.RemoveListener(OnLevelUp);
        }
    }

    private void Update()
    {
        // Toggle UI with 'P' key, but only if we're not already handling input in the panel
        if (Input.GetKeyDown(KeyCode.P) && !(perkPanel != null && perkPanel.activeSelf && EventSystem.current.currentSelectedGameObject != null))
        {
            TogglePerkMenu();
        }
    }

    private void TogglePerkMenu()
    {
        if (perkPanel == null) return;
        
        bool isActive = !perkPanel.activeSelf;
        perkPanel.SetActive(isActive);
        
        if (isActive)
        {
            UpdateUI();
        }
        
        // Optional: Pause game when menu is open
        Time.timeScale = isActive ? 0 : 1;
    }

    private void UpdateUI()
    {
        if (perkSystem == null) return;

        // Update available points
        availablePointsText.text = $"Available Points: {perkSystem.AvailablePoints}";
        
        // Update level and XP
        if (perkSystem != null)
        {
            UpdateXpUI(perkSystem.CurrentXp, perkSystem.XpToNextLevel);
        }
        
        // Update perk UIs
        UpdatePerkUI(perkSystem.HealthPerks, healthPerkIcons, healthPointsText);
        UpdatePerkUI(perkSystem.DamagePerks, damagePerkIcons, damagePointsText);
        
        // Update button interactivity
        healthButton.interactable = perkSystem.AvailablePoints > 0;
        damageButton.interactable = perkSystem.AvailablePoints > 0;
    }

    private void UpdateXpUI(int currentXp, int xpToNextLevel)
    {
        if (xpBar != null)
        {
            xpBar.maxValue = xpToNextLevel;
            xpBar.value = currentXp;
        }
        
        if (levelText != null)
            levelText.text = $"Level {perkSystem.CurrentLevel}";
            
        if (xpText != null)
            xpText.text = $"{currentXp} / {xpToNextLevel} XP";
    }

    private void OnLevelUp(int newLevel)
    {
        // Play level up effect
        // AudioManager.Instance.Play("LevelUp");
        
        // Update UI
        UpdateUI();
    }

    private void UpdatePerkUI(PerkCategory perkCategory, Image[] perkIcons, TextMeshProUGUI pointsText)
    {
        if (perkCategory == null)
        {
            Debug.LogError("PerkCategory is null!");
            return;
        }
        
        if (perkIcons == null)
        {
            Debug.LogError("PerkIcons array is null!");
            return;
        }
        
        pointsText.text = $"{perkCategory.currentPoints} points";
        Debug.Log($"Updating {perkCategory.categoryName} UI with {perkCategory.levels?.Count} levels and {perkIcons.Length} icons");
        
        for (int i = 0; i < perkIcons.Length; i++)
        {
            if (i < perkCategory.levels?.Count)
            {
                bool isActive = perkCategory.levels[i].isActive;
                Debug.Log($"Setting perk {i} active: {isActive}");
                perkIcons[i].color = isActive ? activeColor : inactiveColor;
                
                // Optional: Add visual effects for active perks
                if (isActive)
                {
                    // Example: Add glow effect
                    // perkIcons[i].GetComponent<Outline>().enabled = true;
                }
            }
            else
            {
                perkIcons[i].gameObject.SetActive(false);
            }
        }
    }

    private void OnHealthButtonClicked()
    {
        if (perkSystem.AvailablePoints <= 0) return;
        perkSystem.AddPointToHealth();
        UpdateUI();
    }

    private void OnDamageButtonClicked()
    {
        if (perkSystem.AvailablePoints <= 0) return;
        perkSystem.AddPointToDamage();
        UpdateUI();
    }

    // Call this when you want to add points (e.g., on level up)
    public void AddPointToSpend()
    {
        perkSystem.AddAvailablePoints(1);
        UpdateUI();
    }
}
