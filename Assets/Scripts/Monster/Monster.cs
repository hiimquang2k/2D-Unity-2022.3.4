using UnityEngine;
using System.Collections;
public abstract class Monster : MonoBehaviour
{
    [Header("References")]
    public MonsterData Data;
    public Transform Target;
    public Animator Animator;
    public Rigidbody2D Rb;
    public HealthBarSystem healthBar;
    [Header("Direction")]
    [SerializeField] private DirectionManager _directionManager;

    [Header("Ground Settings")]
    [SerializeField] private float groundCheckDistance = 0.1f;
    [SerializeField] private Transform groundCheckPoint;
    
    [Header("Combat")]
    public float lastAttackTime;
    public float currentAttackCooldown;
    public LayerMask groundLayer;
    public StateMachine stateMachine = new();
    public string currentState;
    // Public property with getter that auto-adds component
    public DirectionManager directionManager
    {
        get
        {
            if (_directionManager == null)
            {
                _directionManager = GetComponent<DirectionManager>();
                if (_directionManager == null)
                {
                    _directionManager = gameObject.AddComponent<DirectionManager>();
                    Debug.LogWarning($"Auto-added DirectionManager to {gameObject.name}");
                }
            }
            return _directionManager;
        }
        set => _directionManager = value;
    }

    protected virtual void Start()
    {
        // Force initialization of direction manager
        var dm = directionManager; // This will auto-add if missing
        var healthSystem = GetComponent<HealthSystem>();
        if (healthSystem != null)
        {
            healthSystem.SetMaxHealth(Data.maxHealth);
            healthSystem.Initialize();
        }
        var healthBar = GetComponentInChildren<HealthBarSystem>();
        healthBar.Initialize();
        InitializeStates();
    }

    protected virtual void InitializeStates()
    {
        // Base states can be initialized here if needed
    }
    public bool IsGrounded()
    {
        if (groundCheckPoint == null)
        {
            Debug.LogWarning("Ground check point not assigned!");
            return true; // Fallback to true to avoid breaking movement
        }
        
        RaycastHit2D hit = Physics2D.Raycast(
            groundCheckPoint.position,
            Vector2.down,
            groundCheckDistance,
            groundLayer
        );
        
        Debug.DrawRay(groundCheckPoint.position, Vector2.down * groundCheckDistance, Color.red);
        return hit.collider != null;
    }

    // Modify the Move method to respect ground
    public void Move(Vector2 velocity)
    {
        if (!IsGrounded())
        {
            // If not grounded, only allow horizontal movement
            velocity.y = Rb.velocity.y; // Maintain current vertical velocity
        }

        Rb.velocity = velocity;

        // Only trigger walk animation above a threshold speed
        float walkThreshold = 0.1f;
        bool shouldWalk = velocity.magnitude > walkThreshold;
        Animator.SetBool("IsWalking", shouldWalk);

        // Optional: Still set speed for blend trees
        Animator.SetFloat("Speed", velocity.magnitude);
    }

    protected virtual void Update()
    {
        FindTarget();
        
        if (Target != null)
        {
            Vector2 direction = (Target.position - transform.position).normalized;
            directionManager.UpdateDirection(direction.x, direction.y);
        }
        currentState = stateMachine.CurrentState?.GetType().Name;
        stateMachine.Update();
    }

    private void FixedUpdate()
    {
        // Apply slight downward force when grounded to prevent bouncing
        if (IsGrounded() && Rb.velocity.y > 0)
        {
            Rb.AddForce(Vector2.down * 5f);
        }
    }
    private void FindTarget()
    {
        // If we already have a target and it's still in range, keep it
        if (Target != null && 
            Vector2.Distance(transform.position, Target.position) <= Data.aggroRange * 1.2f)
        {
            return;
        }

        // Otherwise look for new target
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            float distance = Vector2.Distance(transform.position, player.transform.position);
            if (distance <= Data.aggroRange)
            {
                Target = player.transform;
                return;
            }
        }

        // No valid target found
        Target = null;
    }

    protected virtual void Reset()
    {
        // Auto-populate common references in editor
        if (Rb == null) Rb = GetComponent<Rigidbody2D>();
        if (Animator == null) Animator = GetComponent<Animator>();
        if (_directionManager == null) _directionManager = GetComponent<DirectionManager>();
    }

    public void AnimationEvent_ExecuteAttack() => 
        (stateMachine.CurrentState as AttackState)?.ExecuteAttack();

    public void AnimationEvent_EndAttack() => 
        (stateMachine.CurrentState as AttackState)?.EndAttack();
    public bool HasObstacleAhead()
    {
    Vector2 direction = new Vector2(transform.localScale.x > 0 ? 1 : -1, 0);
    RaycastHit2D hit = Physics2D.Raycast(
        transform.position,
        direction,
        0.5f, // Detection distance
        groundLayer
    );
    return hit.collider != null;
    }
}