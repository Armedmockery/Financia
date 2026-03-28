using UnityEngine;
using System.Collections;
using System.Threading.Tasks;

public class RewardsController : MonoBehaviour
{
    [SerializeField]
    [Header("Coin Reward Animation")]
    [Tooltip("Drag the Animation component from your coins GameObject in the scene")]
    public GameObject coinsAnimationObject;

    public static RewardsController Instance { get; private set; }
    GameManager gameManager;
    

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        gameManager = FindObjectOfType<GameManager>();
    }

    //private void Start()
    //{
    //    // Ensure the singleton is properly set
    //    if (Instance == null)
    //    {
    //        Instance = this;
    //    }
    //}

    public void GiveQuestReward(Quest quest)
    {
        if (quest?.questRewards == null) return;

        foreach(var reward in quest.questRewards)
        {
            switch (reward.type)
            {
                case RewardType.Item:
                    Debug.Log("Giving item reward");
                    GiveItemReward(reward.rewardID, reward.amount);
                    break;
                case RewardType.Score:
                    Debug.Log("Giving score reward");
                    gameManager.updateScoreEnumCall(reward.amount);
                    ItemPickupUIController.Instance?.ShowItemPickup($"+{reward.amount} Score");
                    break;
                case RewardType.Coins:
                    Debug.Log("Giving coins reward");
                    gameManager.updateCoinsEnumCall(reward.amount);
                    ItemPickupUIController.Instance?.ShowItemPickup($"+{reward.amount} Coins");
                    coinsAnimationObject?.SetActive(true);
                    Debug.Log("coinsAnimationObject is active");
                    StartCoroutine(DisableAfterAnimation(coinsAnimationObject));
                    break;
                case RewardType.Level:
                    Debug.Log("Giving level reward");
                    gameManager.updateLevelEnumCall(reward.amount);
                    ItemPickupUIController.Instance?.ShowItemPickup($"Level Up! +{reward.amount}");
                    break;
            }
        }
    }

    public void GiveItemReward(int itemID, int amount)
    {
        var itemPrefab = FindAnyObjectByType<ItemDictionary>()?.GetItemPrefab(itemID);

        if (itemPrefab == null)
        {
            Debug.LogError($"Cannot give item reward: prefab for ID {itemID} not found in ItemDictionary.");
            return;
        }

        // Instantiate a temp instance with the full amount set
        GameObject tempItem = Instantiate(itemPrefab);
        tempItem.SetActive(false); // keep it hidden
        Item itemComp = tempItem.GetComponent<Item>();
        if (itemComp != null)
            itemComp.quantity = amount;

        bool added = InventoryController.Instance.AddItem(tempItem);
        Destroy(tempItem); // always clean up temp object

        if (added)
            Debug.Log($"Gave item reward: ID={itemID}, amount={amount}");
        else
            Debug.LogWarning($"Inventory full � could not give item reward ID={itemID}");
        //for(int i = 0; i<amount; i++)
        //{
        //    if (!InventoryController.Instance.AddItem(itemPrefab))
        //    {
        //        //GameObject dropItem = Instantiate(itemPrefab, transform.position + Vector3.down, Quaternion.identity);
        //        Debug.Log("Item Added");
        //        //dropItem.GetComponent<BounceEffect>().StartBounce();
        //    }
        //    else
        //    {
        //        //showPopup
        //    }
        //}
    }

    private IEnumerator DisableAfterAnimation(GameObject obj)
    {
       
        yield return new WaitForSeconds(2.0f);
        obj.SetActive(false);
    }
}
