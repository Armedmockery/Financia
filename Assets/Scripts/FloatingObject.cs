using UnityEngine;

/// <summary>
/// Attach to any GameObject you want to float/bob in place.
/// Works in both world space (3D) and UI (RectTransform).
///
/// UNITY SETUP:
/// 1. Select your potion GameObject
/// 2. Add this component to it
/// 3. Tweak the float settings in the Inspector
/// </summary>
public class FloatingObject : MonoBehaviour
{
    [Header("Float Settings")]
    public float floatHeight = 0.2f;   // how high it bobs up and down
    public float floatSpeed = 1f;      // how fast it bobs

    //[Header("Rotation Settings")]
    //public bool rotate = true;
    //public float rotationSpeed = 90f;  // degrees per second

    private Vector3 startPosition;

    private void OnEnable()
    {
        // Snap back to origin each time it's enabled
        startPosition = transform.position;
    }

    private void Update()
    {
        // Bob up and down using unscaledTime so it works while game is paused
        float newY = startPosition.y + Mathf.Sin(Time.unscaledTime * floatSpeed) * floatHeight;
        transform.position = new Vector3(startPosition.x, newY, startPosition.z);

        //// Spin on Y axis
        //if (rotate)
        //    transform.Rotate(0f, rotationSpeed * Time.unscaledDeltaTime, 0f);
    }
}