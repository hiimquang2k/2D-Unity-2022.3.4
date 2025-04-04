using UnityEngine;

public abstract class MonsterState
{
    protected IMonsterBehavior monsterBehavior;
    protected Monster monster;
    protected bool isExiting = false;

    public MonsterState(IMonsterBehavior monsterBehavior)
    {
        this.monsterBehavior = monsterBehavior;
        this.monster = monsterBehavior as Monster;
    }

    public virtual void Enter() { }
    public virtual void Exit() { }
    public virtual void Update() { }
    public virtual void Attack() { }
}
