using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [System.Serializable]
    public class MusicTrack
    {
        public string name;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 0.7f;
        public int priority = 0; // Higher = more important
    }

    [Header("Settings")]
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private bool shufflePlaylist = false;
    [SerializeField] private List<MusicTrack> musicLibrary = new List<MusicTrack>();

    private AudioSource[] sources = new AudioSource[2];
    private int currentSourceIndex = 0;
    private Coroutine activeFade;
    private MusicTrack currentTrack;
    private List<MusicTrack> playlist = new List<MusicTrack>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Initialize();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Initialize()
    {
        // Create two AudioSources for crossfading
        for (int i = 0; i < 2; i++)
        {
            sources[i] = gameObject.AddComponent<AudioSource>();
            sources[i].loop = true;
            sources[i].playOnAwake = false;
            sources[i].volume = 0f;
        }

        // Create default playlist from library
        UpdatePlaylist();
    }

    public void PlayTrack(string trackName, bool immediate = false)
    {
        MusicTrack track = musicLibrary.Find(t => t.name == trackName);
        if (track != null)
        {
            Play(track, immediate);
        }
        else
        {
            Debug.LogWarning($"Track '{trackName}' not found in library");
        }
    }

    public void PlayRandom(bool immediate = false)
    {
        if (musicLibrary.Count > 0)
        {
            MusicTrack track = musicLibrary[Random.Range(0, musicLibrary.Count)];
            Play(track, immediate);
        }
    }

    public void PlayNextInPlaylist(bool immediate = false)
    {
        if (playlist.Count == 0) return;

        if (shufflePlaylist && playlist.Count > 1)
        {
            // Get random track that isn't the current one
            MusicTrack nextTrack;
            do {
                nextTrack = playlist[Random.Range(0, playlist.Count)];
            } while (playlist.Count > 1 && nextTrack == currentTrack);

            Play(nextTrack, immediate);
        }
        else
        {
            int currentIndex = playlist.IndexOf(currentTrack);
            int nextIndex = (currentIndex + 1) % playlist.Count;
            Play(playlist[nextIndex], immediate);
        }
    }

    private void Play(MusicTrack track, bool immediate)
    {
        // Don't interrupt higher priority tracks
        if (currentTrack != null && currentTrack.priority > track.priority)
            return;

        currentTrack = track;
        
        if (activeFade != null)
            StopCoroutine(activeFade);

        activeFade = StartCoroutine(CrossFade(track, immediate ? 0f : fadeDuration));
    }

    private IEnumerator CrossFade(MusicTrack track, float duration)
    {
        int nextSourceIndex = 1 - currentSourceIndex;
        AudioSource currentSource = sources[currentSourceIndex];
        AudioSource nextSource = sources[nextSourceIndex];

        // Set up new track
        nextSource.clip = track.clip;
        nextSource.volume = 0f;
        nextSource.Play();

        float timer = 0f;
        float currentStartVolume = currentSource.volume;
        float nextTargetVolume = track.volume;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;

            currentSource.volume = Mathf.Lerp(currentStartVolume, 0f, t);
            nextSource.volume = Mathf.Lerp(0f, nextTargetVolume, t);
            yield return null;
        }

        // Complete transition
        currentSource.Stop();
        currentSource.volume = 0f;
        nextSource.volume = nextTargetVolume;
        currentSourceIndex = nextSourceIndex;
    }

    public void StopMusic(bool fade = true)
    {
        if (activeFade != null)
            StopCoroutine(activeFade);

        currentTrack = null;
        activeFade = StartCoroutine(FadeOut(fade ? fadeDuration : 0f));
    }

    private IEnumerator FadeOut(float duration)
    {
        AudioSource currentSource = sources[currentSourceIndex];
        float startVolume = currentSource.volume;

        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            currentSource.volume = Mathf.Lerp(startVolume, 0f, timer / duration);
            yield return null;
        }

        currentSource.Stop();
        currentSource.volume = 0f;
    }

    public void UpdatePlaylist()
    {
        playlist = new List<MusicTrack>(musicLibrary);
        if (shufflePlaylist)
        {
            // Fisher-Yates shuffle
            for (int i = 0; i < playlist.Count; i++)
            {
                MusicTrack temp = playlist[i];
                int randomIndex = Random.Range(i, playlist.Count);
                playlist[i] = playlist[randomIndex];
                playlist[randomIndex] = temp;
            }
        }
    }

    public void SetVolume(float volume, bool affectCurrentTrack = true)
    {
        if (affectCurrentTrack && currentTrack != null)
        {
            currentTrack.volume = volume;
            sources[currentSourceIndex].volume = volume;
        }
    }
}