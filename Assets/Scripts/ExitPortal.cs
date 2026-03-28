using UnityEngine;

public class ExitPortal : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        
        // Check if maze is complete
        if (MazeObjectiveManager.Instance != null && 
            MazeObjectiveManager.Instance.mazeComplete)
        {
            
            RoomManager.Instance.OnPlayerEnterPortal();
        }
    }
}