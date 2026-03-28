using UnityEngine;

/// <summary>
/// Wires all game events to the single NotificationDot on your menu button.
///
/// UNITY SETUP:
/// 1. Create an empty GameObject, name it "NotificationManager"
/// 2. Add this component to it
/// 3. Drag your menu button (which has NotificationDot on it) into "Menu Dot"
/// </summary>
public class NotificationManager : MonoBehaviour
{
    public static NotificationManager Instance { get; private set; }

    [Tooltip("The menu button that has the NotificationDot component on it")]
    public NotificationDot menuDot;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            //DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        //// Hooks into InventoryController's existing UnityEvent automatically
        //if (InventoryController.Instance != null)
        //    InventoryController.Instance.OnInventoryChanged.AddListener(Notify);
        //else
        //    Debug.LogWarning("NotificationManager: InventoryController not ready at Start.");
    }

    /// <summary>Shows the dot. Called by any game system that has a notification.</summary>
    public void Notify() => menuDot?.Show();

    /// <summary>
    /// Call this when the player opens the menu so the dot clears.
    /// e.g. NotificationManager.Instance?.Clear();
    /// </summary>
    public void Clear() => menuDot?.Hide();
}