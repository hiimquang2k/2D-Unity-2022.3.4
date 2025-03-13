using System;
using System.Collections;
using System.Data.Common;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public PlayerData Data;
    [Header("Character config")]
    [SerializeField] private float speed;
    [SerializeField] private float jumpForce;
    [Header("Layer")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask wallLayer;
    [Header("Gravity")]
    [SerializeField] private float fallGravityValue;
    [SerializeField] private float defaultGravityValue;
    [SerializeField] private float maxFallSpeed;
    [SerializeField] private float wallSlidingSpeed;
    public bool IsFacingRight { get; private set; }
    public float LastOnGroundTime { get; private set; }
    public float LastPressedJumpTime;
    private bool isWallSliding;
    private bool isWallJumping;
    private float wallJumpingDirection;
    private float wallJumpingTime = 0.2f;
    private float wallJumpingCounter;
    private float wallJumpingDuration = 0.1f;
    private Vector2 wallJumppingPower = new(4f,8f);
    public float horizontal { get; private set; }
    public Rigidbody2D body { get; private set; }
    private Animator anim;
    private BoxCollider2D boxCollider;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        boxCollider = GetComponent<BoxCollider2D>();
    }

    private void Start()
    {
        IsFacingRight = true;
    }

    private void FixedUpdate()
    {
        if (!isWallJumping)
            Run();
    }

    private void Update()
    {
        LastOnGroundTime -= Time.deltaTime;
        if (LastPressedJumpTime > 0f)
            LastPressedJumpTime -= Time.deltaTime;

        horizontal = Input.GetAxisRaw("Horizontal");

        if (Input.GetKeyDown(KeyCode.Space))
            LastPressedJumpTime = Data.jumpInputBufferTime;
        if (LastPressedJumpTime > 0f && LastOnGroundTime > 0f) 
            Jump();
        if (Input.GetKeyUp(KeyCode.Space) && body.velocity.y > 0)
            body.velocity = new Vector2(body.velocity.x, 0);

        if (IsGrounded())
            LastOnGroundTime = Data.coyoteTime;

        anim.SetBool("Falling", IsFalling());
        anim.SetBool("Grounded", IsGrounded());
        anim.SetBool("Run", horizontal != 0);

        WallSlide();
        WallJump();
        if (!isWallJumping)
        {
            Flip();
        }
        if (body.velocity.y < 0)
        {
            body.gravityScale = defaultGravityValue * fallGravityValue;
            body.velocity = new Vector2(body.velocity.x, Mathf.Max(body.velocity.y, -maxFallSpeed));
        }
        else body.gravityScale = defaultGravityValue;
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
        body.velocity = new Vector2(body.velocity.x, jumpForce);
    }

    private void WallJump()
    {
        if (isWallSliding)
        {
            isWallJumping = false;
            wallJumpingDirection = -transform.localScale.x;
            wallJumpingCounter = wallJumpingTime;
            CancelInvoke(nameof(StopWallJumping));
        }
        else wallJumpingCounter -= Time.deltaTime;
        if (Input.GetKeyDown(KeyCode.Space) && wallJumpingCounter > 0f)
        {
            isWallJumping = true;
            body.velocity = new Vector2(wallJumpingDirection * wallJumppingPower.x, wallJumppingPower.y);
            wallJumpingCounter = 0f;

            if (transform.localScale.x != wallJumpingDirection)
            {
                IsFacingRight = !IsFacingRight;
                Vector2 localScale = transform.localScale;
                localScale.x *= -1f;
                transform.localScale = localScale;
            }
            Invoke(nameof(StopWallJumping), wallJumpingDuration);
        }
    }

    private void StopWallJumping()
    {
        isWallJumping = false;
    }

    private void WallSlide()
    {
        if (OnWall() && !IsGrounded() && horizontal != 0f)
        {
            isWallSliding = true;
            body.velocity = new Vector2(body.velocity.x, Mathf.Clamp(body.velocity.y, -wallSlidingSpeed, 0));
        }
        else isWallSliding = false;
    }

    private bool IsGrounded()
    {
        return Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0, Vector2.down, 0.1f, groundLayer);
    }

    private bool IsFalling()
    {
        return !IsGrounded() && body.velocity.y < 0;
    }

    private bool OnWall()
    {
        return Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0, new Vector2(transform.localScale.x,0), 0.1f, wallLayer);
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
}