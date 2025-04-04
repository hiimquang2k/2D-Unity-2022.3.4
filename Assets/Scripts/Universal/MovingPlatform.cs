using UnityEngine;
using UnityEngine.Tilemaps;

public class MovingPlatform : MonoBehaviour
{
    public Grid grid;
    public Transform[] waypoints;
    public float moveSpeed = 5f;
    public float waitTime = 1f;
    public bool isLooping = true;
    public bool isOneWay = false;
    public bool isPlayerOnly = false;

    private int currentWaypoint = 0;
    private bool isMoving = true;
    private float timer = 0f;
    private Rigidbody2D rb;
    private BoxCollider2D platformCollider;
    private bool hasPlayer = false;
    private Vector3Int currentCell;
    private Vector3Int targetCell;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0;
        }

        platformCollider = GetComponent<BoxCollider2D>();
        if (platformCollider == null)
        {
            platformCollider = gameObject.AddComponent<BoxCollider2D>();
        }

        if (waypoints.Length == 0)
        {
            Debug.LogWarning("No waypoints assigned to moving platform!");
            return;
        }

        if (grid == null)
        {
            Debug.LogWarning("No Grid assigned to moving platform!");
            return;
        }

        // Set initial position and cell
        transform.position = waypoints[0].position;
        currentCell = grid.WorldToCell(transform.position);
    }

    private void Update()
    {
        if (isMoving)
        {
            MovePlatform();
        }
        else
        {
            CheckForPlayers();
        }
    }

    private void MovePlatform()
    {
        // Get the target cell from the current waypoint
        targetCell = grid.WorldToCell(waypoints[currentWaypoint].position);

        // Calculate the target position based on grid cell
        Vector3 targetPosition = grid.CellToWorld(targetCell);
        float step = moveSpeed * Time.deltaTime;

        // Move the platform using Rigidbody2D
        Vector3 newPosition = Vector3.MoveTowards(transform.position, targetPosition, step);
        rb.MovePosition(newPosition);

        // Check if we've reached the target position
        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            timer += Time.deltaTime;

            if (timer >= waitTime)
            {
                // Move to next waypoint
                if (isLooping)
                {
                    currentWaypoint = (currentWaypoint + 1) % waypoints.Length;
                }
                else if (isOneWay)
                {
                    currentWaypoint = currentWaypoint == waypoints.Length - 1 ? 0 : currentWaypoint + 1;
                }
                else
                {
                    currentWaypoint = currentWaypoint == waypoints.Length - 1 ? 0 : currentWaypoint + 1;
                }

                timer = 0f;
            }
        }
    }

    private void CheckForPlayers()
    {
        if (isPlayerOnly)
        {
            Collider2D[] colliders = Physics2D.OverlapBoxAll(transform.position, platformCollider.bounds.size * 0.9f, 0);
            
            hasPlayer = false;
            foreach (Collider2D collider in colliders)
            {
                if (collider.CompareTag("Player"))
                {
                    hasPlayer = true;
                    break;
                }
            }

            isMoving = hasPlayer;
        }
        else
        {
            isMoving = true;
        }
    }

    private void OnDrawGizmos()
    {
        if (grid != null)
        {
            Gizmos.color = Color.yellow;
            for (int i = 0; i < waypoints.Length; i++)
            {
                Vector3 cellPosition = grid.CellToWorld(grid.WorldToCell(waypoints[i].position));
                Gizmos.DrawWireCube(cellPosition, Vector3.one);
            }

            if (waypoints.Length > 1)
            {
                Gizmos.color = Color.green;
                for (int i = 0; i < waypoints.Length - 1; i++)
                {
                    Vector3 start = grid.CellToWorld(grid.WorldToCell(waypoints[i].position));
                    Vector3 end = grid.CellToWorld(grid.WorldToCell(waypoints[i + 1].position));
                    Gizmos.DrawLine(start, end);
                }
                if (isLooping)
                {
                    Vector3 start = grid.CellToWorld(grid.WorldToCell(waypoints[waypoints.Length - 1].position));
                    Vector3 end = grid.CellToWorld(grid.WorldToCell(waypoints[0].position));
                    Gizmos.DrawLine(start, end);
                }
            }
        }
    }
}