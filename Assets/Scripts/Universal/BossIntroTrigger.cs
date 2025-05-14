using UnityEngine;

public class BossIntroTrigger : MonoBehaviour
{
    [SerializeField] private BossIntroManager bossIntroManager;
    [SerializeField] private Collider2D triggerCollider;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            bossIntroManager.StartBossIntro();
            triggerCollider.enabled = false; // Disable after first trigger
        }
    }
}