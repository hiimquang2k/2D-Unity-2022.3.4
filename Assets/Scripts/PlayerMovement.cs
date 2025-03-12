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
    public void Awake()
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
    public void Update()
    {
        LastOnGroundTime -= Time.deltaTime;
        if (LastPressedJumpTime > 0f)
            LastPressedJumpTime -= Time.deltaTime;

        horizontal = Input.GetAxisRaw("Horizontal");

        //Flip character when moving
        //Method 1
        /*if (horizontal > 0.01f)
            transform.localScale = Vector3.one;
        else if (horizontal < -0.01f)
            transform.localScale = new Vector3(-1, 1, 1);*/
        if (Input.GetKeyDown(KeyCode.Space))
            LastPressedJumpTime = Data.jumpInputBufferTime;
        //Jump
        if (LastPressedJumpTime > 0f && LastOnGroundTime > 0f) 
        {
            Jump();
        }
        if (Input.GetKeyUp(KeyCode.Space) && body.velocity.y > 0)
            body.velocity = new Vector2(body.velocity.x, 0);
  
        //Set animator parameters
        if (IsGrounded())
            LastOnGroundTime = Data.coyoteTime;
        anim.SetBool("Falling",IsFalling());
        anim.SetBool("Grounded", IsGrounded());
        anim.SetBool("Run", horizontal != 0);
        
        WallSlide();
        WallJump();
        if (!isWallJumping)
        {
            Flip();
        }
        //Faster fall           
        if (body.velocity.y < 0)
        {
            body.gravityScale = defaultGravityValue * fallGravityValue;
            body.velocity = new Vector2(body.velocity.x, Mathf.Max(body.velocity.y, -maxFallSpeed));
        }
        else body.gravityScale = defaultGravityValue;
        if ((isWallJumping) && Mathf.Abs(body.velocity.y) < Data.jumpHangTimeThreshold)
		{
			SetGravityScale(Data.gravityScale * Data.jumpHangGravityMult);
		}
    }
    private void Flip()
    {
        if (IsFacingRight && horizontal <0f || !IsFacingRight && horizontal >0f)
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

		float accelRate;

        //Gets an acceleration value based on if we are accelerating (includes turning) 
		//or trying to decelerate (stop). As well as applying a multiplier if we're air borne.
		if (LastOnGroundTime > 0)
			accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? Data.runAccelAmount : Data.runDeccelAmount;
		else
			accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? Data.runAccelAmount * Data.accelInAir : Data.runDeccelAmount * Data.deccelInAir;

        if(Data.doConserveMomentum && Mathf.Abs(body.velocity.x) > Mathf.Abs(targetSpeed) && Mathf.Sign(body.velocity.x) == Mathf.Sign(targetSpeed) && Mathf.Abs(targetSpeed) > 0.01f && LastOnGroundTime < 0)
		{
			//Prevent any deceleration from happening, or in other words conserve are current momentum
			//You could experiment with allowing for the player to slightly increae their speed whilst in this "state"
			accelRate = 0; 
		}
        //Calculate difference between current velocity. and desired velocity.
		float speedDif = targetSpeed - body.velocity.x;
		//Calculate force along x-axis to apply to thr player

		float movement = speedDif * accelRate;

		//Convert this to a vector and apply to rigidbody
		body.AddForce(movement * Vector2.right, ForceMode2D.Force);

		/*
		 * For those interested here is what AddForce() will do
		 * RB.velocity. = new Vector2(RB.velocity..x + (Time.fixedDeltaTime  * speedDif * accelRate) / RB.mass, RB.velocity..y);
		 * Time.fixedDeltaTime is by default in Unity 0.02 seconds equal to 50 FixedUpdate() calls per second
		*/
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
    //Wall Sliding
    private void WallSlide()
    {
        if (OnWall() && !IsGrounded() && horizontal != 0f)
        {
            isWallSliding = true;
            body.velocity = new Vector2(body.velocity.x,Mathf.Clamp(body.velocity.y,-wallSlidingSpeed,0));
        }
        else isWallSliding = false;
    }
    private bool IsGrounded()
    {
        return Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0, Vector2.down, 0.1f, groundLayer);
    }
    private bool IsFalling()
    {
        if (!IsGrounded() && body.velocity.y < 0)
            return true;
        else return false;
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