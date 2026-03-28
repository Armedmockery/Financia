using UnityEngine;

public class MenuController : MonoBehaviour
{
    [SerializeField] private GameObject menuUI;
    [SerializeField] private GameObject hudUI;

    private bool isOpen = false;

    void Start()
    {
        if (menuUI != null)
            menuUI.SetActive(false);

        if (hudUI != null)
            hudUI.SetActive(true);

        Time.timeScale = 1f;
    }

    public void ToggleMenu()
    {
        if (menuUI == null) return;

        isOpen = !isOpen;
        menuUI.SetActive(isOpen);

        if (hudUI != null)
            hudUI.SetActive(!isOpen);

        Time.timeScale = isOpen ? 0f : 1f;

        if (isOpen)
            NotificationManager.Instance?.Clear();
    }
}
