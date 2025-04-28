using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class StatusEffectManager : MonoBehaviour
{
    [Header("Status Display")]
    [SerializeField] private GameObject statusTextPrefab;
    [SerializeField] private Vector3 statusOffset = new Vector3(0, 2f, 0);
    [SerializeField] private float textFadeDuration = 0.5f;

    [Header("Fire Effects")]
    [SerializeField] private Color fireTextColor = new Color(1f, 0.3f, 0f);

    [Header("Lightning Effects")]
    [SerializeField] private Color lightningTextColor = new Color(0.2f, 0.8f, 1f);

    private Dictionary<GameObject, ActiveStatus> activeStatuses = new Dictionary<GameObject, ActiveStatus>();

    private class ActiveStatus
    {
        public GameObject statusText;
        public ParticleSystem effectParticles;
        public Coroutine removalRoutine;
        public string originalTag;
    }

    private void Awake()
    {
        // Auto-configure text prefab if null
        if (statusTextPrefab == null) CreateDefaultTextPrefab();
    }

    private void CreateDefaultTextPrefab()
    {
        statusTextPrefab = new GameObject("StatusText");
        var text = statusTextPrefab.AddComponent<TextMeshPro>();
        text.fontSize = 3;
        text.alignment = TextAlignmentOptions.Center;
        text.sortingOrder = 100;
        
        // Save as prefab in editor
        #if UNITY_EDITOR
        UnityEditor.PrefabUtility.SaveAsPrefabAsset(statusTextPrefab, "Assets/StatusText.prefab");
        Destroy(statusTextPrefab);
        #endif
    }

    public void ApplyStatus(GameObject target, DamageType type)
    {
        if (target == null) return;

        // Clean up existing status
        if (activeStatuses.ContainsKey(target))
        {
            RemoveStatus(target);
        }

        // Create new status
        var status = new ActiveStatus
        {
            originalTag = target.tag
        };

        // Create visual elements
        status.statusText = CreateStatusText(target, type);

        // Set tag based on element
        target.tag = type == DamageType.Fire ? "Burning" : "Electrified";

        // Auto-remove after duration
        status.removalRoutine = StartCoroutine(RemoveAfterDelay(
            target,
            type == DamageType.Fire ? 3f : 2f
        ));

        activeStatuses.Add(target, status);
    }

private GameObject CreateStatusText(GameObject target, DamageType type)
{
    if (target == null) return null;

    // Create text in canvas space instead of world space
    GameObject textObj = Instantiate(statusTextPrefab, target.transform);
    textObj.transform.localPosition = statusOffset;
    
    // These two lines are the magic fix:
    textObj.transform.localRotation = Quaternion.identity;
    textObj.transform.localScale = Vector3.one;
    
    // Force upright text regardless of parent
    var text = textObj.GetComponent<TextMeshPro>();
    if (text == null) text = textObj.AddComponent<TextMeshPro>();
    
    text.text = type == DamageType.Fire ? "BURNING" : "ELECTRIFIED";
    text.color = type == DamageType.Fire ? fireTextColor : lightningTextColor;

    StartCoroutine(FadeText(text, 0f, 1f, textFadeDuration));
    return textObj;
}
    private IEnumerator RemoveAfterDelay(GameObject target, float delay)
    {
        yield return new WaitForSeconds(delay);
        RemoveStatus(target);
    }

    public void RemoveStatus(GameObject target)
    {
        if (!activeStatuses.TryGetValue(target, out ActiveStatus status)) return;

        // Fade out text before removal
        if (status.statusText != null)
        {
            TextMeshPro text = status.statusText.GetComponent<TextMeshPro>();
            StartCoroutine(FadeText(text, 1f, 0f, textFadeDuration, () => 
            {
                Destroy(status.statusText);
            }));
        }

        // Stop and remove particles
        if (status.effectParticles != null)
        {
            status.effectParticles.Stop();
            Destroy(status.effectParticles.gameObject, status.effectParticles.main.duration);
        }

        // Restore original tag
        target.tag = status.originalTag;

        // Clean up
        if (status.removalRoutine != null) StopCoroutine(status.removalRoutine);
        activeStatuses.Remove(target);
    }

    private IEnumerator FadeText(TextMeshPro text, float startAlpha, float endAlpha, float duration, System.Action onComplete = null)
    {
        float timer = 0f;
        Color color = text.color;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            color.a = Mathf.Lerp(startAlpha, endAlpha, timer / duration);
            text.color = color;
            yield return null;
        }

        onComplete?.Invoke();
    }

    // Debug visualization
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        foreach (var pair in activeStatuses)
        {
            if (pair.Key != null)
            {
                Gizmos.DrawWireSphere(pair.Key.transform.position + statusOffset, 0.3f);
            }
        }
    }
}