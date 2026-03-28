using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using FindObjectsInactive = UnityEngine.FindObjectsInactive;
using FindObjectsSortMode = UnityEngine.FindObjectsSortMode;

public class QuestController : MonoBehaviour
{
    public static QuestController Instance { get; private set; }
    public static System.Action OnQuestStateChanged;
    public List<QuestProgress> activateQuests = new();

    [Header("Active Quests")]
    //public List<QuestProgress> activeQuests = new List<QuestProgress>();
    public List<string> handInQuestIDs = new();
    private QuestUI questUI;
    public QuestDatabase questDatabase;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            //DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        Debug.Log("Questcontroller active");
        
        //StartCoroutine(SubscribeToInventoryEvents());
        //InventoryController.Instance.OnInventoryChanged.AddListener(CheckInventoryForQuests);
        //InventoryController.Instance.OnInventoryChanged += CheckInventoryForQuests;
    }

    //private IEnumerator SubscribeToInventoryEvents()
    //{
    //    // Wait until InventoryController is ready
    //    while (InventoryController.Instance == null)
    //    {
    //        Debug.Log("Waiting for InventoryController...");
    //        yield return null;
    //    }

    //    Debug.Log("Subscribing to OnInventoryChanged");
    //    InventoryController.Instance.OnInventoryChanged.AddListener(InventoryController.Instance.CheckInventoryForQuests);
    //}

    public void Start()
    {
        Debug.Log($"RewardsController.Instance: {RewardsController.Instance}");
        questUI = FindObjectOfType<QuestUI>();
        if (InventoryController.Instance != null)
        {
            InventoryController.Instance.OnInventoryChanged.AddListener(CheckInventoryForQuests);
            Debug.Log("Subscribed to inventory changes");
        }
        else
        {
            Debug.LogError("InventoryController.Instance is null!");
        }
    }

    private void OnDestroy()
    {
        // Clean up listener
        if (InventoryController.Instance != null)
        {
            InventoryController.Instance.OnInventoryChanged.RemoveListener(CheckInventoryForQuests);
        }
    }

    public void AcceptQuest(Quest quest)
    {
        //to not let user again get same quest enable
        /*if (IsQuestActive(quest.questID) || handInQuestIDs.Contains(quest.questID))
        {
            Debug.Log("Quest already active or completed.");
            return;
        }*/

        if (IsQuestActive(quest.questID))
            return;

        Debug.Log("accepted quest!");

        var progress = new QuestProgress(quest);

        // Sync with already killed enemies / visited locations
        foreach (var obj in progress.objectives)
        {
            if (obj.type == ObjectiveType.CollectItem && int.TryParse(obj.objectiveID, out int itemID))
            {
                int collected = SaveController.Instance?.GetCollectedCount(itemID) ?? 0;
                obj.currentAmount = Mathf.Min(collected, obj.requiredAmount);
                Debug.Log($"Quest {quest.questID}: objective {obj.description} initialised to {obj.currentAmount}/{obj.requiredAmount}");
            }

            if (obj.type == ObjectiveType.DefeatEnemy)
            {
                int killed = SaveController.Instance?.GetKillCount(obj.objectiveID) ?? 0;
                obj.currentAmount = Mathf.Min(killed, obj.requiredAmount);
            }
            else if (obj.type == ObjectiveType.ReachLocation)
            {
                int visited = SaveController.Instance?.GetVisitCount(obj.objectiveID) ?? 0;
                obj.currentAmount = Mathf.Min(visited, obj.requiredAmount);
            }
        }

        activateQuests.Add(progress);//new QuestProgress(quest)
        Debug.Log($"{quest.questID} activate uwest {activateQuests.Count}");

        // If any objective is already completed, clean up its remaining objects
        //foreach (var obj in progress.objectives)
        //{
        //    if (obj.IsCompleted)
        //        //DisableRemainingObjects(obj);
        //}
        
        // Enable all world objects that belong to this quest
        CheckInventoryForQuests();
        EnableQuestObjects(quest.questID);

        questUI.UpdateQuestUI();
        SaveController.Instance?.SaveGame();
        OnQuestStateChanged?.Invoke();
    }

    public bool IsQuestActive(string questID)
    {
        return activateQuests.Exists(q => q.QuestID == questID);
    }

    public void CheckInventoryForQuests()
    {
        if (InventoryController.Instance == null) return;

        //Dictionary<int, int> itemCounts = InventoryController.Instance.GetItemCounts();
        
        // /Read directly from slots instead of relying on potentially stale cache
        Dictionary<int, int> itemCounts = new Dictionary<int, int>();
        Debug.Log($"CheckInventoryForQuests: activateQuests={activateQuests.Count}, itemCounts={itemCounts.Count}");

        foreach (Transform slotTransform in InventoryController.Instance.inventoryPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();
            if (slot?.currentItem != null)
            {
                Item item = slot.currentItem.GetComponent<Item>();
                if (item != null)
                {
                    itemCounts.TryGetValue(item.ID, out int count);
                    itemCounts[item.ID] = count + item.quantity;
                }
            }
        }

        foreach (QuestProgress quest in activateQuests){
            foreach (QuestObjective questObjective in quest.objectives)
            {
                if(questObjective.type != ObjectiveType.CollectItem) continue;
                if(!int.TryParse(questObjective.objectiveID, out int itemID)) continue;

                int newAmount = itemCounts.TryGetValue(itemID, out int count) ? Mathf.Min(count, questObjective.requiredAmount) : questObjective.currentAmount; // preserve existing value instead of zeroing

                Debug.Log($"  Syncing item {itemID}: inventory={count}, was={questObjective.currentAmount}, now={newAmount}");
                questObjective.currentAmount = newAmount;
                
            }
        }
        questUI?.UpdateQuestUI();
    }

    // Call this from your NPC dialogue script when a conversation completes
    // npcID should match the objectiveID set in the Quest ScriptableObject
    public void UpdateNPCTalked(string npcID, int amount = 1)
    {
        foreach (QuestProgress quest in activateQuests)
        {
            foreach (QuestObjective objective in quest.objectives)
            {
                if (objective.type == ObjectiveType.TalkToNPC &&
                    objective.objectiveID == npcID &&
                    !objective.IsCompleted)           // prevent over-counting
                {
                    int newAmount = Mathf.Min(objective.currentAmount + amount, objective.requiredAmount);
                    if (newAmount != objective.currentAmount)
                    {
                        Debug.Log("Quest objective called for NPC");
                        objective.currentAmount = newAmount;
                        questUI?.UpdateQuestUI();
                        SaveController.Instance?.SaveGame();
                    }
                }
            }
        }
    }

    // Call this from anywhere for Custom objectives (puzzles, events, triggers, etc.)
    // customID should match the objectiveID set in the Quest ScriptableObject
    public void UpdateCustomObjective(string customID, int amount = 1)
    {
        foreach (QuestProgress quest in activateQuests)
        {
            foreach (QuestObjective objective in quest.objectives)
            {
                if (objective.type == ObjectiveType.Custom &&
                    objective.objectiveID == customID &&
                    !objective.IsCompleted)
                {
                    int newAmount = Mathf.Min(objective.currentAmount + amount, objective.requiredAmount);
                    if (newAmount != objective.currentAmount)
                    {
                        Debug.Log("Quest objective called for custom");
                        objective.currentAmount = newAmount;
                        questUI?.UpdateQuestUI();
                        SaveController.Instance?.SaveGame();
                    }
                }
            }
        }
    }

    public bool IsQuestCompleted(string questID)
    {
        QuestProgress quest = activateQuests.Find(q => q.QuestID == questID);
        return quest != null && quest.objectives.TrueForAll(o => o.IsCompleted);
    }

    public void HandInQuest(string questID)
    {
        QuestProgress quest = activateQuests.Find(q => q.QuestID == questID);
        if (quest == null)
        {
            Debug.Log("Quest is null");
            return;
        }

        // Check if quest is actually completed
        if (!quest.IsCompleted)
        {
            Debug.Log($"Quest {questID} is not completed!");
            return;
        }
        //remove required items
        Debug.Log(" remove required items");

        //remove required items
        Debug.Log("Attempting to remove required items...");
        InventoryController.Instance.RebuildItemCounts();
        bool removed = RemoveRequiredItemsFromInventory(questID);
        Debug.Log($"RemoveRequiredItemsFromInventory returned: {removed}");

        if (!removed)
        {
            Debug.Log($"Cannot remove required items for quest {questID}");
            return;
        }

        //if (!RemoveRequiredItemsFromInventory(questID))
        //{
        //    Debug.Log($"Cannot remove required items for quest {questID}");
        //    return; //quest not complete
        //}
        if (quest.quest?.questRewards == null)
        {
            Debug.Log("quest rewards is null!");
        }
            if (quest.quest?.questRewards != null)
        {
            Debug.Log("quest rewards is not null!");
            if (RewardsController.Instance != null)
            {
                Debug.Log("RewardsController.Instance is called!");
                RewardsController.Instance.GiveQuestReward(quest.quest);
                SaveController.Instance?.SaveGame();
            }
            else
            {
                Debug.Log("RewardsController.Instance is null!");
            }
        }
        handInQuestIDs.Add(questID);
        activateQuests.Remove(quest);
        questUI.UpdateQuestUI();
        OnQuestStateChanged?.Invoke();

    }

    public bool IsQuestHandedIn(string questId)
    {
        return handInQuestIDs.Contains(questId);
    }

    private bool RemoveRequiredItemsFromInventory(string questID)
    {
        Debug.Log("Removing required items");
        QuestProgress progress = activateQuests.Find(q => q.quest.questID == questID);
        if (progress == null) return false;

        if (InventoryController.Instance == null) { Debug.Log("No inventory"); return false; }
        Debug.Log("InventoryController.Instance found");

        // Read directly from slots — cache may be stale
        Dictionary<int, int> itemCounts = new Dictionary<int, int>();
        foreach (Transform slotTransform in InventoryController.Instance.inventoryPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();
            if (slot?.currentItem != null)
            {
                Item item = slot.currentItem.GetComponent<Item>();
                if (item != null)
                {
                    itemCounts.TryGetValue(item.ID, out int c);
                    itemCounts[item.ID] = c + item.quantity;
                }
            }
        }

        Debug.Log($"Got {itemCounts.Count} item types from inventory");

        // Verify we have enough before removing anything
        foreach (var objective in progress.objectives)
        {
            if (objective.type != ObjectiveType.CollectItem) continue;
            if (!int.TryParse(objective.objectiveID, out int itemID)) continue;
            itemCounts.TryGetValue(itemID, out int have);
            Debug.Log($"Checking - Item {itemID}: Have {have}, Need {objective.requiredAmount}");
            if (have < objective.requiredAmount) return false;
        }

        // All checks passed — now remove
        foreach (var objective in progress.objectives)
        {
            if (objective.type != ObjectiveType.CollectItem) continue;
            if (!int.TryParse(objective.objectiveID, out int itemID)) continue;
            InventoryController.Instance.RemoveItemsFromInventory(itemID, objective.requiredAmount);
        }

        return true;
    }

    //public bool RemoveRequiredItemsFromInventory(string questID)
    //{
    //    Debug.Log("Removing required items");
    //    QuestProgress quest = activateQuests.Find(q => q.QuestID == questID);
    //    if (quest ==  null) return false;

    //    if (InventoryController.Instance == null) { Debug.Log("No inventory"); return false; }
    //    Debug.Log("InventoryController.Instance found");

    //    Dictionary<int, int> requiredItems = new();
    //    //items from objectives
    //    foreach(QuestObjective objective in  quest.objectives)
    //    {
    //        if(objective.type == ObjectiveType.CollectItem && int.TryParse(objective.objectiveID, out int itemID))
    //        {
    //            requiredItems[itemID] = objective.requiredAmount;
    //            Debug.Log($"Removed {requiredItems[itemID]}");
    //        }
    //    }
    //    // Check if InventoryController is available
    //    if (InventoryController.Instance == null)
    //            {
    //                Debug.LogError("InventoryController.Instance is null!");
    //                return false;
    //            }
    //    Debug.Log("InventoryController.Instance found");

    //    Dictionary<int, int> itemCounts = InventoryController.Instance.GetItemCounts();
    //    Debug.Log($"Got {itemCounts.Count} items from inventory");
    //    foreach (var item in requiredItems)
    //    {
    //        Debug.Log($"Checking requirement - Item {item.Key}: Have {itemCounts.Count}, Need {item.Value}");
    //        if (itemCounts.GetValueOrDefault(item.Key) < item.Value)
    //        {
    //            return false;
    //        }
    //    }

    //    foreach(var itemRequirement in requiredItems)
    //    {
    //        Debug.Log($"Removing {itemRequirement.Value} of item {itemRequirement.Key}");
    //        InventoryController.Instance.RemoveItemsFromInventory(itemRequirement.Key, itemRequirement.Value);
    //    }
    //    Debug.Log("Returning true");
    //    return true;
    //}

    //public void LoadQuestProgress(List<QuestProgress> savedQuests)
    //{
    //    if(savedQuests == null)
    //    {
    //        Debug.Log("savedwuast is null");
    //    }
    //    activateQuests = savedQuests ?? new();

    //    QuestController.Instance.CheckInventoryForQuests();
    //    questUI.UpdateQuestUI();
    //}

    public void LoadQuestProgress(List<QuestProgressSaveData> savedQuests)
    {
        questUI = FindObjectOfType<QuestUI>();
        activateQuests.Clear();

        Debug.Log($"=== LoadQuestProgress START, received {savedQuests?.Count} quests ===");
        Debug.Log($"questDatabase is null? {questDatabase == null}");

        if (questDatabase != null)
        {
            Debug.Log($"questDatabase has {questDatabase.allQuests.Count} quests:");
            foreach (var q in questDatabase.allQuests)
                Debug.Log($"  DB Quest: '{q.questID}' | '{q.questName}'");
        }

        if (savedQuests == null) return;

        foreach (var saveData in savedQuests)
        {
            Debug.Log($"Looking for saved questID: '{saveData.questID}'");
            Quest quest = questDatabase.GetQuestByID(saveData.questID);
            Debug.Log($"Found quest: {(quest == null ? "NULL" : quest.questName)}");

            if (quest == null) continue;

            QuestProgress progress = new QuestProgress(quest);
            foreach (var objSave in saveData.objectives)
            {
                var objective = progress.objectives.Find(o => o.objectiveID == objSave.objectiveID);
                if (objective != null)
                    objective.currentAmount = objSave.currentAmount;
                Debug.Log(objective.currentAmount + objSave.currentAmount);
            }
            activateQuests.Add(progress);
        }

        Debug.Log($"=== LoadQuestProgress END, activateQuests count: {activateQuests.Count} ===");
        OnQuestStateChanged?.Invoke();
    }

    //other objective handling
    public void UpdateEnemyKill(string enemyID, int amount = 1)
    {
        foreach (QuestProgress quest in activateQuests)
        {
            foreach (QuestObjective objective in quest.objectives)
            {
                if (objective.type == ObjectiveType.DefeatEnemy &&
                    objective.objectiveID == enemyID &&
                !objective.IsCompleted)
                {
                    int newAmount = Mathf.Min(objective.currentAmount + amount, objective.requiredAmount);
                    if (newAmount != objective.currentAmount)
                    {
                        objective.currentAmount = newAmount;
                        questUI?.UpdateQuestUI();

                        //if (objective.IsCompleted)
                        //    DisableRemainingObjects(objective);
                    }
                }
            }
        }
    }

    public void UpdateLocationVisit(string locationID, int amount = 1)
    {
        foreach (QuestProgress quest in activateQuests)
        {
            foreach (QuestObjective objective in quest.objectives)
            {
                if (objective.type == ObjectiveType.ReachLocation &&
                    objective.objectiveID == locationID)
                {
                    int newAmount = Mathf.Min(objective.currentAmount + amount, objective.requiredAmount);
                    if (newAmount != objective.currentAmount)
                    {
                        objective.currentAmount = newAmount;
                        questUI?.UpdateQuestUI();

                        //if (objective.IsCompleted)
                        //    DisableRemainingObjects(objective);
                    }
                }
            }
        }
    }

    public void EnableQuestObjects(string questID)
    {
        // Enable items
        Debug.Log("Enabling items");
        foreach (var item in FindObjectsByType<Item>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            Debug.Log("enabling item");
            if (item.requiredQuestID == questID)
                item.gameObject.SetActive(true);
        }

        // Enable enemies
        foreach (var enemy in FindObjectsByType<EnemyStats>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (enemy.requiredQuestID == questID)
                enemy.gameObject.SetActive(true);
        }

        // Enable location triggers
        foreach (var loc in FindObjectsByType<LocationTrigger>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            Debug.Log("enabling trigger");
            if (loc.requiredQuestID == questID)
                loc.gameObject.SetActive(true);
        }
    }

    public void SyncKillObjectives()
    {
        foreach (QuestProgress quest in activateQuests)
        {
            foreach (QuestObjective objective in quest.objectives)
            {
                if (objective.type != ObjectiveType.DefeatEnemy) continue;

                int killCount = SaveController.Instance.GetKillCount(objective.objectiveID);
                objective.currentAmount = Mathf.Min(killCount, objective.requiredAmount);

                Debug.Log($"SyncKill: {objective.objectiveID} kills={killCount}, required={objective.requiredAmount}");
            }
        }
    }

    public void SyncLocationObjectives()
    {
        if (SaveController.Instance == null) return;
        foreach (var quest in activateQuests)
        {
            foreach (var obj in quest.objectives)
            {
                if (obj.type == ObjectiveType.ReachLocation)
                {
                    int visits = SaveController.Instance.GetVisitCount(obj.objectiveID);
                    obj.currentAmount = Mathf.Min(visits, obj.requiredAmount);
                }
            }
        }
        questUI?.UpdateQuestUI();
    }

    
    // Called when an objective's currentAmount reaches requiredAmount
    //private void DisableRemainingObjects(QuestObjective obj)
    //{
    //    switch (obj.type)
    //    {
    //        case ObjectiveType.DefeatEnemy:
    //            foreach (var enemy in FindObjectsByType<EnemyStats>(FindObjectsSortMode.None))
    //            {
    //                if (enemy.enemyID == obj.objectiveID && enemy.gameObject.activeSelf)
    //                    enemy.gameObject.SetActive(false);
    //            }
    //            break;

    //        case ObjectiveType.ReachLocation:
    //            foreach (var loc in FindObjectsByType<LocationTrigger>(FindObjectsSortMode.None))
    //            {
    //                if (loc.locationID == obj.objectiveID && loc.gameObject.activeSelf)
    //                    loc.gameObject.SetActive(false);
    //            }
    //            break;

    //            // Items are handled by their own pickup/destroy logic – no extra cleanup needed
    //    }
    //}
}
