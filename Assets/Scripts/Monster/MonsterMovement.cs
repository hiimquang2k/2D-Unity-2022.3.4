// MonsterMovement.cs
using UnityEngine;

public abstract class MonsterMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float turnSpeed = 5f;
    public float minimumDistanceToTarget = 0.1f;
    public LayerMask groundLayer;
    public LayerMask obstacleLayer;
    public LayerMask wallLayer;  // Add this for wall detection

    protected float moveSpeed;  // Add this line
    protected Rigidbody2D rb;
    protected Animator animator;
    protected bool isMoving = false;
    protected Vector2 moveDirection;
    protected Monster monster;
    protected bool isGrounded = false;
    protected bool isWallSliding = false;
    protected Transform target;  // Add this line

    public float MoveSpeed
    {
        get => moveSpeed;
        set => moveSpeed = value;
    }

    protected virtual void Start()
    {
        InitializeComponents();
    }

    protected virtual void InitializeComponents()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        monster = GetComponent<Monster>();
        
        if (monster != null && monster.monsterData != null)
        {
            moveSpeed = monster.monsterData.moveSpeed;
        }
    }

    protected virtual void Update()
    {
        HandleMovement();
        CheckGround();
        CheckWalls();
    }

    protected virtual void HandleMovement()
    {
        if (isMoving && !isWallSliding)
        {
            rb.velocity = moveDirection * moveSpeed;
            animator.SetBool("isMoving", true);
        }
        else
        {
            rb.velocity = Vector2.zero;
            animator.SetBool("isMoving", false);
        }
    }

    private void CheckGround()
    {
        Vector2 groundCheck = (Vector2)transform.position + Vector2.down * 0.1f;
        isGrounded = Physics2D.OverlapCircle(groundCheck, 0.1f, groundLayer);
        
        // If not grounded, check if we're on an obstacle
        if (!isGrounded)
        {
            isGrounded = Physics2D.OverlapCircle(groundCheck, 0.1f, obstacleLayer);
        }
    }

    private void CheckWalls()
    {
        Vector2 wallCheck = (Vector2)transform.position + moveDirection * 0.1f;
        isWallSliding = Physics2D.OverlapCircle(wallCheck, 0.1f, wallLayer);
    }

    protected virtual Vector2 GetDirectionToTarget(Vector2 targetPosition)
    {
        Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
        
        // Check if we're trying to move into a wall
        Vector2 wallCheck = (Vector2)transform.position + direction * 0.1f;
        if (Physics2D.OverlapCircle(wallCheck, 0.1f, wallLayer))
        {
            // If we're hitting a wall, try to find a way around
            // This is a simple implementation - you might want to make it more sophisticated
            if (direction.x > 0)  // Moving right
            {
                direction = Vector2.up;  // Try to move up
            }
            else  // Moving left
            {
                direction = Vector2.down;  // Try to move down
            }
        }
        
        return direction;
    }
}