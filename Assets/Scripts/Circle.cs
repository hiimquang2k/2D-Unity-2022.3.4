using UnityEngine;

public class CircleRoller : MonoBehaviour
{
    public float rollSpeed = 5f; // Adjust this to change the rolling speed.
    public LayerMask groundLayer; // Assign the ground layer in the Inspector.
    private Rigidbody2D rb;
    private Collider2D circleCollider;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody2D component not found on the GameObject!");
        }
    }

    void Update()
    {
        // Example: Roll left and right with A and D keys.
        float horizontalInput = Input.GetAxis("Horizontal");

        // Apply torque for rolling. Torque makes the object rotate.
        rb.AddTorque(-horizontalInput * rollSpeed); // Negative sign to match visual direction.

        // Optional: Apply a constant roll if you don't want input.
        // rb.AddTorque(-rollSpeed);

        // Optional: Limit the angular velocity to prevent excessive spinning.
        // rb.angularVelocity = Mathf.Clamp(rb.angularVelocity, -maxAngularVelocity, maxAngularVelocity);
    }
    bool IsGrounded()
    {
        // Create a small overlap circle at the bottom of the circle collider.
        Vector2 origin = new Vector2(transform.position.x, transform.position.y - circleCollider.bounds.extents.y); // Bottom center.
        float radius = 0.1f; // Adjust as needed.

        // Check for overlap with the ground layer.
        Collider2D hit = Physics2D.OverlapCircle(origin, radius, groundLayer);

        return hit != null;
    }

    // Optional: Draw the overlap circle in the Scene view for debugging.
    void OnDrawGizmos()
    {
        if (circleCollider != null)
        {
            Gizmos.color = Color.red;
            Vector2 origin = new Vector2(transform.position.x, transform.position.y - circleCollider.bounds.extents.y);
            Gizmos.DrawWireSphere(origin, 0.1f);
        }
    }
}
