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

    public void Move(Vector2 velocity)
    {
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
        if (Target != null)
        {
            Vector2 direction = (Target.position - transform.position).normalized;
            directionManager.UpdateDirection(direction.x, direction.y);
        }
        stateMachine.Update();
    }

    protected virtual void Reset()
    {
        // Auto-populate common references in editor
        if (Rb == null) Rb = GetComponent<Rigidbody2D>();
        if (Animator == null) Animator = GetComponent<Animator>();
        if (_directionManager == null) _directionManager = GetComponent<DirectionManager>();
    }
}