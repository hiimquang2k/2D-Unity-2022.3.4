using UnityEngine;
public class Level1MusicController : MonoBehaviour
{
    void Start()
    {
        MusicManager.Instance.PlayTrack("Level1Theme");
    }
}