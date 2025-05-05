using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public PlayerData playerData;
    public GameObject playerPrefab;

    private GameObject currentPlayer;

    void Awake()
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

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SpawnPlayer();
    }

    void SpawnPlayer()
    {
        if (currentPlayer == null)
        {
            currentPlayer = Instantiate(playerPrefab);
        }

        if (playerData.saveState.hasSave)
        {
            currentPlayer.transform.position = playerData.saveState.savedPosition;
            currentPlayer.GetComponent<HealthSystem>().SetCurrentHealth(playerData.saveState.savedHealth);
        }
    }

    public void SaveGame(Vector3 position)
    {
        playerData.saveState.hasSave = true;
        playerData.saveState.savedHealth = currentPlayer.GetComponent<HealthSystem>().GetMaxHealth();
        playerData.saveState.savedPosition = position;
        playerData.saveState.savedScene = SceneManager.GetActiveScene().name;
    }

    public void LoadGame()
    {
        if (playerData.saveState.hasSave)
        {
            SceneManager.LoadScene(playerData.saveState.savedScene);
        }
    }
}