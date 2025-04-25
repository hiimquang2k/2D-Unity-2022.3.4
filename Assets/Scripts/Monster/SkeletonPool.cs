using UnityEngine;
using System.Collections.Generic;

public class SkeletonPool : MonoBehaviour
{
    public static SkeletonPool Instance { get; private set; }

    [Header("Settings")]
    [SerializeField] private GameObject skeletonPrefab;
    [SerializeField] private int initialPoolSize = 10;

    private Queue<Skeleton> _pool = new Queue<Skeleton>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializePool();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializePool()
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            CreateNewSkeleton();
        }
    }

    private Skeleton CreateNewSkeleton()
    {
        GameObject obj = Instantiate(skeletonPrefab);
        obj.SetActive(false);
        Skeleton skeleton = obj.GetComponent<Skeleton>();
        
        skeleton.PoolInitialize();
        skeleton.OnDeath += ReturnToPool;
        
        _pool.Enqueue(skeleton);
        return skeleton;
    }

    public Skeleton GetFromPool(Vector3 position, Quaternion rotation)
    {
        Skeleton skeleton = _pool.Count > 0 ? _pool.Dequeue() : CreateNewSkeleton();
        
        skeleton.transform.position = position;
        skeleton.transform.rotation = rotation;
        skeleton.gameObject.SetActive(true);
        
        // Reset and prepare skeleton
        skeleton.ResetSkeleton();
        skeleton.stateMachine.SwitchState(MonsterStateType.Idle);
        
        return skeleton;
    }

    public void ReturnToPool(Skeleton skeleton)
    {
        skeleton.gameObject.SetActive(false);
        skeleton.stateMachine.Reset(); // Clear current state
        _pool.Enqueue(skeleton);
    }
}