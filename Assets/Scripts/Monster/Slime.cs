using UnityEngine;

public class Slime : Monster
{
    private int _currentSplitGeneration;
    private SlimeData _slimeData;

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
    }

    protected override void InitializeStates()
    {
        base.InitializeStates(); // Initialize any base states first
        
        // Override with slime-specific states
        stateMachine.AddState(MonsterStateType.Attack, new SlimeAttackState(this));
        stateMachine.AddState(MonsterStateType.Death, new SlimeDeathState(this));
    }

    public void AttemptSplit()
    {
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