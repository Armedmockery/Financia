using UnityEngine;
using TMPro;
using UnityEngine.UI;


public class Item : MonoBehaviour
{
    public int ID;
    public string Name;
    public int quantity=1;
    private TMP_Text quantityText;
      // will hold a GUID

    [Header("Quest Association")]
    public string requiredQuestID;
    public string instanceID; // empty = always active

    private void Start()
    {
        // If this item belongs to a quest, disable it unless that quest is active
        if (!string.IsNullOrEmpty(requiredQuestID))
        {
            // Check if the item is inside the inventory panel (any depth)
            bool inInventory = InventoryController.Instance != null &&
                               transform.IsChildOf(InventoryController.Instance.inventoryPanel.transform);

            if (!inInventory)
            {
                bool questActive = QuestController.Instance != null &&
                                   QuestController.Instance.IsQuestActive(requiredQuestID);
                gameObject.SetActive(questActive);
            }
        }

        if (IsTouchingPlayer())
        {
            PlayerItemCollector collector = FindObjectOfType<PlayerItemCollector>();
            collector?.TryPickup(gameObject);
        }
    }

    private bool IsTouchingPlayer()
    {
        Collider2D playerCollider = GameObject.FindGameObjectWithTag("Player")?.GetComponent<Collider2D>();
        Collider2D itemCollider = GetComponent<Collider2D>();
        return playerCollider != null && itemCollider != null && playerCollider.IsTouching(itemCollider);
    }

    private void Awake()
    {
        if (string.IsNullOrEmpty(instanceID))
            instanceID = System.Guid.NewGuid().ToString();   // unique per instance
        quantityText = GetComponentInChildren<TMP_Text>();
        UpdateQuantityDisplay();
    }

public virtual void PickUp()
{
        if (ItemPickupUIController.Instance != null)
        {
            Sprite itemIcon = GetComponent<Image>()?.sprite;
            ItemPickupUIController.Instance.ShowItemPickup(Name, itemIcon);
        }
        else
        {
            Debug.Log("Error in pcikup fucntion");
        }
           // SaveController.Instance?.AddCollectedItem(instanceID, ID);
}

// Updates the quantity text on the UI
public void UpdateQuantityDisplay()
{
    quantityText.text = quantity > 0 ? quantity.ToString() : "";
}

// Adds items to the stack
public void AddToStack(int amount = 1)
{
    quantity += amount;
        if (quantityText == null)
        {
            Debug.Log("Quantity text not found!");
            quantityText = GetComponentInChildren<TMP_Text>();
        }
            
        UpdateQuantityDisplay();
}

// Removes items from the stack and returns how many were removed
//create copy of this fucntion for quest system without itemRebuild call
public int RemoveFromStackCopy(int amount = 1)
{
    int removed = Mathf.Min(amount, quantity);
    quantity -= removed;
        Debug.Log($"After removal: quantity now {quantity}, removed {removed}");
        UpdateQuantityDisplay();
        if (InventoryController.Instance != null && quantity == 0)
        {
            // This might help trigger updates
            InventoryController.Instance.RebuildItemCounts();
        }
        Debug.Log($"After removal: quantity now {quantity}, removed {removed}");
        return removed;
}

    public int RemoveFromStack(int amount = 1)
    {
        int removed = Mathf.Min(amount, quantity);
        quantity -= removed;
        Debug.Log($"After removal: quantity now {quantity}, removed {removed}");
        UpdateQuantityDisplay();
        if (InventoryController.Instance != null && quantity == 0)
        {
            // This might help trigger updates
            InventoryController.Instance.RebuildItemCounts();
        }
        Debug.Log($"After removal: quantity now {quantity}, removed {removed}");
        return removed;
    }

    // Creates a clone of this item with a specified quantity
    public GameObject CloneItem(int newQuantity)
{
    GameObject clone = Instantiate(gameObject);
    Item cloneItem = clone.GetComponent<Item>();
    cloneItem.quantity = newQuantity;
    cloneItem.UpdateQuantityDisplay();
    return clone;
}


public virtual void UseItem(){
Debug.Log("Useing Item "+ Name);
}
}
