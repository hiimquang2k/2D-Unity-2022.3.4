using UnityEngine;

public class RandomMovement : MonsterMovement
{
    [Header("Random Movement Settings")]
    [SerializeField]
    public float changeDirectionTime = 2f;

    private float timer;

    protected override void Start()
    {
        base.Start();
        
        // Set random movement parameters from MonsterData
        if (monster != null && monster.monsterData != null)
        {
            changeDirectionTime = monster.monsterData.attackCooldown;
        }
        
        timer = changeDirectionTime;
    }

    protected override void HandleMovement()
    {
        if (timer > 0)
        {
            timer -= Time.deltaTime;
        }
        else
        {
            moveDirection = Random.insideUnitCircle.normalized;
            timer = changeDirectionTime + Random.Range(-0.5f, 0.5f); // Add some variation
        }

        rb.velocity = moveDirection * moveSpeed;
        isMoving = moveDirection != Vector2.zero;

        // Update animator parameters
        animator.SetBool("isMoving", isMoving);
        if (isMoving)
        {
            animator.SetFloat("MoveX", moveDirection.x);
            animator.SetFloat("MoveY", moveDirection.y);
        }
    }
}