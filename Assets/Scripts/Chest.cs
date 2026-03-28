using UnityEngine;

public class Chest : MonoBehaviour ,IInteractable
{
    public bool IsOpened {  get; private set; }
    public string ChestID {  get; private set; }
    public GameObject itemPrefab; //items that chest drops
    public Sprite openedSprite;
   
    void Start()
    {
        ChestID ??= GlobalHellper.GenerateUniqueID(gameObject);
    }

    // Update is called once per frame
  
    public bool canInterac()
    {
        return !IsOpened;   
        
    }

    public void Interact()
    {
        if (!canInterac()) return;
        OpenChest();
    }

    private void OpenChest()
    {
        SetOpened(true);

        if (itemPrefab)
        {
            GameObject droppedItem = Instantiate(itemPrefab, transform.position + Vector3.down, Quaternion.identity);
        }
    }

    public void SetOpened(bool opened)
    {
        if (IsOpened = opened)
        {
            GetComponent<SpriteRenderer>().sprite = openedSprite;
        }
    }
}
