using UnityEngine;

public interface IInteractable
{
    void Interact(GameObject interactor);
    bool IsInRange(Vector3 playerPosition, out int priority);
}
