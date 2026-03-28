using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "New Quest",
    menuName = "Quests/Quest"
)]
public class Quest : ScriptableObject
{
    [Header("Quest Info")]
    public string questID;
    public string questName;
    [TextArea]
    public string description;

    [Header("Objectives")]
    public List<QuestObjective> objectives = new List<QuestObjective>();
    public List<QuestReward> questRewards = new List<QuestReward>();
    // Called when the ScriptableObject is loaded or created
    private void OnEnable()
    {
        if (string.IsNullOrEmpty(questID))
        {
            questID = questName + "_" + Guid.NewGuid().ToString();
        }
    }
}

[System.Serializable]
public class QuestObjective
{
    public string objectiveID;        // Used to match item, enemy, NPC, etc.
    public string description;
    public ObjectiveType type;

    public int requiredAmount;
    public int currentAmount;

    public bool IsCompleted => currentAmount >= requiredAmount;
}

public enum ObjectiveType
{
    CollectItem,
    DefeatEnemy,
    ReachLocation,
    TalkToNPC,
    Custom
}
[System.Serializable]
public class QuestProgress
{
    public Quest quest;
    public List<QuestObjective> objectives;

    public QuestProgress(Quest quest)
    {
        this.quest = quest;
        objectives = new List<QuestObjective>();

        // Deep copy to avoid modifying original ScriptableObject data
        foreach (var obj in quest.objectives)
        {
            objectives.Add(new QuestObjective
            {
                objectiveID = obj.objectiveID,
                description = obj.description,
                type = obj.type,
                requiredAmount = obj.requiredAmount,
                currentAmount = 0
            });
        }
    }

    public bool IsCompleted => objectives.TrueForAll(o => o.IsCompleted);

    public string QuestID => quest.questID;
}

[System.Serializable]
public class QuestReward
{
    public RewardType type;
    public int rewardID;
    public int amount = 1;
}

public enum RewardType { Item, Score, Coins, Level}