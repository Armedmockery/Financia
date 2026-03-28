using UnityEngine;
using System.Collections.Generic;
using TMPro;


[System.Serializable]
public class RewardData
{
    public string rewardName;
    public Sprite rewardIcon;
}

public class TaxChoiceTracker : MonoBehaviour
{
    public static TaxChoiceTracker Instance;
    
    [System.Serializable]
    public class ChoiceRecord
    {
        public RoomController room;
        public string choiceText;
        public int wisdomDelta;
        public string explanation;
    }
    
    public List<ChoiceRecord> choiceHistory = new List<ChoiceRecord>();
    
    public int totalWisdomScore = 0;
    
    [Header("Collected Rewards")]
    public List<RewardData> collectedRewards = new List<RewardData>(); // Changed to RewardData list
    
    [Header("UI Reference")]
    public TextMeshProUGUI wisdomScoreText;
    
    void Awake()
    {
        Instance = this;
    }
    
    public void RecordChoice(RoomController room, string text, int wisdom, string explanation)
    {
        ChoiceRecord record = new ChoiceRecord
        {
            room = room,
            choiceText = text,
            wisdomDelta = wisdom,
            explanation = explanation
        };
        
        choiceHistory.Add(record);
        totalWisdomScore += wisdom;
        
        Debug.Log($"Choice recorded: {text} (Wisdom: {wisdom})");
        
        if (wisdomScoreText != null)
            wisdomScoreText.text = $"Tax Wisdom: {totalWisdomScore}";
    }
    // Add this method to get choice data for UI
public List<ChoiceRecord> GetChoices()
{
    return choiceHistory;
}
    // UPDATED: Now takes Sprite parameter
    public void AddReward(string rewardName, Sprite rewardIcon)
    {
        RewardData newReward = new RewardData
        {
            rewardName = rewardName,
            rewardIcon = rewardIcon
        };
        
        collectedRewards.Add(newReward);
        Debug.Log($"Reward collected: {rewardName} with icon");
    }
    
    // For backward compatibility (if needed)
    public void AddReward(string rewardName)
    {
        AddReward(rewardName, null);
    }
    
    // public string GetSummary()
    // {
    //     string summary = "📊 YOUR TAX JOURNEY 📊\n\n";
        
    //     foreach (var choice in choiceHistory)
    //     {
    //         summary += $"• {choice.choiceText}\n";
            
    //         if (choice.wisdomDelta > 0)
    //             summary += $"  └─ ✓ Smart tax move (+{choice.wisdomDelta})\n";
    //         else if (choice.wisdomDelta < 0)
    //             summary += $"  └─ ⚠ Tax mistake ({choice.wisdomDelta})\n";
    //         else
    //             summary += $"  └─ ➡ Neutral choice\n";
    //     }
        
    //     summary += $"\n🎁 REWARDS COLLECTED:\n";
    //     foreach (string reward in collectedRewards)
    //     {
    //         summary += $"• {reward}\n";
    //     }
        
    //     summary += $"\n🎯 FINAL TAX WISDOM: {totalWisdomScore}";
        
    //     return summary;
    // }
    
    public void ResetTracker()
    {
        choiceHistory.Clear();
        collectedRewards.Clear();
        totalWisdomScore = 0;
    }
}