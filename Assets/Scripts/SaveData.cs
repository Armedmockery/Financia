using UnityEngine;
using System.Collections.Generic;
using Game.StockMarket;                                             // ADDED: for PortfolioSaveData

[System.Serializable]
public class SaveData
{
    public Vector3 playerPosition;
    public string mapBoundary;
    public MapArea currentMapArea;
    public int playerHealth;
    public int playerScore;
    public int playerCoins;
    public List<InventorySaveData> inventorySaveData;
    public List<int> hotbarReferences;                              // CHANGED: was List<InventorySaveData> hotbarSaveData
    public List<QuestProgressSaveData> questProgressData;
    public List<string> handedInQuestIDs;
    //public List<string> collectedWorldItemIDs;
    public List<string> defeatedEnemyIDs;   // enemyID added each time an enemy dies
    public List<string> visitedLocationIDs; // locationID added each time a location is triggered
    public List<SaveController.CollectedItemData> collectedWorldItems;   // use the serializable type
    public PortfolioSaveData portfolioSaveData;                     // ADDED: from Branch 2

    public SaveData(
        Vector3 playerPosition,
        string mapBoundary,
        int playerHealth,
        int playerScore,
        int playerCoins,
        List<InventorySaveData> inventorySaveData,
        List<int> hotbarReferences,                                 // CHANGED: was List<InventorySaveData> hotbarSaveData
        List<QuestProgressSaveData> questProgressData,
        List<string> handedInQuestIDs,
        List<SaveController.CollectedItemData> collectedWorldItems,
        List<string> defeatedEnemyIDs,
        List<string> visitedLocationIDs,
        PortfolioSaveData portfolioSaveData)                        // ADDED: from Branch 2
        //List<SaveController.CollectedItemData> collectedWorldItems)
    {
        this.playerPosition = playerPosition;
        this.mapBoundary = mapBoundary;
        this.playerHealth = playerHealth;
        this.playerScore = playerScore;
        this.playerCoins = playerCoins;
        this.inventorySaveData = inventorySaveData;
        this.hotbarReferences = hotbarReferences;                   // CHANGED: was hotbarSaveData
        this.questProgressData = questProgressData;
        this.handedInQuestIDs = handedInQuestIDs;
        this.collectedWorldItems = collectedWorldItems;
        this.defeatedEnemyIDs = defeatedEnemyIDs;
        this.visitedLocationIDs = visitedLocationIDs;
        //this.collectedWorldItems = collectedWorldItems;
        this.portfolioSaveData = portfolioSaveData;                 // ADDED: from Branch 2
    }
}