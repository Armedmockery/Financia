using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class TaxChoiceOption
{
    public string choiceText;      // What shows on door
    public int wisdomValue;         // -1, 0, or +1 for scoring
    public string explanation;      // For end summary
}

public class RoomTaxData : MonoBehaviour
{
    [Header("💰 REWARDS for clearing this room")]
    public List<GameObject> roomRewards;  // Drag your item prefabs here
    public int minRewards = 1;
    public int maxRewards = 2;
    
    [Header("🚪 Tax Choices for THIS Room")]
    public TaxChoiceOption[] roomChoices; // 3 choices

    [Header("Door Label Prefab")]
    public GameObject doorLabelPrefab;
    
    [Header("Runtime")]
    public List<TaxDoor> activeChoiceDoors = new List<TaxDoor>();
    private RoomController roomController;
    
    void Awake()
    {
        roomController = GetComponent<RoomController>();
    }
    
    void Start()
    {
        Invoke("SetupTaxDoors", 0.1f);
    }
    
    void SetupTaxDoors()
    {
        if (roomController == null) return;
        
        Door[] allDoors = GetComponentsInChildren<Door>();
        List<Door> availableDoors = new List<Door>();
        
        foreach (Door door in allDoors)
        {
            if (!door.isReturnPath)
                availableDoors.Add(door);
            else
            {
                // Remove TaxDoor from return path
                TaxDoor existing = door.GetComponent<TaxDoor>();
                if (existing != null) Destroy(existing);
            }
        }
        
        Debug.Log($"Room at {roomController.gridPosition}: Found {availableDoors.Count} doors for choices");
        
        // Assign choices to available doors
        for (int i = 0; i < availableDoors.Count && i < roomChoices.Length; i++)
        {
            Door door = availableDoors[i];
            
            // Remove existing TaxDoor
            TaxDoor existing = door.GetComponent<TaxDoor>();
            if (existing != null) Destroy(existing);
            
            // Add new TaxDoor with choice data
            TaxDoor taxDoor = door.gameObject.AddComponent<TaxDoor>();
            taxDoor.choiceText = roomChoices[i].choiceText;
            taxDoor.wisdomValue = roomChoices[i].wisdomValue;
            taxDoor.explanation = roomChoices[i].explanation;

            
            
            activeChoiceDoors.Add(taxDoor);
            // Force update the label if it already exists
            taxDoor.Initialize(roomController, doorLabelPrefab);
            
            Debug.Log($"Assigned '{roomChoices[i].choiceText}' to {door.direction} door");
        }
    }
    
    // Called when room is cleared - spawn rewards
    public void SpawnRoomRewards()
{
    if (roomRewards == null || roomRewards.Count == 0) return;
    
    int rewardCount = Random.Range(minRewards, maxRewards + 1);
    Debug.Log($"Spawning {rewardCount} rewards for room {roomController.gridPosition}");
    
    // Get the center position
    Vector3 centerPos = roomController.roomCenter != null 
        ? roomController.roomCenter.position 
        : roomController.transform.position;
    
    // Offset values - adjust these as needed
    float xOffset = 2.5f;  // Move right from center
    float yOffset = 1.5f;  // Move up from center
    
    for (int i = 0; i < rewardCount; i++)
    {
        GameObject rewardPrefab = roomRewards[Random.Range(0, roomRewards.Count)];
        
        // Simple offset from center
        Vector3 spawnPos = centerPos + new Vector3(xOffset, yOffset + (i * 1.2f), 0);
        
        Instantiate(rewardPrefab, spawnPos, Quaternion.identity);
    }
}
}