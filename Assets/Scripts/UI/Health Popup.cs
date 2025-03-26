using UnityEngine;
using TMPro;
using System.Collections;

public class HealthPopup : MonoBehaviour
{
    [Header("Visual Settings")]
    [SerializeField] private float lifetime = 1f;
    [SerializeField] private float moveSpeed = 1.5f;
    [SerializeField] private float fadeSpeed = 3f;
    
    [Header("Text Settings")]
    [SerializeField] private Color damageColor = Color.red;
    [SerializeField] private Color healColor = Color.green;
    [SerializeField] private Color criticalColor = new Color(1f, 0.5f, 0f); // Orange for critical
    [SerializeField] private float criticalSizeMultiplier = 1.5f;
    
    private TextMeshPro textMesh;
    private float initialScale;
    private Color textColor;
    private float timeAlive;
    private bool isCritical;

    private void Awake()
    {
        textMesh = GetComponent<TextMeshPro>();
        
        if (textMesh == null)
        {
            Debug.LogError("TextMeshPro component missing on HealthPopup!");
            Destroy(gameObject);
            return;
        }
        
        initialScale = transform.localScale.x;
        textColor = textMesh.color;
    }

    private void Update()
    {
        transform.position += new Vector3(0, moveSpeed * Time.deltaTime, 0);
        
        // Count time alive
        timeAlive += Time.deltaTime;
        
        // Fade out
        textColor.a = Mathf.Lerp(textColor.a, 0, fadeSpeed * Time.deltaTime);
        textMesh.color = textColor;
        
        // Destroy when time is up
        if (timeAlive >= lifetime)
        {
            Destroy(gameObject);
        }
    }

    public void SetValue(int value, bool critical = false)
    {
        isCritical = critical;
        
        // Format the text with sign for healing
        if (value > 0)
        {
            textMesh.text = "+" + value.ToString();
            textMesh.color = healColor;
        }
        else if (value < 0)
        {
            // Remove negative sign and add a minus
            textMesh.text = "-" + Mathf.Abs(value).ToString();
            textMesh.color = critical ? criticalColor : damageColor;
            
            // Add exclamation for critical hits
            if (critical)
            {
                textMesh.text += "!";
            }
        }
        else
        {
            textMesh.text = value.ToString();
            textMesh.color = Color.white;
        }
        
        textColor = textMesh.color;
        
        // Set initial scale - larger for critical hits
        if (critical)
        {
            transform.localScale = Vector3.one * initialScale * criticalSizeMultiplier;
        }
    }
}
