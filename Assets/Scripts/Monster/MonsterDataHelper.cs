using UnityEngine;

public static class MonsterDataHelper
{
    public static MonsterData CreateDefaultMonsterData(MonsterType type)
    {
        MonsterData data = ScriptableObject.CreateInstance<MonsterData>();
        switch (type)
        {
            case MonsterType.Slime:
                data.monsterType = MonsterType.Slime;
                data.maxHealth = 50;
                data.damage = 10f;
                data.attackCooldown = 1.5f;
                data.attackRange = 1f;
                data.patrolSpeed = 2f;
                data.chaseSpeed = 3f;
                data.patrolTime = 2f;
                data.aggroRange = 5f;
                data.idleTime = 1f;
                data.chaseTime = 2f;
                data.attackDuration = 0.5f;
                data.deathDuration = 1f;
                data.healthBarColor = Color.green;
                break;
                
            case MonsterType.Goblin:
                data.monsterType = MonsterType.Goblin;
                data.maxHealth = 100;
                data.damage = 20f;
                data.attackCooldown = 1f;
                data.attackRange = 1.5f;
                data.patrolSpeed = 3f;
                data.chaseSpeed = 4f;
                data.patrolTime = 3f;
                data.aggroRange = 8f;
                data.idleTime = 2f;
                data.chaseTime = 3f;
                data.attackDuration = 0.7f;
                data.deathDuration = 1.5f;
                data.healthBarColor = Color.red;
                break;
                
            // Add more cases for other monster types...
        }
        return data;
    }
}