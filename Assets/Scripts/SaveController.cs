using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Unity.Cinemachine;
using Game.StockMarket;                                             // ADDED: for PortfolioManager

public class SaveController : MonoBehaviour{

    public static SaveController Instance { get; private set; }

    private string saveLocation;
    private InventoryController inventoryController;
    private HotbarController hotbarController;
    [System.Serializable]
    public class CollectedItemData
    {
        public string instanceID;
        public int itemID;
    }

    //private List<CollectedItemData> collectedItems = new();
    private List<CollectedItemData> collectedItemInstances = new List<CollectedItemData>();
    private HashSet<string> collectedItems = new HashSet<string>();

    private GameManager gameManager;            // Assume this holds health/score/coins
    private QuestController questController;

    private List<string> defeatedEnemies = new();
    private List<string> visitedLocations = new();
    public static bool LoadOnNextGameSceneLoad = false;

    private PortfolioManager portfolioManager;                      // ADDED: from Branch 2

    private void Awake()  
    {
        if (Instance == null)
        {
            Instance = this;
            //DontDestroyOnLoad(gameObject);      // Persist across scenes
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        saveLocation = Path.Combine(Application.persistentDataPath, "saveData.json");
        portfolioManager = new PortfolioManager(10000m);           // ADDED: init with default balance on Awake
        Debug.Log("Portfolio created in Awake");
    }

    void Start()
    {
        Debug.Log(Application.persistentDataPath);
        //inventoryController = FindFirstObjectByType<InventoryController>();
        //hotbarController = FindFirstObjectByType<HotbarController>();
        //questController = FindFirstObjectByType<QuestController>();
        //gameManager = FindFirstObjectByType<GameManager>();
        StartCoroutine(LoadGameDelayed()); // ← NOT LoadGame() directly
    }

    private IEnumerator LoadGameDelayed()
    {
        yield return null; // wait for all other Start()s to finish
        yield return null; // second frame for safety

        RefreshControllers();

        if (!File.Exists(saveLocation))
        {
            Debug.Log("No save file found. Creating a new save.");
            SaveGame();
            yield break;
        }

        LoadGame();
    }

    public void SaveGame(){

        RefreshControllers();
        if (inventoryController == null || hotbarController == null ||
            questController == null || gameManager == null)
        {
            Debug.LogError("Cannot save: one or more controllers are missing.");
            return;
        }

        int health = References.userHealth;      // You need to implement these methods
        int score = References.userScore;
        int coins = References.userCoins;

        // Convert active quests to serializable format
        List<QuestProgressSaveData> questSaveData = new List<QuestProgressSaveData>();
        foreach (var progress in questController.activateQuests)
        {
            questSaveData.Add(new QuestProgressSaveData(progress));
        }

        List<string> collectedList = new List<string>(collectedItems);

        SaveData saveData = new SaveData(
            GameObject.FindGameObjectWithTag("Player").transform.position,
            FindObjectOfType<CinemachineConfiner2D>()
                .BoundingShape2D.gameObject.name,
            health,
            score,
            coins,
            inventoryController.GetInventoryItems(),
            hotbarController.GetHotbarReferences(),                 // CHANGED: was GetHotbarItems()
            //QuestController.Instance.activateQuests,
            questSaveData,
            questController.handInQuestIDs,
            collectedItemInstances,
            defeatedEnemies,
            visitedLocations,
            //collectedItems
            portfolioManager.ToSaveData()                           // ADDED: portfolio save
        );

        Debug.Log($"=== SAVEGAME CALLED, saving {questSaveData.Count} quests ===");
        Debug.Log(StackTraceUtility.ExtractStackTrace()); // shows WHO called SaveGame
        File.WriteAllText(
            saveLocation,
            JsonUtility.ToJson(saveData, true)
        );

        Debug.Log("Game saved");
    }

    private IEnumerator DelayedLoad()
    {
        SaveGame();
        yield return null; // Wait one frame for all Start() to complete
        
    }

    public void LoadGame()
    {
        try
        {
            
            RefreshControllers();

            string rawJson = File.ReadAllText(saveLocation);
            SaveData saveData = JsonUtility.FromJson<SaveData>(rawJson);
            Debug.Log($"JSON quest count after read: {saveData.questProgressData?.Count}");

            Debug.Log("Step 1: Setting world state");
            collectedItemInstances = saveData.collectedWorldItems ?? new List<CollectedItemData>();
            defeatedEnemies = saveData.defeatedEnemyIDs ?? new List<string>();
            visitedLocations = saveData.visitedLocationIDs ?? new List<string>();

            Debug.Log("Step 2: Player position");
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                player.transform.position = saveData.playerPosition;

            // Override with spawn point if one exists in this scene
            PlayerSpawnPoint spawnPoint = FindFirstObjectByType<PlayerSpawnPoint>();
            if (spawnPoint != null)
                spawnPoint.Initailise();

            Debug.Log("Step 3: Health/score/coins");
            // your health, score, coins lines here

            Debug.Log("Step 4: SetInventoryItems");
            inventoryController?.SetInventoryItems(saveData.inventorySaveData);

            Debug.Log("Step 5: SetHotbarReferences");               // CHANGED: was LoadQuestProgress label
            hotbarController?.SetHotbarReferences(saveData.hotbarReferences);   // CHANGED: was hotbarSaveData

            Debug.Log("Step 6: LoadQuestProgress");
            questController.handInQuestIDs = saveData.handedInQuestIDs ?? new List<string>();
            questController.LoadQuestProgress(saveData.questProgressData);

            questController.SyncKillObjectives();

            Debug.Log("Step 7: ApplyWorldState");                   // CHANGED: step numbers shifted by 1
            ApplyWorldState();

            Debug.Log("Step 8: RebuildItemCounts");
            inventoryController?.RebuildItemCounts();

            Debug.Log("Step 9: UpdateQuestUI");
            QuestUI.Instance?.UpdateQuestUI();

            Debug.Log("Step 10: Load Portfolio");                   // ADDED: portfolio load
            if (saveData.portfolioSaveData != null)
            {
                portfolioManager = new PortfolioManager(saveData.portfolioSaveData);
                Debug.Log("Loaded cash: " + portfolioManager.CashBalance);
                Debug.Log("Loaded holdings: " + portfolioManager.GetAllHoldings().Count);
            }

            Debug.Log("Game loaded successfully.");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"LoadGame CRASHED at: {e.Message}\n{e.StackTrace}");
        }
    }

    private void RefreshControllers()
    {
        inventoryController = FindFirstObjectByType<InventoryController>();
        hotbarController = FindFirstObjectByType<HotbarController>();
        questController = FindFirstObjectByType<QuestController>();
        gameManager = FindFirstObjectByType<GameManager>();
    }

    // In SaveController
    public void RestoreQuestState(QuestController questController)
    {
        if (!File.Exists(saveLocation)) return;
        SaveData saveData = JsonUtility.FromJson<SaveData>(File.ReadAllText(saveLocation));
        questController.LoadQuestProgress(saveData.questProgressData);
        questController.SyncKillObjectives();
        questController.SyncLocationObjectives();
    }

    public void PersistCurrentState()
    {
        SaveData existingData = null;
        if (File.Exists(saveLocation))
        {
            existingData = JsonUtility.FromJson<SaveData>(File.ReadAllText(saveLocation));
        }
        else
        {
            existingData = new SaveData(
                Vector3.zero, "", 0, 0, 0,
                new List<InventorySaveData>(),
                new List<int>(),                                    // CHANGED: was new List<InventorySaveData>()
                new List<QuestProgressSaveData>(),
                new List<string>(),
                new List<SaveController.CollectedItemData>(),
                new List<string>(),
                new List<string>(),
                portfolioManager.ToSaveData()                       // ADDED: portfolio
            );
        }

        // ONLY patch these three — quest progress in JSON stays untouched
        existingData.defeatedEnemyIDs = new List<string>(defeatedEnemies);
        existingData.visitedLocationIDs = new List<string>(visitedLocations);
        existingData.collectedWorldItems = new List<SaveController.CollectedItemData>(collectedItemInstances);

        File.WriteAllText(saveLocation, JsonUtility.ToJson(existingData, true));
        Debug.Log("Persisted world state to save file.");
    }

    public void AddCollectedItem(string instanceID, int itemID)
    {
        collectedItemInstances.Add(new CollectedItemData { instanceID = instanceID, itemID = itemID });

        // restore if going wrong if (!string.IsNullOrEmpty(uniqueID))
        //    collectedItems.Add(uniqueID);
    }

    //// Get count of collected items for a given item ID (used by quests)
    public int GetCollectedCount(int itemID)
    {
        int count = 0;
        foreach (var data in collectedItemInstances)
            if (data.itemID == itemID) count++;
        return count;
    }

    public void DisableCollectedQuestItems()
    {
        Item[] allWorldItems = FindObjectsByType<Item>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        Transform inventoryPanelTransform = InventoryController.Instance?.inventoryPanel?.transform;

        foreach (Item wi in allWorldItems)
        {
            if (inventoryPanelTransform != null && wi.transform.IsChildOf(inventoryPanelTransform))
                continue;

            bool isCollected = collectedItemInstances.Exists(data => data.instanceID == wi.instanceID);
            if (isCollected)
            {
                wi.gameObject.SetActive(false);
            }
        }
    }

    public void AddDefeatedEnemy(string enemyID)
    {
        defeatedEnemies.Add(enemyID);
        Debug.Log("Added enemy");
    }

    public bool IsEnemyDefeated(string enemyID)
    {
        return defeatedEnemies.Contains(enemyID);
    }

    public void AddVisitedLocation(string locationID)
    {
        visitedLocations.Add(locationID);
    }

    public int GetKillCount(string enemyID)
    {
        int count = 0;
        foreach (var id in defeatedEnemies)
            if (id == enemyID) count++;
        return count;
    }

    public int GetVisitCount(string locationID)
    {
        int count = 0;
        foreach (var id in visitedLocations)
            if (id == locationID) count++;
        return count;
    }

    // Call this after loading data
    private void ApplyWorldState()
    {
        // Disable all enemies that have been killed
        //foreach (var enemy in FindObjectsByType<EnemyStats>(FindObjectsSortMode.None))
        //{
        //    if (defeatedEnemies.Contains(enemy.enemyID))
        //        enemy.gameObject.SetActive(false);
        //}

        // Disable all location triggers that have been visited
        foreach (var trigger in FindObjectsByType<LocationTrigger>(FindObjectsSortMode.None))
        {
            if (visitedLocations.Contains(trigger.locationID))
                trigger.gameObject.SetActive(false);
        }
    }

    public static void AutoSave()                                   // CHANGED: was instance method, now static
    {
        // Avoid saving too frequently – use a timer or just save immediately
        Instance?.SaveGame();
    }

    public void ClearSaveGame()
    {
        if (File.Exists(saveLocation))
        {
            File.Delete(saveLocation);
            Debug.Log("Save data cleared.");
        }

        // Reset in memory collections
        collectedItemInstances.Clear();
        defeatedEnemies.Clear();
        visitedLocations.Clear();
        // Optionally re enable all world objects (they will be active by default)
        foreach (var item in FindObjectsByType<Item>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            item.gameObject.SetActive(true);
        foreach (var enemy in FindObjectsByType<EnemyStats>(FindObjectsSortMode.None))
            enemy.gameObject.SetActive(true);
        foreach (var loc in FindObjectsByType<LocationTrigger>(FindObjectsSortMode.None))
            loc.gameObject.SetActive(true);

        Debug.Log("In memory state cleared.");
        LoadGame();
    }

    public PortfolioManager GetPortfolioManager()                   // ADDED: from Branch 2
    {
        return portfolioManager;
    }
}