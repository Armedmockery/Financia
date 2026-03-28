using System.Collections.Generic;
using UnityEngine;

public class PlayerItemCollector : MonoBehaviour
{
    private InventoryController inventoryController;
    private HashSet<Collider2D> pendingPickups = new HashSet<Collider2D>();

    void Start()
    {
        inventoryController = FindFirstObjectByType<InventoryController>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Item"))
        {
            pendingPickups.Add(other);
            TryPickup(other.gameObject);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Item"))
        {
            pendingPickups.Remove(other);
        }
    }

    private void Update()
    {
        // Process any items still in the trigger (e.g., if they became active while player was standing on them)
        foreach (var col in new List<Collider2D>(pendingPickups))
        {
            if (col != null && col.gameObject.activeInHierarchy)
            {
                TryPickup(col.gameObject);
            }
            else
            {
                pendingPickups.Remove(col);
            }
        }
    }

    public void TryPickup(GameObject itemObj)
    {
        Item item = itemObj.GetComponent<Item>();
        if (item == null) return;

        // Optional: log to see attempts
        Debug.Log($"Attempting to pick up {item.name} (ID: {item.ID})");

        bool added = inventoryController.AddItem(itemObj); // uses original AddItem
        Debug.Log($"AddItem returned: {added}");
        if (added)
        {
            item.PickUp();
            // Remove from pending set (the collider will be destroyed soon)
            Collider2D col = itemObj.GetComponent<Collider2D>();
            if (col != null) pendingPickups.Remove(col);
            Debug.Log($"Destroying item {itemObj.name}");
            SaveController.Instance.AddCollectedItem(item.instanceID, item.ID);
            // Destroy the world item
            Destroy(itemObj);
        }
    }
}