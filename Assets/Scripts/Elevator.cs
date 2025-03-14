using UnityEngine;
using System.Collections;

public class ElevatorWithGroundCheck : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private Transform[] stops;
    [SerializeField] private float speed = 2f;
    [SerializeField] private float waitTime = 1f;

    [Header("Platform Settings")]
    [SerializeField] private Transform platform;
    [SerializeField] private PlatformEffector2D platformEffector;

    [Header("Options")]
    [SerializeField] private bool loopPath = true;
    [SerializeField] private bool startAutomatically = true;
    [SerializeField] private bool reverseOnEnd = true;
    [SerializeField] private string groundLayerName = "Ground"; // Layer for ground detection

    private int currentStop = 0;
    private int direction = 1;
    private bool isMoving = false;
    private bool isWaiting = false;
    private Rigidbody2D platformRigidbody;

    void Start()
    {
        if (stops.Length < 2)
        {
            Debug.LogError("Elevator needs at least 2 stop positions!");
            enabled = false;
            return;
        }

        // Get or add required components
        platformRigidbody = platform.GetComponent<Rigidbody2D>();
        if (!platformRigidbody)
        {
            platformRigidbody = platform.gameObject.AddComponent<Rigidbody2D>();
            platformRigidbody.bodyType = RigidbodyType2D.Kinematic;
            platformRigidbody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            platformRigidbody.useFullKinematicContacts = true;
        }

        // Make sure platform is on the ground layer for player's ground check
        platform.gameObject.layer = LayerMask.NameToLayer(groundLayerName);

        // Set initial position
        if (platform && stops.Length > 0)
            platform.position = stops[0].position;

        if (startAutomatically)
            StartCoroutine(MoveElevator());
    }

    public void ActivateElevator()
    {
        if (!isMoving && !isWaiting)
            StartCoroutine(MoveElevator());
    }

    private IEnumerator MoveElevator()
    {
        isMoving = true;

        while (true)
        {
            // Set next destination
            Vector3 targetPosition = stops[currentStop].position;

            // Move to next stop using rigidbody
            while (Vector2.Distance(platform.position, targetPosition) > 0.01f)
            {
                Vector2 newPosition = Vector2.MoveTowards(
                    platform.position,
                    targetPosition,
                    speed * Time.fixedDeltaTime
                );

                platformRigidbody.MovePosition(newPosition);
                yield return new WaitForFixedUpdate();
            }

            // Wait at the stop
            isWaiting = true;
            yield return new WaitForSeconds(waitTime);
            isWaiting = false;

            // Determine next stop
            if (reverseOnEnd)
            {
                // Change direction at the ends
                if (currentStop == stops.Length - 1)
                    direction = -1;
                else if (currentStop == 0)
                    direction = 1;

                currentStop += direction;
            }
            else if (loopPath)
            {
                // Loop from last to first stop
                currentStop = (currentStop + 1) % stops.Length;
            }
            else
            {
                // Stop at the end
                if (currentStop < stops.Length - 1)
                    currentStop++;
                else
                    break;
            }
        }

        isMoving = false;
    }

    private void OnDrawGizmos()
    {
        // Draw path in editor for visualization
        if (stops == null || stops.Length < 2)
            return;

        Gizmos.color = Color.yellow;

        // Draw points
        for (int i = 0; i < stops.Length; i++)
        {
            if (stops[i] != null)
                Gizmos.DrawWireSphere(stops[i].position, 0.3f);
        }

        // Draw connections
        for (int i = 0; i < stops.Length - 1; i++)
        {
            if (stops[i] != null && stops[i + 1] != null)
                Gizmos.DrawLine(stops[i].position, stops[i + 1].position);
        }

        // Draw loop connection if applicable
        if (loopPath && stops.Length > 1 && stops[0] != null && stops[stops.Length - 1] != null)
            Gizmos.DrawLine(stops[stops.Length - 1].position, stops[0].position);
    }
}