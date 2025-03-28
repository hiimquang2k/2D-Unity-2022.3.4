using UnityEngine;

public class HealthPackSpawner : MonoBehaviour
{
    [SerializeField] private GameObject healthPackPrefab;
    [SerializeField] private Transform platformPos;
    [SerializeField] private int maxHealthPacks = 5;
    [SerializeField] private float a;
    [SerializeField] private float b;

    private float timer;
    [SerializeField] private int currentHealthPacks = 0;

    private void Start()
    {
        // Optional: Spawn initial health packs
        SpawnInitialHealthPacks();
    }

    private void Update()
    {
        if (currentHealthPacks == 0)
        {
            SpawnInitialHealthPacks();
        }
    }

    private void SpawnInitialHealthPacks()
    {
        // Spawn half of the max health packs at start
        for (int i = 0; i < maxHealthPacks; i++)
        {
            SpawnHealthPack();
        }
    }

    private void SpawnHealthPack()
    {
        // Spawn the health pack
        GameObject healthPackObj = Instantiate(healthPackPrefab, new Vector2(Random.Range(a, b), platformPos.position.y), Quaternion.identity);
        HealthPack healthPack = healthPackObj.GetComponent<HealthPack>();
        if (healthPack != null)
        {
            healthPack.Initialize(this);
        }

        // Keep track of spawned health packs
        currentHealthPacks++;
    }

    public void HandleHealthPackDestroyed()
    {
        currentHealthPacks--;
    }
}