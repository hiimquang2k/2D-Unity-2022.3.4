using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using Cinemachine; // Required for Cinemachine Virtual Camera

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    [Header("Scene Management")]
    [SerializeField] private string startingSceneName = "GameScene";
    public PlayerData playerData;
    [SerializeField] private GameObject playerPrefab;

    private GameObject currentPlayer;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    public void SaveGame(Vector3 position)
    {
        // Get player reference and components
        var player = FindObjectOfType<PlayerMovement>(); // Replace with your player controller
        var health = player.GetComponent<HealthSystem>(); // Assuming you have a Health component

        // Save all relevant data
        playerData.saveState.hasSave = true;
        playerData.saveState.savedPosition = position;
        playerData.saveState.savedHealth = health.CurrentHealth;
        playerData.saveState.savedScene = SceneManager.GetActiveScene().name;

        // Save perk system data if it exists
        var perkSystem = FindObjectOfType<PerkSystem>();
        if (perkSystem != null)
        {
            // Create a deep copy of the perk data
            playerData.saveState.perkSystemData = new PerkSystemData
            {
                currentXp = perkSystem.CurrentXp,
                currentLevel = perkSystem.CurrentLevel,
                xpToNextLevel = perkSystem.XpToNextLevel,
                availablePoints = perkSystem.AvailablePoints,
                healthPerks = new PerkCategory
                {
                    categoryName = perkSystem.HealthPerks.categoryName,
                    currentPoints = perkSystem.HealthPerks.currentPoints,
                    levels = new List<PerkLevel>(perkSystem.HealthPerks.levels)
                },
                damagePerks = new PerkCategory
                {
                    categoryName = perkSystem.DamagePerks.categoryName,
                    currentPoints = perkSystem.DamagePerks.currentPoints,
                    levels = new List<PerkLevel>(perkSystem.DamagePerks.levels)
                }
            };
        }

        Debug.Log($"Game saved at {position} in {playerData.saveState.savedScene}");
    }

    public void LoadGame()
    {
        if (!playerData.saveState.hasSave)
        {
            Debug.LogWarning("No save data found!");
            return;
        }

        // Load the saved scene
        SceneManager.LoadScene(playerData.saveState.savedScene);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == playerData.saveState.savedScene ||
           (scene.name == startingSceneName && !playerData.saveState.hasSave))
        {
            SpawnPlayer();
        }
    }

    private void SpawnPlayer()
    {
        // Destroy existing player if any
        if (currentPlayer != null)
        {
            Destroy(currentPlayer);
        }

        // Determine spawn position
        Vector3 spawnPosition = playerData.saveState.hasSave ?
            playerData.saveState.savedPosition : new
            Vector3(1, 1, 0); // Or your default spawn point

        // Instantiate player
        currentPlayer = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);

        // Get the Cinemachine Virtual Camera and assign player as its target
        CinemachineVirtualCamera virtualCamera = FindObjectOfType<CinemachineVirtualCamera>();
        if (virtualCamera != null)
        {
            virtualCamera.Follow = currentPlayer.transform;
            virtualCamera.LookAt = currentPlayer.transform;
        }

        // Restore health if loading from save
        if (playerData.saveState.hasSave)
        {
            var health = currentPlayer.GetComponent<HealthSystem>();
            if (health != null)
            {
                health.SetCurrentHealth(playerData.saveState.savedHealth);
            }

            // Restore perk system data if it exists
            var perkSystem = FindObjectOfType<PerkSystem>();
            if (perkSystem != null && playerData.saveState.perkSystemData != null)
            {
                perkSystem.LoadPerkData(playerData.saveState.perkSystemData);
                Debug.Log("Loaded perk system data from save");
            }
        }

        else
        {
            // Initialize a new game with default perk data
            var perkSystem = FindObjectOfType<PerkSystem>();
            if (perkSystem != null)
            {
                perkSystem.LoadPerkData(new PerkSystemData());
                Debug.Log("Initialized new perk system data");
            }
        }

        Debug.Log($"Player spawned at {spawnPosition} in {SceneManager.GetActiveScene().name}");
    }

    public void StartNewGame()
    {
        playerData.ResetData();
        playerData.saveState.savedScene = startingSceneName;
        SceneManager.LoadScene(startingSceneName);
    }
}