using UnityEngine;

public abstract class Monster : MonoBehaviour
{
    [Header("References")]
    public MonsterData data;
    public Transform target;
    public Animator animator;
    public Rigidbody2D rb;

    protected StateMachine stateMachine = new();
    
    public void Move(Vector2 velocity)
    {
        rb.velocity = velocity;
        animator.SetFloat("Speed", velocity.magnitude);
    }

    protected virtual void Update() => stateMachine.Update();
}
