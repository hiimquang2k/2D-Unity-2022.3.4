using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    [Header("Scene Management")]
    [SerializeField] private string startingSceneName = "GameScene";
    [SerializeField] private PlayerData playerData;
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

        // Restore health if loading from save
        if (playerData.saveState.hasSave)
        {
            var health = currentPlayer.GetComponent<HealthSystem>();
            if (health != null)
            {
                health.SetCurrentHealth(playerData.saveState.savedHealth);
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