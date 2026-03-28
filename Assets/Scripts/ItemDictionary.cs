using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemDictionary : MonoBehaviour
{
    public List<Item> itemPrefabs;

    private Dictionary<int, GameObject> itemDictionary;

    private void Awake()
    {
        itemDictionary = new Dictionary<int, GameObject>();

        //// Auto-increment IDs
        //for (int i = 0; i < itemPrefabs.Count; i++)
        //{
        //    if (itemPrefabs[i] != null)
        //    {
        //        itemPrefabs[i].ID = i + 1;
        //    }
        //}

        foreach (Item item in itemPrefabs)
        {
            if (item != null)
            {
                itemDictionary[item.ID] = item.gameObject;
            }
            

        }

        if (FindObjectsByType<ItemDictionary>(FindObjectsSortMode.None).Length > 1)
        {
            Destroy(gameObject);
        }
        else
        {
            //DontDestroyOnLoad(gameObject);
        }
    }

    void Start()
    {
        Debug.Log("ItemDictionery on!");
    }

    public GameObject GetItemPrefab(int itemID)
    {
        itemDictionary.TryGetValue(itemID, out GameObject prefab);

        if (prefab == null)
        {
            Debug.LogWarning($"Item with ID {itemID} not found in dictionary");
        }

        return prefab;
    }
}
