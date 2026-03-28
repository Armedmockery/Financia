using UnityEngine;

public class PhoneButtonToggle : MonoBehaviour
{
    public GameObject phoneObject;

    [Tooltip("Drag the PhoneNotificationPopup GameObject here")]
    public PhoneNotificationPopup notificationPopup;

    private bool isPhoneOpen = false;

    public void TogglePhone()
    {
        isPhoneOpen = !isPhoneOpen;
        phoneObject.SetActive(isPhoneOpen);

        // When phone opens, check if a cutscene left a notification
        if (isPhoneOpen && CutSceneManager.HasPendingPhoneNotification)
        {
            notificationPopup?.Show();
            CutSceneManager.ClearPhoneNotification();  // clear so it only shows once
        }

    }
}
