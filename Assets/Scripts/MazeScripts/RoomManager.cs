using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;
using System.Collections;


public class RoomManager : MonoBehaviour
{
    // public RoomController startRoomPrefab;
    public List<RoomController> roomPrefabs;
    public CinemachineConfiner2D confiner;

    public RoomController startRoomInScene;

    private Dictionary<Vector2Int, RoomController> spawnedRooms =
        new Dictionary<Vector2Int, RoomController>();

    private RoomController currentRoom;
    public CinemachineCamera cineCam;
    public RoomController CurrentRoom
    {
        get { return currentRoom; }
    }



    public int roomSize = 20;
    public static RoomManager Instance;
    private List<RoomController> remainingRooms;

    [Header("Portal")]
public GameObject exitPortal;


 
[Header("Enemy Prefabs")]
public GameObject poisonSlimePrefab;
public GameObject lavaSlimePrefab;

public int minEnemies = 2;
public int maxEnemies = 4;

[Header("Document Drop System")]
public List<GameObject> documentPrefabs;   // Drag your document prefabs here

private int documentDropIndex = 0;




    void Awake()
    {
        Instance = this;
    }

IEnumerator Start()
{
    yield return null;

    startRoomInScene.gridPosition = Vector2Int.zero;
    spawnedRooms.Add(Vector2Int.zero, startRoomInScene);

    currentRoom = startRoomInScene;

    MinimapManager.Instance?.RegisterRoom(startRoomInScene);
    MinimapManager.Instance?.SetCurrentRoom(startRoomInScene);

    UpdateCameraBounds(currentRoom);
    remainingRooms = new List<RoomController>(roomPrefabs);
}

    // void SpawnStartRoom()
    // {
    //     RoomController room =
    //         Instantiate(startRoomPrefab, Vector3.zero, Quaternion.identity);

    //     room.gridPosition = Vector2Int.zero;

    //     spawnedRooms.Add(Vector2Int.zero, room);
    // }
    public void TrySpawnRoom(RoomController currentRoom, DoorDirection dir)
    {
        Vector2Int offset = GetOffset(dir);
        Vector2Int newPos = currentRoom.gridPosition + offset;
        if (remainingRooms.Count == 0)
    return;


        if (spawnedRooms.ContainsKey(newPos))
            return;

        SpawnRoom(newPos);
    }

    Vector2Int GetOffset(DoorDirection dir)
    {
        switch (dir)
        {
            case DoorDirection.North: return Vector2Int.up;
            case DoorDirection.South: return Vector2Int.down;
            case DoorDirection.East:  return Vector2Int.right;
            case DoorDirection.West:  return Vector2Int.left;
        }

        return Vector2Int.zero;
    }
    void SpawnRoom(Vector2Int gridPos)
{
    
    if (remainingRooms.Count == 0)
        return;

    int index = Random.Range(0, remainingRooms.Count);
    RoomController prefab = remainingRooms[index];

    remainingRooms.RemoveAt(index);

    Vector3 worldPos =
        new Vector3(gridPos.x * roomSize, gridPos.y * roomSize, 0);

    RoomController newRoom =
        Instantiate(prefab, worldPos, Quaternion.identity);

    newRoom.gridPosition = gridPos;

    MinimapManager.Instance?.RegisterRoom(newRoom);

    spawnedRooms.Add(gridPos, newRoom);
}
DoorDirection GetOppositeDirection(DoorDirection dir)
{
    switch (dir)
    {
        case DoorDirection.North: return DoorDirection.South;
        case DoorDirection.South: return DoorDirection.North;
        case DoorDirection.East: return DoorDirection.West;
        case DoorDirection.West: return DoorDirection.East;
        default: return DoorDirection.North;
    }
}

// 2. Get door component by direction
Door GetDoorInDirection(RoomController room, DoorDirection dir)
{
    switch (dir)
    {
        case DoorDirection.North: 
            return room.northDoor?.GetComponent<Door>();
        case DoorDirection.South: 
            return room.southDoor?.GetComponent<Door>();
        case DoorDirection.East:  
            return room.eastDoor?.GetComponent<Door>();
        case DoorDirection.West:  
            return room.westDoor?.GetComponent<Door>();
        default: 
            return null;
    }
}
public void MoveThroughDoor(RoomController roomFromDoor,
                            DoorDirection dir,
                            Transform player)
{
    Vector2Int offset = GetOffset(dir);
    Vector2Int newPos = roomFromDoor.gridPosition + offset;

    if (!spawnedRooms.ContainsKey(newPos))
    {
        if (remainingRooms.Count == 0)
            return;
        SpawnRoom(newPos);
    }
    
    RoomController nextRoom = spawnedRooms[newPos];

    DoorDirection oppositeDir = GetOppositeDirection(dir);
    Door returnDoor = GetDoorInDirection(nextRoom, oppositeDir);
    if (returnDoor != null)
    {
        returnDoor.isReturnPath = true;
        Debug.Log($"✅ MARKED RETURN PATH: In room {nextRoom.gridPosition}, {oppositeDir} door is return path");
    }
    else
    {
        Debug.LogError($"❌ Could not find door in direction {oppositeDir} for room {nextRoom.gridPosition}");
    }


    Transform entryPoint = GetOppositeDoorSpawn(nextRoom, dir);
    if (entryPoint == null)
{
    Debug.LogError("Entry point not found!");
    return;
}
    
    // FIX: Add offset to move player TOWARD the door they just came from
    Vector3 spawnPosition = entryPoint.position;
    
    // Add offset based on direction to place player just inside the doorway
    switch (dir)
    {
        case DoorDirection.North: // Coming from North, spawn at South door, move TOWARD North (up)
            spawnPosition.y += 1.5f; // Move UP toward the door they came from
            break;
        case DoorDirection.South: // Coming from South, spawn at North door, move TOWARD South (down)
            spawnPosition.y -= 1.5f; // Move DOWN toward the door they came from
            break;
        case DoorDirection.East: // Coming from East, spawn at West door, move TOWARD East (right)
            spawnPosition.x += 1.5f; // Move RIGHT toward the door they came from
            break;
        case DoorDirection.West: // Coming from West, spawn at East door, move TOWARD West (left)
            spawnPosition.x -= 1.5f; // Move LEFT toward the door they came from
            break;
    }
    
    Debug.Log($"Moving {dir} from {roomFromDoor.gridPosition} to {nextRoom.gridPosition}");
    Debug.Log($"Original spawn: {entryPoint.position}, Adjusted spawn: {spawnPosition}");

    currentRoom = nextRoom;
    MinimapManager.Instance?.SetCurrentRoom(nextRoom);
    UpdateCameraBounds(currentRoom);
    
    // Teleport player to adjusted position
    player.position = spawnPosition;
    
    if (cineCam != null)
    {
        cineCam.ForceCameraPosition(spawnPosition, Quaternion.identity);
    }
    
    Physics2D.SyncTransforms();
    Debug.Log($"Spawn point parent: {entryPoint.parent.parent.name}");

    HandleRoomEntry(currentRoom);

}
void LockRoomDoors(RoomController room)
{
    // Use LockDoor() instead of SetActive(false)
    if (room.northDoor != null)
        room.northDoor.GetComponent<Door>()?.LockDoor();

    if (room.southDoor != null)
        room.southDoor.GetComponent<Door>()?.LockDoor();

    if (room.eastDoor != null)
        room.eastDoor.GetComponent<Door>()?.LockDoor();

    if (room.westDoor != null)
        room.westDoor.GetComponent<Door>()?.LockDoor();
}

    Transform GetOppositeDoorSpawn(RoomController room, DoorDirection enteredDirection)
{
    // When you enter through a door, you spawn at the OPPOSITE door in the new room
    switch (enteredDirection)
    {
        case DoorDirection.North: 
            // You entered through North, so you should spawn at the South door of the new room
            return room.southDoor.GetComponent<Door>().SpawnPoint;
            
        case DoorDirection.South: 
            // You entered through South, so spawn at North door
            return room.northDoor.GetComponent<Door>().SpawnPoint;
            
        case DoorDirection.East:  
            // You entered through East, so spawn at West door
            return room.westDoor.GetComponent<Door>().SpawnPoint;
            
        case DoorDirection.West:  
            // You entered through West, so spawn at East door
            return room.eastDoor.GetComponent<Door>().SpawnPoint;
    }

    return null;
}
public void UpdateCameraBounds(RoomController room)
{
    confiner.BoundingShape2D = room.roomBounds;
    confiner.InvalidateBoundingShapeCache();
}


void HandleRoomEntry(RoomController room)
{
    
    if (room.isCleared)
        return;

    if (room.isCombatRoom)
    {
        StartCoroutine(StartCombatRoutine(room));
    }
    else
    {
        // NON-COMBAT ROOM - clear immediately or after player action
        Debug.Log("Non-combat room - marking as cleared");
        
        // If you want instant clear:
         StartCoroutine(ClearNonCombatRoom(room));
        
        // If you want player to click something first:
        // Show some UI, then call RoomCleared when done
    }
}
IEnumerator ClearNonCombatRoom(RoomController room)
{
    yield return new WaitForSeconds(0.5f); // Small delay
    RoomManager.Instance.OnRoomCleared(room);
}
public void ReturnToCity()
{
    Debug.Log("Returning to City...");
    
    // You can call the MazeLifeManager's ReturnToCity if it exists
    if (MazeLifeManager.Instance != null)
    {
        MazeLifeManager.Instance.ReturnToCity();
    }
    else
    {
        // Fallback
        Time.timeScale = 1f;
        // SceneManager.LoadScene("CityScene");
    }
}

public void ActivatePortalInCurrentRoom()
{
    Debug.Log($"🔍 ActivatePortalInCurrentRoom called! exitPortal = {exitPortal}");
    
    if (exitPortal == null)
    {
        Debug.LogError("❌ exitPortal is NULL! Drag the portal prefab to RoomManager inspector!");
        return;
    }
    
    if (currentRoom == null)
    {
        Debug.LogError("❌ currentRoom is NULL!");
        return;
    }
    
    // Get spawn position
    Vector3 spawnPos = currentRoom.roomCenter != null 
        ? currentRoom.roomCenter.position 
        : currentRoom.transform.position;
    
    Debug.Log($"📍 Spawning portal at: {spawnPos}");
    
    // Check if we should instantiate or just move existing
    if (!exitPortal.activeSelf || exitPortal.scene.name == null)
    {
        // Instantiate new portal
        GameObject newPortal = Instantiate(exitPortal, spawnPos, Quaternion.identity);
        newPortal.SetActive(true);
        Debug.Log($"✅ New portal instantiated: {newPortal.name} at {newPortal.transform.position}");
    }
    else
    {
        // Move existing portal
        exitPortal.transform.position = spawnPos;
        exitPortal.SetActive(true);
        Debug.Log($"✅ Existing portal moved to: {spawnPos}");
    }
}
// Called when player touches portal
public void OnPlayerEnterPortal()
{
    if (MazeObjectiveManager.Instance != null && !MazeObjectiveManager.Instance.mazeComplete) 
    {
        Debug.Log("Portal not active yet!");
        return;
    }
    
    // Get data
    int wisdom = TaxChoiceTracker.Instance?.totalWisdomScore ?? 0;
    int roomsCleared = MazeObjectiveManager.Instance?.roomsCleared ?? 0;
    int totalRooms = MazeObjectiveManager.Instance?.totalRoomsInMaze ?? 4;
    
    // Show completion dialog
    if (MazeDialogUI.Instance != null)
    {
        MazeDialogUI.Instance.ShowCompletionDialog(
            wisdom,
            roomsCleared,
            totalRooms
        );
    }
}

public IEnumerator StartCombatRoutine(RoomController room)
{
    // Small delay so player fully spawns before combat
    yield return new WaitForSeconds(0.2f);

    LockRoomDoors(room);
    SpawnEnemiesInRoom(room);
}


void SpawnEnemiesInRoom(RoomController room)
{
    int enemyCount = Random.Range(minEnemies, maxEnemies + 1);

    room.enemiesInRoom.Clear();
    room.totalEnemiesSpawned = enemyCount;

    int poisonCount = 0;
    int lavaCount = 0;

    for (int i = 0; i < enemyCount; i++)
    {
        GameObject prefabToSpawn = null;

        switch (room.enemySpawnType)
        {
            case EnemySpawnType.PoisonOnly:
                prefabToSpawn = poisonSlimePrefab;
                poisonCount++;
                break;

            case EnemySpawnType.LavaOnly:
                prefabToSpawn = lavaSlimePrefab;
                lavaCount++;
                break;

            case EnemySpawnType.Mixed:
                if (Random.value > 0.5f)
                {
                    prefabToSpawn = poisonSlimePrefab;
                    poisonCount++;
                }
                else
                {
                    prefabToSpawn = lavaSlimePrefab;
                    lavaCount++;
                }
                break;
        }

        Vector3 spawnPos = GetRandomPointInRoom(room);
        GameObject enemy = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);

        EnemyStatsMaze stats = enemy.GetComponent<EnemyStatsMaze>();
        if (stats != null)
        {
            stats.assignedRoom = room;
            room.enemiesInRoom.Add(stats);
        }
    }

    // 🔥 Build UI type text dynamically
    string typeText = GetTypeText(poisonCount, lavaCount);

    if (MazeUIController.Instance != null)
    {
        MazeUIController.Instance.ShowCombatUI(enemyCount, enemyCount, typeText);
    }
}

string GetTypeText(int poisonCount, int lavaCount)
{
    if (poisonCount > 0 && lavaCount > 0)
        return "Poison, Lava";
    else if (poisonCount > 0)
        return "Poison";
    else if (lavaCount > 0)
        return "Lava";
    else
        return "Unknown";
}


Vector3 GetRandomPointInRoom(RoomController room)
{
    // Get room center
    Vector3 center = room.roomCenter != null 
        ? room.roomCenter.position 
        : room.transform.position;
    
    // Spawn within a radius from center
    float spawnRadius = 5f; // Adjust this value as needed
    
    float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
    float distance = Random.Range(2f, spawnRadius); // Min 2f to avoid clumping
    
    Vector3 offset = new Vector3(
        Mathf.Cos(angle) * distance,
        Mathf.Sin(angle) * distance,
        0f
    );
    
    return center + offset;
}
// Add this to your RoomManager.cs

// Call this when room is cleared
public void OnRoomCleared(RoomController room)
{
    Debug.Log("Room Cleared!");
    
    room.isCleared = true;
    
    // SPAWN REWARDS for clearing this room
    RoomTaxData taxData = room.GetComponent<RoomTaxData>();
    if (taxData != null)
    {
        taxData.SpawnRoomRewards();
    }

    if (MinimapManager.Instance != null)
    {
        MinimapManager.Instance.UpdateRoomAppearance(room);
    }
    
    // UNLOCK all tax doors
    UnlockTaxDoors(room);
    // ✅ NOTIFY OBJECTIVE MANAGER
    if (MazeObjectiveManager.Instance != null)
    {
        MazeObjectiveManager.Instance.OnRoomCleared();
    }
    
    if (room.isCombatRoom && MazeUIController.Instance != null)
    {
        MazeUIController.Instance.ShowRoomCleared();
    }
}

void UnlockTaxDoors(RoomController room)
{
    TaxDoor[] taxDoors = room.GetComponentsInChildren<TaxDoor>();
    foreach (TaxDoor taxDoor in taxDoors)
    {
        taxDoor.UnlockDoor();
    }
}



// Helper method to lock all doors (called from TaxDoor)
public void LockAllDoorsInRoom(RoomController room)
{
    Door[] doors = room.GetComponentsInChildren<Door>();
    foreach (Door door in doors)
    {
        door.LockDoor();
    }
}

void DropDocument(RoomController room)
{
    if (documentPrefabs == null || documentPrefabs.Count == 0)
        return;

    if (documentDropIndex >= documentPrefabs.Count)
        return; // No more documents to drop

    GameObject prefab = documentPrefabs[documentDropIndex];
    documentDropIndex++;

    Vector3 spawnPos = room.roomCenter != null
        ? room.roomCenter.position
        : room.transform.position;

    Instantiate(prefab, spawnPos, Quaternion.identity);

    Debug.Log("Dropped document: " + prefab.name);
}



void UnlockRoomDoors(RoomController room)
{
    if (room.northDoor != null)
        room.northDoor.GetComponent<Door>()?.UnlockDoor();

    if (room.southDoor != null)
        room.southDoor.GetComponent<Door>()?.UnlockDoor();

    if (room.eastDoor != null)
        room.eastDoor.GetComponent<Door>()?.UnlockDoor();

    if (room.westDoor != null)
        room.westDoor.GetComponent<Door>()?.UnlockDoor();
}
public void ResetMaze()
{
    Debug.Log("🔄 COMPLETE MAZE RESET STARTING...");
    
    // ===== 1. DESTROY ALL ENEMIES (including inactive) =====
    EnemyStatsMaze[] allEnemies = FindObjectsOfType<EnemyStatsMaze>(true); // true = include inactive
    foreach (EnemyStatsMaze enemy in allEnemies)
    {
        if (enemy != null && enemy.gameObject != null)
            Destroy(enemy.gameObject);
    }
    
    // ===== 2. DESTROY ALL DROPPED ITEMS/REWARDS =====
    TaxRewardPickup[] allRewards = FindObjectsOfType<TaxRewardPickup>(true);
    foreach (TaxRewardPickup reward in allRewards)
    {
        if (reward != null && reward.gameObject != null)
            Destroy(reward.gameObject);
    }
    
    // Also destroy any other items with specific tags
    GameObject[] allObjects = FindObjectsOfType<GameObject>(true);
    foreach (GameObject obj in allObjects)
    {
        // Destroy any dropped items (tag them as "Reward" or "Item" in inspector)
        if (obj.CompareTag("Item"))
        {
            Destroy(obj);
        }
    }
    
    // ===== 3. DESTROY ALL NON-START ROOMS =====
    foreach (var room in spawnedRooms.Values)
    {
        foreach (var enemy in room.enemiesInRoom)
        {
            if (enemy != null)
                Destroy(enemy.gameObject);
        }
        room.enemiesInRoom.Clear();
        room.isCleared = false;
    }
    
    // Destroy all non-start rooms
    foreach (var room in spawnedRooms.Values)
    {
        if (room != startRoomInScene)
            Destroy(room.gameObject);
    }
    
    spawnedRooms.Clear();
    
    // ===== 4. RESET START ROOM =====
    startRoomInScene.isCleared = false;
    startRoomInScene.gridPosition = Vector2Int.zero;
    startRoomInScene.enemiesInRoom.Clear();
    
    // Remove any dynamic TaxDoor components from start room
    TaxDoor[] taxDoors = startRoomInScene.GetComponentsInChildren<TaxDoor>(true); // Include inactive
    foreach (TaxDoor td in taxDoors)
    {
        if (Application.isPlaying)
            Destroy(td);
    }
    
    // Reset door states in start room
    Door[] doors = startRoomInScene.GetComponentsInChildren<Door>(true);
    foreach (Door door in doors)
    {
        door.isReturnPath = false;
        door.UnlockDoor(); // Make sure doors are unlocked
    }
    
    spawnedRooms.Add(Vector2Int.zero, startRoomInScene);
    currentRoom = startRoomInScene;
    
    // ===== 5. RESET ALL MANAGERS =====
    MinimapManager.Instance?.ResetMap();
    MinimapManager.Instance?.RegisterRoom(startRoomInScene);
    MinimapManager.Instance?.SetCurrentRoom(startRoomInScene);
    
    // Reset objective
    if (MazeObjectiveManager.Instance != null)
        MazeObjectiveManager.Instance.ResetObjective();
    
    // Reset tax tracker
    if (TaxChoiceTracker.Instance != null)
        TaxChoiceTracker.Instance.ResetTracker();
    
    // ===== 6. DISABLE PORTAL =====
    if (exitPortal != null)
    {
        // If it's an instance, destroy it
        if (exitPortal.scene.name != null) // It's in the scene
        {
            Destroy(exitPortal);
            exitPortal = null;
        }
        else // It's a prefab reference
        {
            exitPortal.SetActive(false);
        }
    }
    
    // Also find any portal in scene and destroy it
    ExitPortal[] portals = FindObjectsOfType<ExitPortal>(true);
    foreach (ExitPortal portal in portals)
    {
        Destroy(portal.gameObject);
    }
    
    // ===== 7. RESET ROOM PREFAB LIST =====
    remainingRooms = new List<RoomController>(roomPrefabs);
    documentDropIndex = 0;
    
    // ===== 8. RESET CAMERA =====
    UpdateCameraBounds(startRoomInScene);
    
    // ===== 9. HIDE UI =====
    MazeUIController.Instance?.HideUIImmediate();
    
    // ===== 10. FORCE GARBAGE COLLECTION (optional, use sparingly) =====
    // Resources.UnloadUnusedAssets();
    
    Debug.Log("✅ MAZE FULLY RESET. Ready for new run!");
}
public void ForceCameraToStart()
{
    if (cineCam != null)
    {
        Vector3 pos = startRoomInScene.roomCenter.position;
        cineCam.ForceCameraPosition(pos, Quaternion.identity);
    }
}

public void ResetSingleRoom(RoomController room)
{
    if (room == null) return;
    
    Debug.Log($"🔄 Resetting single room: {room.gridPosition}");
    
    // Destroy all enemies in the room (including inactive)
    foreach (var enemy in room.enemiesInRoom)
    {
        if (enemy != null && enemy.gameObject != null)
            Destroy(enemy.gameObject);
    }
    
    // Also find any enemies that might be in this room but not in list
    EnemyStatsMaze[] enemiesInRoom = room.GetComponentsInChildren<EnemyStatsMaze>(true);
    foreach (EnemyStatsMaze enemy in enemiesInRoom)
    {
        Destroy(enemy.gameObject);
    }
    
    // Destroy any dropped items in this room
    TaxRewardPickup[] rewardsInRoom = room.GetComponentsInChildren<TaxRewardPickup>(true);
    foreach (TaxRewardPickup reward in rewardsInRoom)
    {
        Destroy(reward.gameObject);
    }
    
    room.enemiesInRoom.Clear();

    // Unlock doors
    UnlockRoomDoors(room);

    // Mark room as not cleared
    room.isCleared = false;
    
    // Hide combat UI
    MazeUIController.Instance?.HideUIImmediate();

    Debug.Log($"✅ Room reset at: {room.gridPosition}");
}

public void ForceSetCurrentRoom(RoomController room)
{
    currentRoom = room;
    UpdateCameraBounds(room);
}


}
