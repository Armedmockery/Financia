using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.InputSystem;


public class HotbarController : MonoBehaviour
{
    public GameObject hotbarPanel;
    public GameObject slotPrefab;
    public int slotCount = 5;

    private ItemDictionary itemDictionary;

    private Key[] hotbarKeys;

    private void Awake(){
        itemDictionary=FindObjectOfType<ItemDictionary>();

        hotbarKeys = new Key[slotCount];
        for (int i = 0; i < slotCount; i++)
        {
            hotbarKeys[i] = i<9? (Key)((int)Key.Digit1 + i) : Key.Digit0;
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }
    

    // Update is called once per frame
    private void Update()
{
    CleanupInvalidReferences();

    for (int i = 0; i < slotCount; i++)
    {
        if (Keyboard.current[hotbarKeys[i]].wasPressedThisFrame)
        {
            UseItemInSlot(i);
        }
    }
}

    void CleanupInvalidReferences()
{
    foreach (Transform t in hotbarPanel.transform)
    {
        HotbarSlot hs = t.GetComponent<HotbarSlot>();

        if (hs != null && hs.IsEmpty())
        {
            hs.Clear();
        }
    }
}
    // public void UseItemInSlot(int index){
    //     Slot slot = hotbarPanel.transform.GetChild(index).GetComponent<Slot>();
    //     if(slot.currentItem!=null){
    //         Item item= slot.currentItem.GetComponent<Item>();
    //         item.UseItem(); 
    //     }
    // }
    public void UseItemInSlot(int index)
{
    HotbarSlot hotbarSlot = hotbarPanel.transform
        .GetChild(index)
        .GetComponent<HotbarSlot>();

    if (hotbarSlot == null || hotbarSlot.IsEmpty())
        return;

    Item item = hotbarSlot.GetItem();
    item.UseItem();
}

   
    public List<int> GetHotbarReferences()
{
    List<int> refs = new List<int>();

    foreach (Transform t in hotbarPanel.transform)
    {
        HotbarSlot hs = t.GetComponent<HotbarSlot>();

        if (hs != null && hs.linkedInventorySlot != null)
        {
            refs.Add(
                hs.linkedInventorySlot.transform.GetSiblingIndex()
            );
        }
        else
        {
            refs.Add(-1);
        }
    }

    return refs;
}
public void SetHotbarReferences(List<int> refs)
{
    foreach (Transform child in hotbarPanel.transform)
        Destroy(child.gameObject);

    for (int i = 0; i < slotCount; i++)
    {
        GameObject slotObj = Instantiate(slotPrefab, hotbarPanel.transform);
        slotObj.AddComponent<HotbarSlot>();
    }

    for (int i = 0; i < refs.Count; i++)
    {
        if (refs[i] == -1) continue;

        Slot invSlot =
            InventoryController.Instance.inventoryPanel.transform
            .GetChild(refs[i])
            .GetComponent<Slot>();

        HotbarSlot hs =
            hotbarPanel.transform.GetChild(i)
            .GetComponent<HotbarSlot>();

        hs.linkedInventorySlot = invSlot;
    }
}


}
