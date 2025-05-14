using UnityEngine;

public class Slime : Monster
{
    private int _currentSplitGeneration;
    public SlimeData _slimeData;
    private bool _isDying = false;

    protected override void Start()
    {
        // First validate data type
        if (!(Data is SlimeData))
        {
            Debug.LogError("Slime requires SlimeData scriptable object!");
            return;
        }
        
        _slimeData = (SlimeData)Data;
        _currentSplitGeneration = 0;
        
        base.Start(); // Call base initialization

        var healthSystem = GetComponent<HealthSystem>();
        if (healthSystem != null)
        {
            healthSystem.OnDeath += TriggerDeath;
        }
    }

    private void OnDestroy()
    {
        var healthSystem = GetComponent<HealthSystem>();
        if (healthSystem != null)
        {
            healthSystem.OnDeath -= TriggerDeath;
        }
    }

    public void TriggerDeath()
    {
        if (_isDying) return;
        _isDying = true;
        
        // Attempt to split before entering death state
        AttemptSplit();
        
        // Change to death state
        stateMachine.SwitchState(MonsterStateType.Death);
    }

    protected override void InitializeStates()
    {
        base.InitializeStates(); // Initialize any base states first
        
        // Override with slime-specific states
        stateMachine.AddState(MonsterStateType.Attack, new AttackState(this));
        stateMachine.AddState(MonsterStateType.Death, new DeathState(this));
        stateMachine.AddState(MonsterStateType.Idle, new IdleState(this));
        stateMachine.AddState(MonsterStateType.Chase, new ChaseState(this));
        stateMachine.AddState(MonsterStateType.Patrol, new PatrolState(this));
        stateMachine.SwitchState(MonsterStateType.Idle);
    }

    public void AttemptSplit()
    {
        if (!_slimeData.canSplit) return;
        if (_currentSplitGeneration >= _slimeData.maxSplitGenerations) return;
        
        if (Random.value <= _slimeData.splitChance)
        {
            for (int i = 0; i < 2; i++)
            {
                Vector2 spawnPos = (Vector2)transform.position + 
                                Random.insideUnitCircle * 0.5f;
                
                var babySlime = Instantiate(
                    _slimeData.smallerSlimePrefab, 
                    spawnPos, 
                    Quaternion.identity
                ).GetComponent<Slime>();
                
                if (babySlime != null)
                {
                    babySlime._currentSplitGeneration = _currentSplitGeneration + 1;
                    babySlime.transform.localScale *= 0.7f;
                }
            }
        }
    }
}