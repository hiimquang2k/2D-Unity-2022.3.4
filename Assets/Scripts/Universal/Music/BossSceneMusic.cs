using UnityEngine;
public class BossSceneMusic : MonoBehaviour
{
    void Start()
    {
        MusicManager.Instance.PlayTrack("BossTheme");
    }
}