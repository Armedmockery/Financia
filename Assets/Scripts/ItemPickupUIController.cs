using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemPickupUIController : MonoBehaviour
{
    public static ItemPickupUIController Instance { get; private set; }

    [Header("Popup Settings")]
    public GameObject popupPrefab;
    public int maxPopups = 5;
    public float popupDuration = 2f;

    private readonly Queue<GameObject> activePopups = new Queue<GameObject>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("Itempickup UI controller available");
        }
        else
        {
            Debug.Log("Multiple ItemPickupUIController instances detected! Destroying the extra one.");
            Destroy(gameObject);
        }
    }

    public void ShowItemPickup(string itemName, Sprite itemIcon = null)
    {
        GameObject newPopup = Instantiate(popupPrefab, transform);

        // Set item name
        TMP_Text text = newPopup.GetComponentInChildren<TMP_Text>();
        if (text != null)
        {
            text.text = itemName;
        }

        // Only set icon if a sprite was provided
        Image itemImage = newPopup.transform.Find("ItemIcon")?.GetComponent<Image>();
        if (itemImage != null)
        {
            itemImage.sprite = itemIcon;
            itemImage.gameObject.SetActive(itemIcon != null); // hide icon slot if no sprite
        }

        activePopups.Enqueue(newPopup);

        // Remove oldest popup if limit exceeded
        if (activePopups.Count > maxPopups)
        {
            Destroy(activePopups.Dequeue());
        }

        NotificationManager.Instance?.Notify();
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
