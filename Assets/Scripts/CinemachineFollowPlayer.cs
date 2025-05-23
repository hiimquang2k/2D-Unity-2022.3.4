using UnityEngine;
using Cinemachine;

public class CinemachineFollowPlayer : MonoBehaviour
{
    [Tooltip("The tag of the GameObject to follow")]
    public string targetTag = "Player";
    
    [Tooltip("The Cinemachine Virtual Camera component")]
    public CinemachineVirtualCamera virtualCamera;

    private Transform targetTransform;
    private bool hasValidTarget = false;

    private void Start()
    {
        // If no virtual camera is assigned, try to get it from this GameObject
        if (virtualCamera == null)
        {
            virtualCamera = GetComponent<CinemachineVirtualCamera>();
        }
        
        // Initial attempt to find the player
        FindAndSetTarget();
    }

    private void Update()
    {
        // If we don't have a valid target or the target was destroyed, try to find it again
        if (!hasValidTarget || targetTransform == null)
        {
            FindAndSetTarget();
        }
    }

    private void FindAndSetTarget()
    {
        if (virtualCamera == null) return;

        // Find the player by tag
        GameObject player = GameObject.FindGameObjectWithTag(targetTag);
        
        if (player != null)
        {
            targetTransform = player.transform;
            virtualCamera.Follow = targetTransform;
            virtualCamera.LookAt = targetTransform;
            hasValidTarget = true;
        }
        else if (!hasValidTarget) // Only log error if we never had a valid target
        {
            Debug.LogError($"No GameObject with tag '{targetTag}' found in the scene.");
            hasValidTarget = false;
        }
    }
}
