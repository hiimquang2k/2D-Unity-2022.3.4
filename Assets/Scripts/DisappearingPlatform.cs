using UnityEngine;
using System.Collections;
using UnityEngine.Tilemaps;
public class DisappearingPlatform : MonoBehaviour
{
    [SerializeField] private float reappearTime = 3f;
    [SerializeField] private float disappearTime = 1.0f;
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("On collision with Player.");
            StartCoroutine(DisablePlatform());
        }
    }

    private IEnumerator DisablePlatform()
    {
        yield return new WaitForSeconds(disappearTime);
        GetComponent<Collider2D>().enabled = false;
        GetComponent<TilemapRenderer>().enabled = false;
        yield return new WaitForSeconds(reappearTime);
        GetComponent<Collider2D>().enabled = true;
        GetComponent<TilemapRenderer>().enabled = true;
    }
}