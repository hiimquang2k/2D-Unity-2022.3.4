using UnityEngine;

[CreateAssetMenu(fileName = "PlayerData", menuName = "Scriptable Objects/PlayerData")]
public class PlayerData : ScriptableObject
{
    [Header("Gravity")]
    [HideInInspector] public float gravityStrength;
    [HideInInspector] public float gravityScale;

    [Header("Gravity Modifiers")]
    public float fallGravityMult = 1.5f;
    public float maxFallSpeed = 25f;
    public float fastFallGravityMult = 2f;
    public float maxFastFallSpeed = 40f;
    
    [Header("Run")]
    public float runMaxSpeed = 10f;
    public float runAcceleration = 5f;
    [HideInInspector] public float runAccelAmount;
    public float runDecceleration = 5f;
    [HideInInspector] public float runDeccelAmount;

    [Header("Air Control")]
    [Range(0f, 1f)] 
    public float accelInAir = 0.75f;
    [Range(0f, 1f)] 
    public float deccelInAir = 0.75f;
    public bool doConserveMomentum = true;

    [Header("Jump")]
    public float jumpHeight = 4f;
    public float jumpTimeToApex = 0.5f;
    [HideInInspector] public float jumpForce;

    [Header("Jump Modifiers")]
    public float jumpCutGravityMult = 3f;
    [Range(0f, 1f)] 
    public float jumpHangGravityMult = 0.5f;
    public float jumpHangTimeThreshold = 0.1f;
    public float jumpHangAccelerationMult = 1.1f;
    public float jumpHangMaxSpeedMult = 1.1f;

    [Header("Wall Jump")]
    public Vector2 wallJumpForce = new Vector2(10f, 15f);
    [Range(0f, 1f)] 
    public float wallJumpRunLerp = 0.5f;
    [Range(0f, 1.5f)] 
    public float wallJumpTime = 0.2f;
    public bool doTurnOnWallJump = true;

    [Header("Dash")]
    public float dashSpeed = 20f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;

    [Header("Slide")]
    public float slideSpeed = 5f;
    public float slideAccel = 10f;

    [Header("Assists")]
    [Range(0.01f, 0.5f)] 
    public float coyoteTime = 0.2f;
    [Range(0.01f, 0.5f)] 
    public float jumpInputBufferTime = 0.2f;
    [Range(0.01f, 0.5f)] 
    public float attackInputBufferTime = 0.2f;

    [Header("Direction")]
    public Vector2 initialDirection = Vector2.right;

    [System.Serializable]
    public class SaveData
    {
        public int currentHealth;
        public Vector3 playerPosition;
    }

    private void OnValidate()
    {
        // Optimize calculations by caching gravity
        float absoluteGravity = Physics2D.gravity.y;
        gravityStrength = -(2 * jumpHeight) / (jumpTimeToApex * jumpTimeToApex);
        gravityScale = gravityStrength / absoluteGravity;

        // Simplified acceleration calculations
        runAccelAmount = Mathf.Clamp((50 * runAcceleration) / runMaxSpeed, 0.01f, runMaxSpeed);
        runDeccelAmount = Mathf.Clamp((50 * runDecceleration) / runMaxSpeed, 0.01f, runMaxSpeed);

        // Precise jump force calculation
        jumpForce = Mathf.Abs(gravityStrength) * jumpTimeToApex;
    }
}