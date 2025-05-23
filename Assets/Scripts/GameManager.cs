using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using Cinemachine; // Required for Cinemachine Virtual Camera

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    [Header("Scene Management")]
    [SerializeField] private string startingSceneName = "GameScene";
    [SerializeField] private Vector3 startingPosition = new Vector3(1, 1, 0);

    [SerializeField] private GameObject playerPrefab;
    public PlayerData playerData;
    public GameObject currentPlayer;

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
        Debug.Log($"Scene loaded: {scene.name}, hasSave: {playerData.saveState.hasSave}");
        
        // Check if we have a teleport position
        if (PlayerPrefs.HasKey("TargetPosX") && PlayerPrefs.HasKey("TargetPosY"))
        {
            // If loading from teleport, use the teleport position
            Vector2 teleportPos = new Vector2(
                PlayerPrefs.GetFloat("TargetPosX"),
                PlayerPrefs.GetFloat("TargetPosY")
            );
            // Clear the teleport position
            PlayerPrefs.DeleteKey("TargetPosX");
            PlayerPrefs.DeleteKey("TargetPosY");
            PlayerPrefs.Save();
            
            // Spawn player at teleport position
            SpawnPlayer(teleportPos);
        }
        // If this is the starting scene or a new game, spawn the player
        else if (scene.name == startingSceneName || 
                (scene.name == playerData.saveState.savedScene && playerData.saveState.hasSave))
        {
            SpawnPlayer();
        }
        // If coming from intro scene, make sure to spawn the player
        else if (scene.name == playerData.saveState.savedScene)
        {
            // If we get here, it means we're coming from the intro
            playerData.saveState.hasSave = true;
            SpawnPlayer();
        }
    }

    private void SpawnPlayer(Vector2? teleportPosition = null)
    {
        // Destroy existing player if any
        if (currentPlayer != null)
        {
            Destroy(currentPlayer);
        }

        // Determine spawn position
        Vector3 spawnPosition;
        if (teleportPosition.HasValue)
        {
            spawnPosition = teleportPosition.Value;
        }
        else if (playerData.saveState.hasSave)
        {
            spawnPosition = playerData.saveState.savedPosition;
        }
        else
        {
            spawnPosition = new Vector3(1, 1, 0);
        }

        // Instantiate player
        currentPlayer = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
        Debug.Log($"Player spawned at {spawnPosition} in {SceneManager.GetActiveScene().name}");
        
        // Make sure the player is active before setting up the camera
        if (currentPlayer.activeInHierarchy)
        {
            SetupCamera();
        }
        else
        {
            // If player isn't active yet, wait one frame and try again
            StartCoroutine(DelayedCameraSetup());
        }
    }

    private System.Collections.IEnumerator DelayedCameraSetup()
    {
        // Wait for one frame to ensure everything is initialized
        yield return null;
        SetupCamera();
    }

    private void SetupCamera()
    {
        if (currentPlayer == null) 
        {
            Debug.LogError("Cannot setup camera: currentPlayer is null");
            return;
        }

        // Find all Cinemachine virtual cameras in the scene
        var virtualCameras = FindObjectsOfType<CinemachineVirtualCamera>();
        Debug.Log($"Found {virtualCameras.Length} virtual cameras in the scene");

        // Assign player to all virtual cameras
        foreach (var cam in virtualCameras)
        {
            cam.Follow = currentPlayer.transform;
            cam.LookAt = currentPlayer.transform;
            Debug.Log($"Assigned player to camera: {cam.name}");
        }

        // If no cameras found, log a warning
        if (virtualCameras.Length == 0)
        {
            Debug.LogWarning("No Cinemachine Virtual Cameras found in the scene!");
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

    }

    public void StartNewGame()
    {
        // Reset player data
        playerData.ResetData();
        playerData.saveState.savedScene = startingSceneName;
        
        // Get starting position
        Vector3 startPosition = startingPosition;
        
        // Use SceneTransitionManager for smooth transition
        SceneTransitionManager.TransitionToScene(startingSceneName, startPosition);
    }
}