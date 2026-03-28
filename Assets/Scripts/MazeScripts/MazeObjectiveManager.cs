using UnityEngine;
using TMPro;

public class MazeObjectiveManager : MonoBehaviour
{
    public static MazeObjectiveManager Instance;
    
    [Header("Maze Settings")]
    public int totalRoomsInMaze = 4; // Set this in Inspector
    public int roomsCleared = 0;
    
    public bool mazeComplete = false;
    
    [Header("UI Reference (Optional)")]
    public TextMeshProUGUI progressText; // "Rooms: 1/4"
    
    private void Awake()
    {
        Instance = this;
    }
    
    private void Start()
    {
        roomsCleared = 0;
        mazeComplete = false;
        UpdateUI();
    }
    
    public void OnRoomCleared()
    {
        if (mazeComplete) return;
        
        roomsCleared++;
        Debug.Log($"✅ Rooms cleared: {roomsCleared}/{totalRoomsInMaze}");
        
        UpdateUI();
        
        // Check if maze is complete
        if (roomsCleared >= totalRoomsInMaze)
        {
            CompleteMaze();
        }
    }
    
    void CompleteMaze()
    {
        mazeComplete = true;
        Debug.Log("🎉 MAZE COMPLETE! All rooms cleared!");
        
        // Show message
        if (MazeUIController.Instance != null)
        {
            MazeUIController.Instance.ShowCustomMessage(
                "✨ All rooms cleared! Portal activated! ✨",
                3f
            );
        }
        
        // Activate portal in CURRENT room
        if (RoomManager.Instance != null)
        {
            RoomManager.Instance.ActivatePortalInCurrentRoom();
        }
    }
    
    void UpdateUI()
    {
        if (progressText != null)
        {
            progressText.text = $"Rooms: {roomsCleared}/{totalRoomsInMaze}";
        }
    }
    
    public void ResetObjective()
    {
        roomsCleared = 0;
        mazeComplete = false;
        UpdateUI();
    }
}