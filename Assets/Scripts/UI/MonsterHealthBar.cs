using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

[RequireComponent(typeof(CanvasGroup))]
public class HealthBarSystem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private HealthSystem healthSystem;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Image healthFill;
    [SerializeField] private Image damageFill;
    [SerializeField] private Transform targetTransform;

    [Header("Settings")]
    [SerializeField] private float fadeSpeed = 4f;
    [SerializeField] private float displayDuration = 3f;
    [SerializeField] private Vector3 offset = new Vector3(0, 1.5f, 0);
    [SerializeField] private float damageFillDelay = 0.5f;

    private Camera _camera;
    private float _timeSinceDamage;
    private Tween _damageTween;

    private void Awake()
    {
        _camera = Camera.main;
        healthSystem.OnHealthChanged += UpdateHealth;
        healthSystem.OnDeath += OnDeath;
    }

    public void Initialize()
    {
        healthSlider.minValue = 0;
        healthSlider.maxValue = healthSystem.GetMaxHealth();
        healthSlider.value = healthSystem.CurrentHealth;
        damageFill.fillAmount = healthSlider.normalizedValue;
    }

    private void UpdateHealth(int current, int max)
    {
        healthSlider.value = current;
        UpdateHealthColor(current, max);

        // Show bar and reset fade timer
        GetComponent<CanvasGroup>().alpha = 1;
        _timeSinceDamage = 0;

        // Animate delayed damage fill
        if (_damageTween != null) _damageTween.Kill();
        _damageTween = DOVirtual.DelayedCall(damageFillDelay, () => {
            damageFill.DOFillAmount(healthSlider.normalizedValue, 0.8f);
        });
    }

    private void UpdateHealthColor(int current, int max)
    {
        float percent = (float)current / max;
        //healthFill.color = Color.Lerp(Color.red, Color.green, percent);
    }

    private void LateUpdate()
    {
        // Billboard effect and fade logic
        transform.position = targetTransform.position + offset;
        transform.rotation = Quaternion.LookRotation(transform.position - _camera.transform.position);

        // Flip the health bar based on monster's direction
        Vector3 localScale = transform.localScale;
        localScale.x = Mathf.Abs(localScale.x) * Mathf.Sign(targetTransform.localScale.x);
        transform.localScale = localScale;

        if ((_timeSinceDamage += Time.deltaTime) > displayDuration)
        {
            GetComponent<CanvasGroup>().alpha = Mathf.MoveTowards(
                GetComponent<CanvasGroup>().alpha,
                0,
                fadeSpeed * Time.deltaTime
            );
        }
    }

    private void OnDeath()
    {
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        healthSystem.OnHealthChanged -= UpdateHealth;
        healthSystem.OnDeath -= OnDeath;
    }
}