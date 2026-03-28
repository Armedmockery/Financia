using UnityEngine;
using TMPro;

public class PhoneNotificationPopup : MonoBehaviour
{

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}