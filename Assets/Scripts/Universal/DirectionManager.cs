using UnityEngine;

public class DirectionManager : MonoBehaviour
{
    [SerializeField] private bool isFacingRight = true;
    private Vector2 _inputDirection;
    private Vector2 _movementDirection;
    private Transform _transform;

    public bool IsFacingRight => isFacingRight;
    public Vector2 InputDirection => _inputDirection;
    public Vector2 MovementDirection => _movementDirection;

    private void Awake()
    {
        _transform = transform;
    }

    public void SetInitialDirection(Vector2 direction)
    {
        if (direction.x > 0)
        {
            isFacingRight = true;
        }
        else if (direction.x < 0)
        {
            isFacingRight = false;
        }
        
        Vector3 localScale = _transform.localScale;
        localScale.x = Mathf.Abs(localScale.x) * (isFacingRight ? 1 : -1);
        _transform.localScale = localScale;
    }

    public void UpdateDirection(float horizontalInput, float verticalInput)
    {
        _inputDirection = new Vector2(horizontalInput, verticalInput).normalized;

        // Update movement direction
        if (_inputDirection != Vector2.zero)
        {
            _movementDirection = _inputDirection;
        }

        // Handle facing direction
        if (horizontalInput < 0 && isFacingRight)
        {
            Flip();
        }
        else if (horizontalInput > 0 && !isFacingRight)
        {
            Flip();
        }
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 localScale = _transform.localScale;
        localScale.x *= -1f;
        _transform.localScale = localScale;
    }

    public Vector2 GetDirection(bool useMovementDirection = true)
    {
        return useMovementDirection && _movementDirection != Vector2.zero 
            ? _movementDirection 
            : (isFacingRight ? Vector2.right : Vector2.left);
    }
}