using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class JoystickBackgroundFader : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [Header("Settings")]
    [SerializeField] private float activeAlpha = 1f;
    [SerializeField] private float inactiveAlpha = 0.3f;
    [SerializeField] private float fadeSpeed = 5f;
    
    private CanvasGroup bgCanvasGroup;
    private bool isDragging = false;
    
    void Start()
    {
        // Find the parent's "bg" object
        Transform parentBg = transform.parent.Find("bg");
        if (parentBg != null)
        {
            bgCanvasGroup = parentBg.GetComponent<CanvasGroup>();
            if (bgCanvasGroup == null)
                bgCanvasGroup = parentBg.gameObject.AddComponent<CanvasGroup>();
                
            bgCanvasGroup.alpha = activeAlpha;
            Debug.Log("Found bg through parent!");
        }
        else
        {
            Debug.LogError("Could not find 'bg' in parent!");
        }
    }
    
    void Update()
    {
        if (bgCanvasGroup != null)
        {
            float targetAlpha = isDragging ? inactiveAlpha : activeAlpha;
            bgCanvasGroup.alpha = Mathf.Lerp(bgCanvasGroup.alpha, targetAlpha, Time.deltaTime * fadeSpeed);
        }
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        isDragging = true;
        Debug.Log("DOT: Pointer Down");
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        isDragging = true;
    }
    
    public void OnPointerUp(PointerEventData eventData)
    {
        isDragging = false;
        Debug.Log("DOT: Pointer Up");
    }
}