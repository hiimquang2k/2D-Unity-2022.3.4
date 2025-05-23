using UnityEngine;
public class Level2MusicController : MonoBehaviour
{
    void Start()
    {
        MusicManager.Instance.PlayTrack("Level2Theme");
    }
}