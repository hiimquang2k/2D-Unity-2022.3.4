using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    #region Fields
    [Header("Data")]
    public PlayerData Data;

    [Header("Layers")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private LayerMask obstacleLayer;

    [Header("Trail Settings")]
    [SerializeField] private TrailRenderer trailRenderer;
    [SerializeField] private float trailTime = 0.5f;

    private Rigidbody2D body;
    private Animator anim;
    private BoxCollider2D boxCollider;
    private Vector2 dashDirection;

    private bool canDash = true;
    private bool isDashing = false;
    public bool IsFacingRight = true;

    private float LastOnGroundTime;
    private float LastPressedJumpTime;
    private float horizontal;
    private float vertical;
    #endregion

    #region Unity Methods
    private void Awake()
    {
        InitializeComponents();
    }

    private void Start()
    {
        IsFacingRight = true;
    }

    private void FixedUpdate()
    {
        if (!isDashing)
        {
            Run();
        }
    }

    private void Update()
    {
        UpdateInput();
        UpdateGroundedState();
        UpdateJump();
        UpdateDash();
        UpdateAnimation();
        UpdateGravity();
    }
    #endregion

    #region Movement Methods
    private void Run()
    {
        float targetSpeed = horizontal * Data.runMaxSpeed;
        float accelRate = (LastOnGroundTime > 0) ? Data.runAccelAmount : Data.runAccelAmount * Data.accelInAir;
CheckDirectionToFace(horizontal > 0);
        float speedDif = targetSpeed - body.velocity.x;
        float movement = speedDif * accelRate;
        body.AddForce(movement * Vector2.right, ForceMode2D.Force);

        if (Data.doConserveMomentum && Mathf.Abs(body.velocity.x) > Mathf.Abs(targetSpeed) 
            && Mathf.Sign(body.velocity.x) == Mathf.Sign(targetSpeed) 
            && Mathf.Abs(targetSpeed) > 0.01f 
            && LastOnGroundTime < 0)
        {
            body.velocity = new Vector2(body.velocity.x, body.velocity.y);
        }
    }

    private void Jump()
    {
        LastPressedJumpTime = 0;
        LastOnGroundTime = 0;
        anim.Play("Pre-Jump");
        body.velocity = new Vector2(body.velocity.x, Data.jumpForce);
    }

    private void UpdateGroundedState()
    {
        LastOnGroundTime -= Time.deltaTime;
        if (IsGrounded())
        {
            LastOnGroundTime = Data.coyoteTime;
        }
    }

    private void UpdateJump()
    {
        if (LastPressedJumpTime > 0f)
            LastPressedJumpTime -= Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.Space))
            LastPressedJumpTime = Data.jumpInputBufferTime;

        if (LastPressedJumpTime > 0f && LastOnGroundTime > 0f)
            Jump();

        if (Input.GetKeyUp(KeyCode.Space) && body.velocity.y > 0)
            body.velocity = new Vector2(body.velocity.x, 0);
    }

    private void UpdateDash()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) && canDash && !isDashing)
        {
            StartCoroutine(DashRoutine());
        }
    }

    private void UpdateAnimation()
    {
        anim.SetBool("Falling", IsFalling());
        anim.SetBool("Grounded", IsGrounded());
        anim.SetBool("Run", horizontal != 0);
    }

    private void UpdateGravity()
    {
        if (body.velocity.y < 0)
        {
            body.gravityScale = Data.gravityScale * Data.fallGravityMult;
            body.velocity = new Vector2(body.velocity.x, Mathf.Max(body.velocity.y, -Data.maxFallSpeed));
        }
        else if (!isDashing)
        {
            body.gravityScale = Data.gravityScale;
        }
    }
    #endregion

    #region Input Methods
    private void UpdateInput()
    {
        horizontal = Input.GetAxisRaw("Horizontal");
        vertical = Input.GetAxisRaw("Vertical");
        dashDirection = new Vector2(horizontal, vertical).normalized;

        if (dashDirection == Vector2.zero)
            dashDirection = IsFacingRight ? Vector2.right : Vector2.left;
    }

    private void Flip()
    {
        if (IsFacingRight && horizontal < 0f || !IsFacingRight && horizontal > 0f)
        {
            IsFacingRight = !IsFacingRight;
            Vector3 localScale = transform.localScale;
            localScale.x *= -1f;
            transform.localScale = localScale;
        }
    }
    #endregion

    #region Ground Check Methods
    private bool IsGrounded()
    {
        return Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0, Vector2.down, 0.1f, groundLayer | obstacleLayer);
    }

    private bool IsFalling()
    {
        return !IsGrounded() && body.velocity.y < 0;
    }
    #endregion

    #region Dash Methods
    private IEnumerator DashRoutine()
    {
        isDashing = true;
        canDash = false;
        float originalGravity = body.gravityScale;
        body.gravityScale = 0;
        anim.SetTrigger("Dash");
        body.velocity = dashDirection * Data.dashSpeed;
        trailRenderer.emitting = true;
        yield return new WaitForSeconds(Data.dashDuration);
        isDashing = false;
        body.gravityScale = originalGravity;
        yield return new WaitForSeconds(Data.dashCooldown);
        canDash = true;
        trailRenderer.emitting = false;
    }
    #endregion

    #region Utility Methods
    private void InitializeComponents()
    {
        body = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        boxCollider = GetComponent<BoxCollider2D>();

        if (trailRenderer == null)
        {
            trailRenderer = GetComponent<TrailRenderer>();
            if (trailRenderer == null)
            {
                trailRenderer = gameObject.AddComponent<TrailRenderer>();
                SetUpTrailRenderer();
            }
        }
        trailRenderer.emitting = false;
    }

    private void SetUpTrailRenderer()
    {
        trailRenderer.time = trailTime;
        trailRenderer.startWidth = 0.5f;
        trailRenderer.endWidth = 0.1f;
        trailRenderer.startColor = new Color(1f, 1f, 1f, 0.8f);
        trailRenderer.endColor = new Color(1f, 1f, 1f, 0f);
        Material trailMaterial = new Material(Shader.Find("Sprites/Default"));
        trailRenderer.material = trailMaterial;
    }
    #endregion

    #region Public Methods
    public void CheckDirectionToFace(bool isMovingRight)
    {
        if (isMovingRight != IsFacingRight)
            Flip();
    }

    public void SetGravityScale(float scale)
    {
        body.gravityScale = scale;
    }
    #endregion
}