using UnityEngine;

public class DeathState : MonsterState
{
    public DeathState(Monster monster) : base(monster) { }

    public override void Enter()
    {
        monster.animator.SetBool("isDead", true);
        monster.rb.velocity = Vector2.zero;
        monster.rb.bodyType = RigidbodyType2D.Static;
        monster.enabled = false;
    }

    public override void Exit()
    {
        // This state should never exit
    }
}