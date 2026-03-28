using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InventoryController : MonoBehaviour
{

    private ItemDictionary itemDictionary;

    public GameObject inventoryPanel;
    public GameObject slotPrefab;
    public int slotCount;
    public GameObject[] itemPrefabs;

    public static InventoryController Instance {get; private set;}
    Dictionary<int,int> itemsCountCache = new();
    public UnityEvent OnInventoryChanged; //event to notify the quest system or any other which needs to know inventory changed or not . 

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            //DontDestroyOnLoad(gameObject); // Optional but helpful
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    // Start is called before the first frame update
    void Start()
    {

        if (itemDictionary == null)
        {
            itemDictionary = FindFirstObjectByType<ItemDictionary>();
            RebuildItemCounts();
        }

        // Only create slots if panel exists and has no children
        if (inventoryPanel != null && inventoryPanel.transform.childCount == 0)
        {
            for (int i = 0; i < slotCount; i++)
                Instantiate(slotPrefab, inventoryPanel.transform);
        }

        RebuildItemCounts();  // Rebuild after slots are ready
        // for (int i = 0; i < slotCount; i++)
        // {
        //     itemDictionary=FindObjectOfType<ItemDictionary>();

        //     Slot slot = Instantiate(
        //         slotPrefab,
        //         inventoryPanel.transform
        //     ).GetComponent<Slot>();

        //     if (i < itemPrefabs.Length)
        //     {
        //         GameObject item = Instantiate(
        //             itemPrefabs[i],
        //             slot.transform
        //         );

        //         item.GetComponent<RectTransform>().anchoredPosition =
        //             Vector2.zero;

        //         slot.currentItem = item;
        //     }
        // }
    }

    public void RebuildItemCounts()
    {
        Debug.Log($"=== RebuildItemCounts START ===");
        itemsCountCache.Clear();

        foreach (Transform slotTransform in inventoryPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();
            if (slot.currentItem != null)
            {
                Item item = slot.currentItem.GetComponent<Item>();
                if (item != null)
                {
                    itemsCountCache.TryGetValue(item.ID, out int currentCount);
                    itemsCountCache[item.ID] = currentCount + item.quantity;
                    slot.currentItem.SetActive(true);
                    Debug.Log($"  Item {item.ID}: count {currentCount + item.quantity}");
                }
            }
        }
        Debug.Log("Invoking OnInventoryChanged");
        OnInventoryChanged?.Invoke();
    }

    public Dictionary<int , int> GetItemCounts()=>itemsCountCache;

    //public bool AddItem(GameObject itemPrefab)
    //{
    //    Item itemToAdd= itemPrefab.GetComponent<Item>();
    //    if(itemToAdd==null) return false;

    //    itemToAdd.gameObject.SetActive(true);

    //    // Check if we have this item type in inventory
    //    foreach (Transform slotTranform in inventoryPanel.transform)
    //    {
    //        Slot slot = slotTranform.GetComponent<Slot>();

    //        if (slot != null && slot.currentItem != null)
    //        {
    //            Debug.Log("Inventory slot found!");
    //            Item slotItem = slot.currentItem.GetComponent<Item>();
    //            Debug.Log($"slotItem is null? {slotItem == null}");
    //            if (slotItem != null)
    //            {
    //                Debug.Log($"slotItem.ID = {slotItem.ID}, itemToAdd.ID = {itemToAdd.ID}");
    //            }
    //            if (slotItem != null && slotItem.ID == itemToAdd.ID)
    //            {
    //                Debug.Log("Condition passed, entering stacking block.");
    //                try
    //                {
    //                    // Same item, stack them
    //                    slotItem.AddToStack();
    //                    Debug.Log("Inventory stack found!");
    //                    slotItem.gameObject.SetActive(true);
    //                    itemToAdd.PickUp();
    //                    RebuildItemCounts();
    //                    Debug.Log("Inventory item added to stack!");
    //                    return true;
    //                }
    //                catch (System.Exception e)
    //                {
    //                    Debug.LogError($"Exception during stacking: {e}");
    //                    return false;
    //                }
    //            }
    //            else
    //            {
    //                Debug.Log("Condition failed: slotItem null or ID mismatch");
    //            }
    //        }
    //    }

    // In InventoryController.cs
    public bool AddItem(GameObject worldItem)
    {
        Item itemToAdd = worldItem.GetComponent<Item>();
        if (itemToAdd == null) return false;

        // Try to stack with existing items
        foreach (Transform slotTransform in inventoryPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();
            if (slot.currentItem != null)
            {
                Item slotItem = slot.currentItem.GetComponent<Item>();
                if (slotItem != null && slotItem.ID == itemToAdd.ID)
                {
                    // Stack
                    slotItem.AddToStack(itemToAdd.quantity);
                    RebuildItemCounts();
                    return true;   // caller will destroy world item and record collection
                }
            }
        }

        // No stack found � look for an empty slot
        foreach (Transform slotTransform in inventoryPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();
            if (slot.currentItem == null)
            {
                // Get the prefab from the dictionary using the item's ID
                GameObject prefab = itemDictionary.GetItemPrefab(itemToAdd.ID);
                if (prefab == null)
                {
                    Debug.LogError($"No prefab found for item ID {itemToAdd.ID}");
                    return false;
                }

                GameObject newItem = Instantiate(prefab, slotTransform);
                newItem.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                newItem.SetActive(true);
                slot.currentItem = newItem;

                // If the world item represented a stack, copy its quantity
                Item newItemComp = newItem.GetComponent<Item>();
                if (newItemComp != null && itemToAdd.quantity > 1)
                {
                    newItemComp.quantity = itemToAdd.quantity;
                    newItemComp.UpdateQuantityDisplay();
                }

                RebuildItemCounts();
                return true;
            }
        }

        Debug.Log("Inventory is full!");
        return false;
    }

    //public List<InventorySaveData> GetInventoryItems()
    //{
    //    HashSet<int> usedSlots = new HashSet<int>();
    //    List<InventorySaveData> invData = new List<InventorySaveData>();
    //    foreach (Transform slotTransform in inventoryPanel.transform)
    //    {
    //        int index = slotTransform.GetSiblingIndex();
    //        if (usedSlots.Contains(index))
    //        {
    //            Debug.LogError($"Duplicate slot index {index} detected in inventory panel!");
    //            continue;
    //        }
    //        usedSlots.Add(index);

    //        Slot slot = slotTransform.GetComponent<Slot>();
    //        if (slot.currentItem != null)
    //        {
    //            Item item = slot.currentItem.GetComponent<Item>();
    //            invData.Add(new InventorySaveData(item.ID, index, item.quantity));
    //        }
    //    }
    //    return invData;
    //}

    public List<InventorySaveData> GetInventoryItems()
    {
        HashSet<int> usedSlots = new HashSet<int>();
        List<InventorySaveData> invData = new List<InventorySaveData>();
        foreach (Transform slotTransform in inventoryPanel.transform)
        {
            int index = slotTransform.GetSiblingIndex();
            Slot slot = slotTransform.GetComponent<Slot>();
            if (slot == null)
            {
                Debug.LogError($"Slot component missing on slot at index {index}");
                continue;
            }
            if (slot.currentItem != null)
            {
                Item item = slot.currentItem.GetComponent<Item>();
                if (item != null)
                {
                    invData.Add(new InventorySaveData(item.ID, index, item.quantity));
                    Debug.Log($"Slot {index}: found item ID {item.ID} (qty {item.quantity})");
                }
                else
                {
                    Debug.LogWarning($"Slot {index}: currentItem has no Item component!");
                }
            }
            else
            {
                Debug.Log($"Slot {index}: empty");
            }
        }
        Debug.Log($"GetInventoryItems: total items found = {invData.Count}");
        return invData;
    }

    public void SetInventoryItems(List<InventorySaveData> inventorySaveData)
    {
        try
        {
            Debug.Log($"SetInventoryItems called with {inventorySaveData?.Count} items.");

            if (inventoryPanel == null) { Debug.LogError("inventoryPanel is NULL"); return; }
            if (itemDictionary == null)
            {
                itemDictionary = FindFirstObjectByType<ItemDictionary>();
                if (itemDictionary == null) { Debug.LogError("ItemDictionary not found"); return; }
            }

            // Clear existing items
            Debug.Log("Step A: Clearing slots");
            foreach (Transform slotTransform in inventoryPanel.transform)
            {
                Slot slot = slotTransform.GetComponent<Slot>();
                if (slot == null) { Debug.LogError($"Slot component missing on {slotTransform.name}"); continue; }
                if (slot.currentItem != null)
                {
                    Destroy(slot.currentItem);
                    slot.currentItem = null;
                }
            }

            Debug.Log("Step B: Populating slots");
            foreach (InventorySaveData data in inventorySaveData)
            {
                Debug.Log($"Step B1: slotIndex={data.slotIndex}, slotCount={slotCount}");
                if (data.slotIndex < 0 || data.slotIndex >= inventoryPanel.transform.childCount)
                {
                    Debug.LogWarning($"Slot index {data.slotIndex} out of range");
                    continue;
                }

                Debug.Log("Step B2: Getting slot transform");
                Transform slotT = inventoryPanel.transform.GetChild(data.slotIndex);

                Debug.Log("Step B3: Getting Slot component");
                Slot slot = slotT.GetComponent<Slot>();
                if (slot == null) { Debug.LogError($"No Slot component at index {data.slotIndex}"); continue; }

                Debug.Log("Step B4: Getting prefab");
                GameObject prefab = itemDictionary.GetItemPrefab(data.itemID);
                if (prefab == null) { Debug.LogError($"No prefab for ID {data.itemID}"); continue; }

                Debug.Log("Step B5: Instantiating");
                GameObject newItem = Instantiate(prefab, slotT);
                newItem.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                newItem.SetActive(true);

                Item itemComp = newItem.GetComponent<Item>();
                if (itemComp != null && data.quantity > 1)
                {
                    itemComp.quantity = data.quantity;
                    itemComp.UpdateQuantityDisplay();
                }

                slot.currentItem = newItem;
                Debug.Log($"Step B6: Done - restored item {data.itemID} in slot {data.slotIndex}");
            }

            Debug.Log("Step C: RebuildItemCounts");
            RebuildItemCounts();
            Debug.Log("SetInventoryItems completed.");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"SetInventoryItems CRASHED: {e.Message}\n{e.StackTrace}");
        }
    }
    public void RemoveItemsFromInventory(int itemID, int amountToRemove)
    {
       
        Debug.Log($"RemoveItemsFromInventory called: ID={itemID}, Amount={amountToRemove}");
        foreach (Transform slotTransform in inventoryPanel.transform)
        {
            if (amountToRemove <= 0) break;

            Slot slot = slotTransform.GetComponent<Slot>();
            if(slot?.currentItem?.GetComponent<Item>() is Item item && item.ID == itemID)
            {
                int removed = Mathf.Min(amountToRemove, item.quantity);
                Debug.Log($"Removing {removed} from stack");
                int temp = item.RemoveFromStackCopy(removed);
                amountToRemove -= removed;

                if(item.quantity == 0)
                {
                    Destroy(slot.currentItem);
                    slot.currentItem = null;
                }
            }
        }
        RebuildItemCounts();   
    }
}
