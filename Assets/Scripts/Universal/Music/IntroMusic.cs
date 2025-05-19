using UnityEngine;
public class IntroMusicController : MonoBehaviour
{
    void Start()
    {
        MusicManager.Instance.PlayTrack("IntroTheme");
    }
}