using UnityEngine;

public class Bonfire : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float interactionRadius = 5f;
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    [Header("References")]
    [SerializeField] private PlayerData playerData;
    [SerializeField] private MobSpawner mobSpawner;

    private bool playerInRange;
    void Start()
    {
        GetComponent<CircleCollider2D>().radius = interactionRadius;
    }
    void Update()
    {
        if (playerInRange && Input.GetKeyDown(interactKey))
        {
            // Full heal and save
            GameManager.Instance.SaveGame(transform.position);
            mobSpawner.ToggleSpawning(false);
            
            // Visual feedback
            GetComponent<Animator>().SetTrigger("Activate");
            Debug.Log("Game saved at bonfire!");
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            mobSpawner.ToggleSpawning(false);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            mobSpawner.ToggleSpawning(true);
        }
    }
}