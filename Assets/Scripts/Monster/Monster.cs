using UnityEngine;

public abstract class Monster : MonoBehaviour
{
    [Header("References")]
    public MonsterData Data;
    public Transform Target;
    public Animator Animator;
    public Rigidbody2D Rb;

    public StateMachine stateMachine = new();
    
    public void Move(Vector2 velocity)
    {
        Rb.velocity = velocity;
        Animator.SetFloat("Speed", velocity.magnitude);
    }

    protected virtual void Update() => stateMachine.Update();
}
