using UnityEngine;
public class SlimeDeathState : IMonsterState
{
    private Slime _slime;

    public SlimeDeathState(Slime slime)
    {
        _slime = slime;
    }

    public void Enter()
    {
        _slime.Animator.SetTrigger("Death");
        _slime.AttemptSplit();
        Object.Destroy(_slime.gameObject, 1.2f); // Match animation length
    }

    public void Update() { }
    public void Exit() { }
}