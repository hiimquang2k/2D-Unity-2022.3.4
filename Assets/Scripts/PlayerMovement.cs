using System;
using System.Collections;
using System.Data.Common;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public PlayerData Data;
    [Header("Layer")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask wallLayer;
    [Header("Trail Settings")]
    [SerializeField] private TrailRenderer trailRenderer;
    [SerializeField] private float trailTime = 0.5f;
    private bool canDash = true;
    private bool isDashing = false;
    public bool IsFacingRight { get; private set; }
    public float LastOnGroundTime { get; private set; }
    public float LastPressedJumpTime;
    public float horizontal { get; private set; }
    public float vertical { get; private set; }
    public Rigidbody2D body { get; private set; }
    private Animator anim;
    private BoxCollider2D boxCollider;
    private Vector2 dashDirection;
    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        boxCollider = GetComponent<BoxCollider2D>();
        if (trailRenderer == null)
        {
            trailRenderer = GetComponent<TrailRenderer>();

            // Create trail renderer if it doesn't exist
            if (trailRenderer == null)
            {
                trailRenderer = gameObject.AddComponent<TrailRenderer>();
                SetUpTrailRenderer();
            }
        }

        // Disable trail initially
        trailRenderer.emitting = false;
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
        LastOnGroundTime -= Time.deltaTime;
        if (LastPressedJumpTime > 0f)
            LastPressedJumpTime -= Time.deltaTime;

        horizontal = Input.GetAxisRaw("Horizontal");
        vertical = Input.GetAxisRaw("Vertical");
        dashDirection = new Vector2(horizontal, vertical).normalized;
        // If no directional input, dash in the direction player is facing
        if (dashDirection == Vector2.zero)
            dashDirection = IsFacingRight ? Vector2.right : Vector2.left;

        if (Input.GetKeyDown(KeyCode.Space))
            LastPressedJumpTime = Data.jumpInputBufferTime;
        if (LastPressedJumpTime > 0f && LastOnGroundTime > 0f) 
            Jump();
        if (Input.GetKeyUp(KeyCode.Space) && body.velocity.y > 0)
            body.velocity = new Vector2(body.velocity.x, 0);
        // Dash input (using left shift)
        if (Input.GetKeyDown(KeyCode.LeftShift) && canDash && !isDashing)
        {
            StartCoroutine(DashRoutine());
        }

        if (IsGrounded())
            LastOnGroundTime = Data.coyoteTime;

        anim.SetBool("Falling", IsFalling());
        anim.SetBool("Grounded", IsGrounded());
        anim.SetBool("Run", horizontal != 0);

        if (!isDashing)
        {
            Flip();
        }

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

    private void Flip()
    {
        if (IsFacingRight && horizontal < 0f || !IsFacingRight && horizontal > 0f)
        {
            IsFacingRight = !IsFacingRight;
            Vector2 localScale = transform.localScale;
            localScale.x *= -1f;
            transform.localScale = localScale;
        }
    }

    private void Run()
    {
        float targetSpeed = horizontal * Data.runMaxSpeed;
        float accelRate = (LastOnGroundTime > 0) ? Data.runAccelAmount : Data.runAccelAmount * Data.accelInAir;

        float speedDif = targetSpeed - body.velocity.x;
        float movement = speedDif * accelRate;
        body.AddForce(movement * Vector2.right, ForceMode2D.Force);

        if(Data.doConserveMomentum && Mathf.Abs(body.velocity.x) > Mathf.Abs(targetSpeed) && Mathf.Sign(body.velocity.x) == Mathf.Sign(targetSpeed) && Mathf.Abs(targetSpeed) > 0.01f && LastOnGroundTime < 0)
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

    private bool IsGrounded()
    {
        return Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0, Vector2.down, 0.1f, groundLayer);
    }

    private bool IsFalling()
    {
        return !IsGrounded() && body.velocity.y < 0;
    }

    public void CheckDirectionToFace(bool isMovingRight)
    {
        if (isMovingRight != IsFacingRight)
            Flip();
    }

    public void SetGravityScale(float scale)
    {
        body.gravityScale = scale;
    }
    private void SetUpTrailRenderer()
    {
        trailRenderer.time = trailTime;
        trailRenderer.startWidth = 0.5f;
        trailRenderer.endWidth = 0.1f;
        trailRenderer.startColor = new Color(1f, 1f, 1f, 0.8f);
        trailRenderer.endColor = new Color(1f, 1f, 1f, 0f);

        // Create a new material with the default sprite shader
        Material trailMaterial = new Material(Shader.Find("Sprites/Default"));
        trailRenderer.material = trailMaterial;
    }
    private IEnumerator DashRoutine()
    {
        // Start dash
        isDashing = true;
        canDash = false;

        // Store original gravity
        float originalGravity = body.gravityScale;
        body.gravityScale = 0; // Disable gravity during dash

        // Play dash animation if you have one
        // anim.SetTrigger("Dash");

        // Apply dash force
        body.velocity = dashDirection * Data.dashSpeed;

        // Enable trail effect
        trailRenderer.emitting = true;

        // Wait for dash duration
        yield return new WaitForSeconds(Data.dashDuration);

        // End dash
        isDashing = false;
        body.gravityScale = originalGravity;

        // Disable trail after dash
        trailRenderer.emitting = false;

        // Wait for cooldown
        yield return new WaitForSeconds(Data.dashCooldown);
        canDash = true;
    }
}