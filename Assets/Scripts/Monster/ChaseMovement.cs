using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaseMovement : MonsterMovement
{
    [SerializeField]
    private float chaseRange;

    public float ChaseRange
    {
        get => chaseRange;
        set => chaseRange = value;
    }

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }

    protected override void HandleMovement()
    {
        if (monster == null || base.target == null)
            return;

        Vector2 direction = GetDirectionToTarget(base.target.position);
        moveDirection = direction;
        isMoving = direction != Vector2.zero;

        rb.velocity = direction * moveSpeed;

        // Update animator parameters
        animator.SetBool("isMoving", isMoving);
        if (isMoving)
        {
            animator.SetFloat("MoveX", direction.x);
            animator.SetFloat("MoveY", direction.y);
        }
    }

    public void SetTarget(Transform newTarget)
    {
        base.target = newTarget;
    }
}
