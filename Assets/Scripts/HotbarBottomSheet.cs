using UnityEngine;

public class HotbarBottomSheet : MonoBehaviour
{
    public RectTransform hotbarPanel;
    // public GameObject screenOverlay; // full-screen transparent button
    public GameObject[] buttonsToHide;

    public float slideDuration = 0.25f;
    public float hiddenY = -300f;   // adjust based on UI
    public float shownY = 0f;

    private bool isOpen = false;
    private Coroutine slideRoutine;

    void Start()
    {
        // Start hidden
        Vector2 pos = hotbarPanel.anchoredPosition;
        pos.y = hiddenY;
        hotbarPanel.anchoredPosition = pos;

        // screenOverlay.SetActive(false);
    }

    public void ToggleHotbar()
    {
        if (isOpen)
            CloseHotbar();
        else
            OpenHotbar();
    }

    public void OpenHotbar()
    {
        if (isOpen) return;

        isOpen = true;
        // screenOverlay.SetActive(true);

        foreach (var btn in buttonsToHide)
            btn.SetActive(false);

        StartSlide(shownY);
    }

    public void CloseHotbar()
    {
        if (!isOpen) return;

        isOpen = false;
        // screenOverlay.SetActive(false);

        foreach (var btn in buttonsToHide)
            btn.SetActive(true);

        StartSlide(hiddenY);
    }

    void StartSlide(float targetY)
    {
        if (slideRoutine != null)
            StopCoroutine(slideRoutine);

        slideRoutine = StartCoroutine(SlideTo(targetY));
    }

    System.Collections.IEnumerator SlideTo(float targetY)
    {
        float startY = hotbarPanel.anchoredPosition.y;
        float time = 0f;

        while (time < slideDuration)
        {
            time += Time.deltaTime;
            float t = time / slideDuration;

            float y = Mathf.Lerp(startY, targetY, t);
            hotbarPanel.anchoredPosition =
                new Vector2(hotbarPanel.anchoredPosition.x, y);

            yield return null;
        }

        hotbarPanel.anchoredPosition =
            new Vector2(hotbarPanel.anchoredPosition.x, targetY);
    }
}
