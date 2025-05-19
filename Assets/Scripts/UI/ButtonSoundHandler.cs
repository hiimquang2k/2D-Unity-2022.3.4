using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class ButtonSoundHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    private static bool initialized = false;
    private Button button;
    private bool isPointerDown = false;

    private void Awake()
    {
        button = GetComponent<Button>();
        
        // Automatically attach to all buttons in the scene
        if (!initialized)
        {
            initialized = true;
            AttachToAllButtons();
        }
    }

    private void AttachToAllButtons()
    {
        Button[] allButtons = FindObjectsOfType<Button>();
        foreach (Button btn in allButtons)
        {
            if (!btn.GetComponent<ButtonSoundHandler>())
            {
                btn.gameObject.AddComponent<ButtonSoundHandler>();
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isPointerDown)
        {
            AudioManager.Instance.PlayButtonHoverSound();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isPointerDown = false;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isPointerDown = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (isPointerDown)
        {
            AudioManager.Instance.PlayButtonPressSound();
            isPointerDown = false;
        }
    }
}
