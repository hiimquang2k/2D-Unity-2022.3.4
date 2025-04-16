public class ChaseState : IMonsterState
{
    // Public property with private backing field
    public float UpdateInterval { get; set; }
    
    private Monster _monster;
    private float _lastUpdateTime;

    public ChaseState(Monster monster, float updateInterval = 0.5f)
    {
        _monster = monster;
        UpdateInterval = updateInterval; // Set through constructor
    }

    public void Enter() => _monster.Animator.SetBool("IsChasing", true);
    
    public void Update()
    {
        if (Time.time - _lastUpdateTime > _updateInterval)
        {
            Vector2 direction = (_monster.Target.position - _monster.transform.position).normalized;
            _monster.Move(direction * _monster.Data.chaseSpeed);
            _lastUpdateTime = Time.time;
        }
    }

    public void Exit()
    {
        _monster.Move(Vector2.zero);
        _monster.Animator.SetBool("IsChasing", false);
    }
}
