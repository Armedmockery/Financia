using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;


public class HotbarSlot : MonoBehaviour, IDropHandler
{
    public Slot linkedInventorySlot;

    public Image icon;
    public TMP_Text quantityText;

    public bool IsEmpty()
    {
        return linkedInventorySlot == null ||
               linkedInventorySlot.currentItem == null;
    }

    public Item GetItem()
    {
        if (IsEmpty()) return null;
        return linkedInventorySlot.currentItem.GetComponent<Item>();
    }

    public void Clear()
    {
        linkedInventorySlot = null;
        RefreshUI();
    }

    void Update()
    {
        RefreshUI();
    }

    void RefreshUI()
    {
        if (IsEmpty())
        {
            icon.enabled = false;
            quantityText.text = "";
            return;
        }

        Item item = GetItem();

        icon.enabled = true;
        icon.sprite =
            linkedInventorySlot.currentItem.GetComponent<Image>().sprite;

        quantityText.text =
            item.quantity > 1 ? item.quantity.ToString() : "";
    }
    public void OnDrop(PointerEventData eventData)
{
    GameObject draggedItem = eventData.pointerDrag;

    if (draggedItem == null) return;

    Slot sourceInventorySlot =
        draggedItem.transform.parent.GetComponent<Slot>();

    HotbarSlot sourceHotbarSlot =
        draggedItem.GetComponentInParent<HotbarSlot>();

    HotbarController controller =
        FindObjectOfType<HotbarController>();

    // Inventory → Hotbar
    if (sourceInventorySlot != null)
    {
        linkedInventorySlot = sourceInventorySlot;
        return;
    }

    // Hotbar → Hotbar
    if (sourceHotbarSlot != null)
    {
        Slot temp = linkedInventorySlot;
        linkedInventorySlot = sourceHotbarSlot.linkedInventorySlot;
        sourceHotbarSlot.linkedInventorySlot = temp;
    }
}
public void RemoveReference()
{
    linkedInventorySlot = null;
}


}
