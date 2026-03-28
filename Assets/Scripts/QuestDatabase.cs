using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "QuestDatabase", menuName = "Quests/Database")]
public class QuestDatabase : ScriptableObject
{
    public List<Quest> allQuests;
    private Dictionary<string, Quest> questDict;

    private void OnEnable()
    {
        questDict = new Dictionary<string, Quest>();
        foreach (var q in allQuests)
            if (q != null && !questDict.ContainsKey(q.questID))
                questDict.Add(q.questID, q);
    }

    public Quest GetQuestByID(string id)
    {
        if (questDict != null && questDict.TryGetValue(id, out Quest q))
            return q;

        Quest found = null; // ← initialize to null
        questDict = new Dictionary<string, Quest>();
        foreach (var quest in allQuests)
        {
            if (quest == null) continue;
            if (!questDict.ContainsKey(quest.questID))
                questDict.Add(quest.questID, quest);
            if (quest.questID == id)
                found = quest;
        }

        if (found == null)
            Debug.LogError($"Quest ID '{id}' not found. Available IDs: {string.Join(", ", questDict.Keys)}");

        return found;
    }
}