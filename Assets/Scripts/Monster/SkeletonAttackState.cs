using UnityEngine;
public class SkeletonAttackState : AttackState
{
    private int _lastAttackVariant = 0;
    private ImprovedCameraShake _cameraShake;

    public SkeletonAttackState(Monster monster) : base(monster) 
    {
        _cameraShake = Object.FindObjectOfType<ImprovedCameraShake>();
    }

    protected override void ExecuteAttack()
    {
        Debug.Log("ExecuteAttack called!");  // Add this line
        // Visual feedback
        if (_monster.Data.attackSound) 
            AudioSource.PlayClipAtPoint(_monster.Data.attackSound, _monster.transform.position);
        
        base.ExecuteAttack(); // This handles the actual damage
        
        // Camera shake
        _cameraShake?.ShakeCamera(_lastAttackVariant == 1 ? 0.3f : 0.6f);
    }

    protected override int SelectAttackVariant()
    {
        _lastAttackVariant = _lastAttackVariant == 1 ? 2 : 1;
        return _lastAttackVariant;
    }
}