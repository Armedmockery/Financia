using UnityEngine;

public class MazeLifeManager : MonoBehaviour
{
    public static MazeLifeManager Instance;
    private bool isHandlingDeath = false;

    [Header("Life Settings")]
    public int maxLives = 3;
    private int currentLives;

    private PlayerStats player;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        currentLives = maxLives;
    }

    public void OnPlayerDeath(PlayerStats deadPlayer)
    {
        RoomController deathRoom = RoomManager.Instance.CurrentRoom;
        if (isHandlingDeath) return;

        isHandlingDeath = true;

        player = deadPlayer;
        currentLives--;
        MazeUIController.Instance.gameObject.SetActive(false);

        if (currentLives > 0)
        {
            // 🔥 Reset only that room
            RoomManager.Instance.ResetSingleRoom(deathRoom);

            HandleLifeLost();
        }
        else
        {
            HandleRunFailed();
        }
    }

    void HandleLifeLost()
    {
        Time.timeScale = 0f;

        
        MazeDialogUI.Instance.ShowMessage(
            $"Life Lost!\nLives Remaining: {currentLives}",
            ContinueAfterLifeLost
        );
    }

    void ContinueAfterLifeLost()
    {
        Time.timeScale = 1f;
        
        RespawnPlayer();
    }

    void RespawnPlayer()
    {

        // If player reference is null, try to find the player
    if (player == null)
    {
        player = FindObjectOfType<PlayerStats>();
        if (player == null)
        {
            Debug.LogError("❌ Cannot find player to respawn!");
            return;
        }
    }
        // Heal
        player.currentHealth = player.maxHealth;

        // Reset combat state safely
        player.SendMessage("OnRespawn", SendMessageOptions.DontRequireReceiver);

        // Teleport to start
        Vector3 spawnPos =
            RoomManager.Instance.startRoomInScene.roomCenter.position;

        player.transform.position = spawnPos;
        RoomManager.Instance.ForceSetCurrentRoom(RoomManager.Instance.startRoomInScene);
    MinimapManager.Instance?.SetCurrentRoom(RoomManager.Instance.startRoomInScene);
        //RoomManager.Instance.UpdateCameraBounds(RoomManager.Instance.startRoomInScene);
        

        // // Force camera update
        // RoomManager.Instance.ForceCameraToStart();

        // Make sure player active
        if (!player.gameObject.activeSelf)
            player.gameObject.SetActive(true);

        isHandlingDeath = false;
        player.isDead = false;
    }

    void HandleRunFailed()
{
    Time.timeScale = 0f;
    
    // Get data
    int wisdom = TaxChoiceTracker.Instance?.totalWisdomScore ?? 0;
    int roomsCleared = MazeObjectiveManager.Instance?.roomsCleared ?? 0;
    int totalRooms = MazeObjectiveManager.Instance?.totalRoomsInMaze ?? 4;
    
    // Show game over dialog
    if (MazeDialogUI.Instance != null)
    {
        MazeDialogUI.Instance.ShowGameOverDialog(
            wisdom,
            roomsCleared,
            totalRooms
        );
    }
}
public void RetryMaze()
{
    Time.timeScale = 1f;
    currentLives = maxLives;

    // Find player if reference is lost
    if (player == null)
        player = FindObjectOfType<PlayerStats>();
    
    // Reset tax tracker
    if (TaxChoiceTracker.Instance != null)
        TaxChoiceTracker.Instance.ResetTracker();
    
    RoomManager.Instance.ResetMaze();
    isHandlingDeath = false;
    
    RespawnPlayer();
}

    public void ReturnToCity()
    {
        Time.timeScale = 1f;
        isHandlingDeath = false;

        Debug.Log("Return to City selected");
        // SceneManager.LoadScene("CityScene");
    }
}