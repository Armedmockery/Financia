using UnityEngine;
using UnityEngine.UI;

public class TaxRewardPickup : MonoBehaviour
{
    [Header("Item Details")]
    public string rewardName = "Tax Document";
    public Sprite rewardIcon;  // Drag the item's icon here
    
    [Header("Settings")]
    public bool showPopup = true;
    
    private void Start()
    {
        // If no icon assigned, try to get from Image component
        if (rewardIcon == null)
        {
            Image img = GetComponent<Image>();
            if (img != null)
                rewardIcon = img.sprite;
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        
        // 1. Add to TaxChoiceTracker list
        if (TaxChoiceTracker.Instance != null)
        {
            TaxChoiceTracker.Instance.AddReward(rewardName, rewardIcon);
            Debug.Log($"✅ Tax reward added to tracker: {rewardName}");
        }
        
        // 2. Show popup using your existing system
        if (showPopup && ItemPickupUIController.Instance != null)
        {
            ItemPickupUIController.Instance.ShowItemPickup(rewardName, rewardIcon);
        }
        
        // 3. Destroy the item
        Destroy(gameObject);
    }
}