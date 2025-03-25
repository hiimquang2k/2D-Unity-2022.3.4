using UnityEngine;

public class CooldownSystem : MonoBehaviour
{
    private float _currentCooldown;
    private bool _isOnCooldown;

    public bool IsOnCooldown => _isOnCooldown;

    public void StartCooldown(float duration)
    {
        _currentCooldown = duration;
        _isOnCooldown = true;
    }

    public void UpdateCooldown()
    {
        if (_isOnCooldown)
        {
            _currentCooldown -= Time.deltaTime;
            if (_currentCooldown <= 0)
            {
                _isOnCooldown = false;
            }
        }
    }
}