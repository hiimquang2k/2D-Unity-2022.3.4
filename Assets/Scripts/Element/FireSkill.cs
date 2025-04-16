// FireSkill.cs
using UnityEngine;
using System.Collections;

public class FireSkill : MonoBehaviour
{
    public float respawnDuration = 3f;
    public ParticleSystem flameTrail;
    public AudioClip igniteSFX;

    public void Activate()
    {
        StartCoroutine(RespawnRoutine());
    }

    IEnumerator RespawnRoutine()
    {
        Vector3 respawnPoint = transform.position;
        GetComponent<HealthSystem>().isInvulnerable = true;

        // VFX
        flameTrail.Play();

        yield return new WaitForSeconds(respawnDuration);

        // SFX
        AudioSource.PlayClipAtPoint(igniteSFX, transform.position);
        GetComponent<HealthSystem>().isInvulnerable = false;
    }
}