using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PopupsScript : MonoBehaviour
{
    public static PopupsScript Instance { get; private set; }

    [Header("Popup Settings")]
    public GameObject popupErrorPrefab;
    public int maxPopups = 3;
    public float popupDuration = 2f;

    private readonly Queue<GameObject> activePopups = new Queue<GameObject>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogError("Multiple ItemPickupUIController instances detected! Destroying the extra one.");
            Destroy(gameObject);
        }
    }

    public void ShowItemPickup(string message)
    {
        GameObject newPopup = Instantiate(popupErrorPrefab, transform);

        // Set item name
        TMP_Text text = newPopup.GetComponentInChildren<TMP_Text>();
        if (text != null)
        {
            text.text = message;
        }

        activePopups.Enqueue(newPopup);

        // Remove oldest popup if limit exceeded
        if (activePopups.Count > maxPopups)
        {
            Destroy(activePopups.Dequeue());
        }

        StartCoroutine(FadeOutAndDestroy(newPopup));
    }

    private IEnumerator FadeOutAndDestroy(GameObject popup)
    {
        yield return new WaitForSeconds(popupDuration);

        if (popup == null) yield break;

        CanvasGroup canvasGroup = popup.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = popup.AddComponent<CanvasGroup>();
        }

        float timePassed = 0f;
        while (timePassed < 1f)
        {
            if (popup == null) yield break;

            canvasGroup.alpha = 1f - timePassed;
            timePassed += Time.deltaTime;
            yield return null;
        }

        Destroy(popup);
    }
}
