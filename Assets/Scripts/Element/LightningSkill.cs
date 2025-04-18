// LightningSkill.cs
using UnityEngine;

public class LightningSkill : MonoBehaviour
{
    public float teleportRange = 5f;
    public GameObject stunZonePrefab;
    public AudioClip zapSFX;

    public void Activate()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, transform.right, teleportRange);

        if (hit.collider != null && hit.collider.CompareTag("Enemy"))
        {
            ElementStatus status = hit.collider.GetComponent<ElementStatus>();
            if (status != null) status.ApplyElement(Element.Lightning, 3f);
            
            // Teleport to enemy
            transform.position = hit.point;

            // Create stun zone
            Instantiate(stunZonePrefab, transform.position, Quaternion.identity);

            // VFX/SFX
            GetComponent<ParticleSystem>().Play();
            AudioSource.PlayClipAtPoint(zapSFX, transform.position);
        }
    }
}