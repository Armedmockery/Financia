using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RewardItemUI : MonoBehaviour
{
    [Header("UI Elements")]
    public Image iconImage;        // Assign in prefab Inspector
    public TextMeshProUGUI nameText; // Assign in prefab Inspector
    
    public void Setup(string rewardName, Sprite rewardIcon = null)
    {
        // Set name
        if (nameText != null)
            nameText.text = rewardName;
        
        // Handle icon
        if (iconImage != null)
        {
            if (rewardIcon != null)
            {
                iconImage.sprite = rewardIcon;
                iconImage.gameObject.SetActive(true);
                iconImage.color = Color.white; // Make sure it's visible
            }
            else
            {
                // If no icon, hide the image or show default
                iconImage.gameObject.SetActive(false);
                Debug.Log($"No icon for {rewardName}");
            }
        }
    }
}