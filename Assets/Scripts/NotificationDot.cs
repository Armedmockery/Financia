using UnityEngine;

/// <summary>
/// Attach ONCE to your main menu button.
/// Shows/hides a green dot child whenever any notification fires.
///
/// UNITY SETUP:
/// 1. Select your menu button in the Hierarchy
/// 2. Add this component to it
/// 3. Create a child Image on the button (name it "NotifDot")
///    - Sprite: circle, Color: green
///    - Anchor: top-right corner of the button
///    - Size: ~20x20 px
/// 4. Drag that child Image into the "Dot Object" field on this component
/// 5. Drag this button into NotificationManager's "Menu Dot" field
/// </summary>
public class NotificationDot : MonoBehaviour
{
    [Header("Dot Reference")]
    [Tooltip("The small green dot child Image GameObject on this button")]
    public GameObject dotObject;

    [Header("Pulse Animation")]
    public bool animatePulse = true;
    public float pulseSpeed = 2f;
    public float pulseMinScale = 0.85f;
    public float pulseMaxScale = 1.15f;

    private bool isVisible;

    private void Awake()
    {
        if (dotObject != null)
            dotObject.SetActive(false);
    }

    private void Update()
    {
        if (!isVisible || !animatePulse || dotObject == null) return;

        float scale = Mathf.Lerp(
            pulseMinScale,
            pulseMaxScale,
            (Mathf.Sin(Time.unscaledTime * pulseSpeed) + 1f) / 2f
        );
        dotObject.transform.localScale = Vector3.one * scale;
    }

    public void Show()
    {
        if (dotObject == null) return;
        isVisible = true;
        dotObject.SetActive(true);
        dotObject.transform.localScale = Vector3.one;
    }

    public void Hide()
    {
        if (dotObject == null) return;
        isVisible = false;
        dotObject.SetActive(false);
    }
}