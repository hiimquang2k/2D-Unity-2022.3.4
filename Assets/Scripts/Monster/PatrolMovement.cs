using UnityEngine;

public class PatrolMovement : MonsterMovement
{
    [Header("Patrol Settings")]
    [SerializeField]
    public Transform[] patrolPoints;
    [SerializeField]
    public float waitTime = 2f;

    private int currentPoint;
    private float waitTimer;
    private bool isWaiting;

    protected override void Start()
    {
        base.Start();
        currentPoint = 0;
        waitTimer = 0f;
        isWaiting = false;
        
        // Set patrol speed from MonsterData
        if (monster != null && monster.monsterData != null)
        {
            moveSpeed = monster.monsterData.patrolSpeed;
            waitTime = monster.monsterData.patrolWaitTime;
        }
    }

    protected override void HandleMovement()
    {
        if (waitTimer > 0)
        {
            waitTimer -= Time.deltaTime;
            isMoving = false;
            return;
        }

        if (Vector2.Distance(transform.position, patrolPoints[currentPoint].position) < minimumDistanceToTarget)
        {
            currentPoint = (currentPoint + 1) % patrolPoints.Length;
            waitTimer = waitTime;
            isWaiting = true;
        }

        if (!isWaiting)
        {
            moveDirection = GetDirectionToTarget(patrolPoints[currentPoint].position);
            isMoving = true;
        }
    }
}