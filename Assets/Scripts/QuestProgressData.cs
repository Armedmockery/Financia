using System;
using System.Collections.Generic;

[Serializable]
public class QuestProgressSaveData
{
    public string questID;                     // Reference to the Quest asset by ID
    public List<ObjectiveSaveData> objectives; // Current progress of each objective

    public QuestProgressSaveData() { }

    public QuestProgressSaveData(QuestProgress progress)
    {
        questID = progress.quest.questID;
        objectives = new List<ObjectiveSaveData>();
        foreach (var obj in progress.objectives)
        {
            objectives.Add(new ObjectiveSaveData(obj));
        }
    }
}

[Serializable]
public class ObjectiveSaveData
{
    public string objectiveID;
    public int currentAmount;

    public ObjectiveSaveData() { }

    public ObjectiveSaveData(QuestObjective objective)
    {
        objectiveID = objective.objectiveID;
        currentAmount = objective.currentAmount;
    }
}