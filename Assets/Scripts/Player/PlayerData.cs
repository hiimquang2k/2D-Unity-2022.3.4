using UnityEngine;

[CreateAssetMenu(fileName = "PlayerData", menuName = "Game/Player/PlayerData")]
public class PlayerData : ScriptableObject
{
    [Header("Health Settings")]
    public int maxHealth = 100;

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

    [Header("Combat Settings")]
    public int attackDamage1 = 20;
    public int attackDamage2 = 30;
    public float attackDistance = 1.5f;
    public float attackCooldown = 0.5f;
    public float comboWindow = 0.8f;
    public LayerMask enemyLayer;
    public LayerMask bossLayer;
    public float hitstopDuration = 0.1f;
    public float hitstopTimeScale = 0.05f;
    public GameObject hitEffectPrefab;
    public AudioClip hitSound1;
    public AudioClip hitSound2;
    public Transform attackOrigin;
    public SaveState saveState = new SaveState();

    // Existing code...
    [System.Serializable]
    public class SaveState
    {
        public bool hasSave = false;
        public int savedHealth = 100;
        public Vector3 savedPosition = Vector3.zero;
        public string savedScene = "";
        public Vector2 savedVelocity = Vector2.zero;
        public bool wasDashing = false;
        public float dashCooldownRemaining = 0f;
    }

    public void ResetData()
    {
        saveState = new SaveState();
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