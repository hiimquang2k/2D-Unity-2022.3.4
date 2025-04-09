using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    [Header("Data")]
    public PlayerData Data;

    [Header("Layers")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private LayerMask obstacleLayer;

    private Rigidbody2D body;
    private Animator anim;
    private BoxCollider2D boxCollider;
    private TrailRenderer trailRenderer;

    private CooldownSystem cooldownSystem;
    private DirectionManager directionManager;

    // Optimization: Use constants for frequently used values
    private const float GROUND_CHECK_DISTANCE = 0.1f;

    // State tracking
    [SerializeField] private float lastOnGroundTime;
    private float lastPressedJumpTime;
    private float horizontal;
    private float vertical;

    private bool canDash = true;
    private bool isDashing;

    private void Awake()
    {
        InitializeComponents();
    }

    private void Start()
    {
        directionManager.SetInitialDirection(Data.initialDirection);
    }

    private void FixedUpdate()
    {
        if (!isDashing)
        {
            Run();
        }

        if (Input.GetKey(KeyCode.A))
        {
            body.AddForce(new Vector2(-5f, 0), ForceMode2D.Force);
        }
        else if (Input.GetKey(KeyCode.D))
        {
            body.AddForce(new Vector2(5f, 0), ForceMode2D.Force);
        }
    }

    private void Update()
    {
        UpdateTimers();
        UpdateInput();
        UpdateMovement();
        UpdateAnimation();
    }

    private void UpdateTimers()
    {
        // Optimize timer calculations
        lastOnGroundTime -= Time.deltaTime;
        lastPressedJumpTime -= Time.deltaTime;

        // Update ground state
        if (IsGrounded())
        {
            lastOnGroundTime = Data.coyoteTime;
        }

        // Update dash cooldown
        cooldownSystem.UpdateCooldown();
        
        // Check cooldowns
        canDash = !cooldownSystem.IsOnCooldown("Dash");
    }

    private void UpdateInput()
    {
        horizontal = Input.GetAxisRaw("Horizontal");
        vertical = Input.GetAxisRaw("Vertical");
        directionManager.UpdateDirection(horizontal, vertical);
    }

    private void UpdateMovement()
    {
        // Jump input handling
        if (Input.GetKeyDown(KeyCode.Space))
            lastPressedJumpTime = Data.jumpInputBufferTime;

        if (lastPressedJumpTime > 0f && lastOnGroundTime > 0f)
            Jump();

        // Jump cut mechanic
        if (Input.GetKeyUp(KeyCode.Space) && body.velocity.y > 0)
            body.velocity = new Vector2(body.velocity.x, 0);

        // Dash input
        if (Input.GetKeyDown(KeyCode.LeftShift) && canDash)
        {
            StartCoroutine(Dash());
        }

        // Gravity management
        UpdateGravity();
    }

    private void Run()
    {
        float targetSpeed = horizontal * Data.runMaxSpeed;
        float accelRate = (lastOnGroundTime > 0) ? Data.runAccelAmount : Data.runAccelAmount * Data.accelInAir;
        float speedDif = targetSpeed - body.velocity.x;
        float movement = speedDif * accelRate;

        body.AddForce(movement * Vector2.right, ForceMode2D.Force);

        // Momentum conservation with optimized conditions
        if (Data.doConserveMomentum && 
            Mathf.Abs(body.velocity.x) > Mathf.Abs(targetSpeed) && 
            Mathf.Sign(body.velocity.x) == Mathf.Sign(targetSpeed) && 
            Mathf.Abs(targetSpeed) > 0.01f && 
            lastOnGroundTime < 0)
        {
            body.velocity = new Vector2(body.velocity.x, body.velocity.y);
        }
    }

    private void Jump()
    {
        lastPressedJumpTime = 0;
        lastOnGroundTime = 0;
        body.velocity = new Vector2(body.velocity.x, Data.jumpForce);
    }

    private void UpdateGravity()
    {
        if (body.velocity.y < 0)
        {
            body.gravityScale = Data.gravityScale * Data.fallGravityMult;
            body.velocity = new Vector2(
                body.velocity.x, 
                Mathf.Max(body.velocity.y, -Data.maxFallSpeed)
            );
        }
        else if (!isDashing)
        {
            body.gravityScale = Data.gravityScale;
        }
    }

    private void UpdateAnimation()
    {
        anim.SetBool("Falling", IsFalling());
        anim.SetBool("Grounded", IsGrounded());
        anim.SetBool("Run", horizontal != 0);
    }

    private bool IsGrounded()
    {
        return Physics2D.BoxCast(
            boxCollider.bounds.center, 
            boxCollider.bounds.size, 
            0, 
            Vector2.down, 
            GROUND_CHECK_DISTANCE, 
            groundLayer | obstacleLayer
        );
    }

    private bool IsFalling()
    {
        return !IsGrounded() && body.velocity.y < 0;
    }

    private IEnumerator Dash()
    {
        isDashing = true;
        cooldownSystem.StartCooldown(Data.dashCooldown, "Dash");
        
        float dashTime = 0f;
        while (dashTime < Data.dashDuration)
        {
            body.velocity = directionManager.GetDirection() * Data.dashSpeed;
            dashTime += Time.deltaTime;
            yield return null;
        }

        isDashing = false;
    }

    private void InitializeComponents()
    {
        body = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        boxCollider = GetComponent<BoxCollider2D>();

        // Simplified component initialization
        cooldownSystem = GetComponent<CooldownSystem>() ?? gameObject.AddComponent<CooldownSystem>();
        directionManager = GetComponent<DirectionManager>() ?? gameObject.AddComponent<DirectionManager>();

        // Trail renderer setup (optional)
        trailRenderer = GetComponent<TrailRenderer>();
        if (trailRenderer == null)
        {
            trailRenderer = gameObject.AddComponent<TrailRenderer>();
            ConfigureTrailRenderer();
        }
        trailRenderer.emitting = false;
    }

    private void ConfigureTrailRenderer()
    {
        trailRenderer.time = 0.5f;
        trailRenderer.startWidth = 0.5f;
        trailRenderer.endWidth = 0.1f;
        trailRenderer.startColor = new Color(1f, 1f, 1f, 0.8f);
        trailRenderer.endColor = new Color(1f, 1f, 1f, 0f);
        trailRenderer.material = new Material(Shader.Find("Sprites/Default"));
    }

    public void SetGravityScale(float scale) => body.gravityScale = scale;

}