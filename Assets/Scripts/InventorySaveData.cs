using UnityEngine;

[System.Serializable]
public class InventorySaveData
{
    public int itemID;
    public int slotIndex; //index of slot where the item will be placed.
    public int quantity=1;

    public InventorySaveData(int itemID, int slotIndex, int quantity)
    {
        this.itemID = itemID;
        this.slotIndex = slotIndex;
        this.quantity=quantity;
    }
}
