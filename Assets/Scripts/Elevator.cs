using UnityEngine;
using System.Collections;
using UnityEngine.Tilemaps;

public class ElevatorWithGroundCheck : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private Transform[] stops;
    [SerializeField] private float speed = 2f;
    [SerializeField] private float waitTime = 1f;
    [SerializeField] private float snapThreshold = 0.1f; // How close to a tile center to snap

    [Header("Platform Settings")]
    [SerializeField] private Transform platform;
    [SerializeField] private PlatformEffector2D platformEffector;
    [SerializeField] private Tilemap tilemap; // Reference to the tilemap

    [Header("Options")]
    [SerializeField] private bool loopPath = true;
    [SerializeField] private bool startAutomatically = true;
    [SerializeField] private bool reverseOnEnd = true;
    [SerializeField] private string groundLayerName = "Ground";

    private int currentStop = 0;
    private int direction = 1;
    private bool isMoving = false;
    private bool isWaiting = false;
    private Rigidbody2D platformRigidbody;
    private Vector3 lastPosition;

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

        // Set initial position and snap to tile
        if (platform && stops.Length > 0)
        {
            platform.position = SnapToTile(stops[0].position);
            lastPosition = platform.position;
        }

        // Adjust stop points to be exactly on tile centers
        AdjustStopPoints();

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
            // Set next destination and snap to tile
            Vector3 targetPosition = SnapToTile(stops[currentStop].position);

            // Move to next stop using rigidbody
            while (Vector2.Distance(platform.position, targetPosition) > snapThreshold)
            {
                Vector2 newPosition = Vector2.MoveTowards(
                    platform.position,
                    targetPosition,
                    speed * Time.fixedDeltaTime
                );

                // Snap position if close enough to target
                if (Vector2.Distance(newPosition, targetPosition) <= snapThreshold)
                {
                    newPosition = targetPosition;
                }

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

    private Vector3 SnapToTile(Vector3 position)
    {
        if (tilemap == null)
            return position;

        // Get the size of a single tile
        Vector3 tileSize = tilemap.cellSize;
        
        // Convert world position to cell position
        Vector3Int cellPosition = tilemap.WorldToCell(position);
        // Convert back to world position
        Vector3 snappedPosition = tilemap.CellToWorld(cellPosition);
        
        // Snap to the center of the tile
        return new Vector3(
            snappedPosition.x + tileSize.x / 2,
            snappedPosition.y + tileSize.y / 2,
            position.z
        );
    }

    private void AdjustStopPoints()
    {
        if (tilemap == null)
            return;

        foreach (Transform stop in stops)
        {
            if (stop != null)
            {
                // Snap the stop point to the nearest tile center
                Vector3 snappedPosition = SnapToTile(stop.position);
                stop.position = snappedPosition;
            }
        }
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
            {
                Vector3 snapPos = SnapToTile(stops[i].position);
                Gizmos.DrawWireSphere(snapPos, 0.3f);
            }
        }

        // Draw connections
        for (int i = 0; i < stops.Length - 1; i++)
        {
            if (stops[i] != null && stops[i + 1] != null)
            {
                Vector3 start = SnapToTile(stops[i].position);
                Vector3 end = SnapToTile(stops[i + 1].position);
                Gizmos.DrawLine(start, end);
            }
        }

        // Draw loop connection if applicable
        if (loopPath && stops.Length > 1 && stops[0] != null && stops[stops.Length - 1] != null)
        {
            Vector3 start = SnapToTile(stops[stops.Length - 1].position);
            Vector3 end = SnapToTile(stops[0].position);
            Gizmos.DrawLine(start, end);
        }
    }
}