using UnityEngine;
using TMPro;
using Cinemachine;
using System.Collections;

public class BossIntroManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject bossNameCanvas;
    [SerializeField] private TextMeshProUGUI bossNameText;
    [SerializeField] private CinemachineVirtualCamera bossCamera;
    [SerializeField] private ImprovedCameraShake cameraShake;
    [SerializeField] private AudioClip bossIntroSFX;

    [Header("Settings")]
    [SerializeField] private string bossName = "THE SOUL DEVOURER";
    [SerializeField] private float introDuration = 4f;
    [SerializeField] private float nameDisplayDelay = 0.5f;
    [SerializeField] private float cameraShakeIntensity = 0.5f;

    private AudioSource audioSource;
    private CinemachineVirtualCamera mainCamera;
    private PlayerMovement playerMovement;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        bossNameCanvas.SetActive(false);
        
        if (bossCamera != null)
            bossCamera.Priority = 0;
    }

    private void Start()
    {
        mainCamera = Camera.main.GetComponent<CinemachineVirtualCamera>();
        playerMovement = FindObjectOfType<PlayerMovement>();
    }

    public void StartBossIntro()
    {
        StartCoroutine(BossIntroSequence());
    }

    private IEnumerator BossIntroSequence()
    {
        // Freeze player
        if (playerMovement != null)
            playerMovement.LockMovement(true);

        // Switch to boss camera
        if (bossCamera != null)
        {
            bossCamera.Priority = 20;
            yield return new WaitForSeconds(0.5f); // Camera transition time
        }

        // Play SFX
        if (bossIntroSFX != null)
            audioSource.PlayOneShot(bossIntroSFX);

        // Shake camera
        if (cameraShake != null)
            cameraShake.ShakeCamera(cameraShakeIntensity);

        // Show boss name
        yield return new WaitForSeconds(nameDisplayDelay);
        StartCoroutine(ShowBossName());

        // Change music
        //MusicManager.Instance.PlayTrack("BossTheme");

        // Wait for intro duration
        yield return new WaitForSeconds(introDuration);

        // Restore camera
        if (bossCamera != null)
            bossCamera.Priority = 0;

        // Restore control
        if (playerMovement != null)
            playerMovement.LockMovement(false);
    }

    private IEnumerator ShowBossName()
    {
        bossNameText.text = bossName;
        bossNameCanvas.SetActive(true);
        
        CanvasGroup cg = bossNameCanvas.GetComponent<CanvasGroup>();
        float fadeTime = 0.5f;
        
        // Fade in
        for (float t = 0; t < fadeTime; t += Time.deltaTime)
        {
            cg.alpha = t / fadeTime;
            yield return null;
        }
        
        // Hold
        yield return new WaitForSeconds(2f);
        
        // Fade out
        for (float t = fadeTime; t > 0; t -= Time.deltaTime)
        {
            cg.alpha = t / fadeTime;
            yield return null;
        }
        
        bossNameCanvas.SetActive(false);
    }
}