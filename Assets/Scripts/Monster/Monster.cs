using UnityEngine;

public abstract class Monster : MonoBehaviour
{
    [Header("References")]
    public MonsterData Data;
    public Transform Target;
    public Animator Animator;
    public Rigidbody2D Rb;
    [Header("Direction")]
    [SerializeField] private DirectionManager _directionManager;
    [Header("Ground Settings")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckDistance = 0.1f;
    [SerializeField] private Transform groundCheckPoint;

    public StateMachine stateMachine = new();

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
}