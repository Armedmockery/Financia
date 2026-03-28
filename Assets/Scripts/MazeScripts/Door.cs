// using UnityEngine;

// public class Door : MonoBehaviour
// {
//     public DoorDirection direction;
//     private Transform spawnPoint;

//     private RoomController room;
//     private bool isActive = true;
//     public Transform SpawnPoint 
//     { 
//         get 
//         {
//             if (spawnPoint == null)
//                 spawnPoint = transform.Find("SpawnPoint");
//             return spawnPoint;
//         }
//     }


//     void Start()
// {
//     room = GetComponentInParent<RoomController>();
//     spawnPoint = GetComponentInChildren<Transform>().Find("SpawnPoint");
    
//     // DEBUG: Verify what was found
//     if (spawnPoint != null)
//     {
//         Debug.Log($"Door {direction} found spawn point at {spawnPoint.position} with parent {spawnPoint.parent.name}");
//     }
//     else
//     {
//         Debug.LogError($"Door {direction} could NOT find SpawnPoint!");
//     }
// }



//    private RoomController GetRoom()
// {
//     return GetComponentInParent<RoomController>();
// }

// private void OnTriggerEnter2D(Collider2D other)
// {
//     if (!isActive) return;
//     if (!other.CompareTag("Player")) return;

//     isActive = false;
    
//     RoomController currentRoom = GetRoom(); // Get fresh reference!
    
//     Debug.Log($"Player entered {direction} door of room at {currentRoom.gridPosition}");
    
//     RoomManager.Instance.MoveThroughDoor(currentRoom, direction, other.transform);
//     Invoke(nameof(ResetDoor), 0.5f);
// }

// void ResetDoor()
// {
//     isActive = true;
// }


// }
using UnityEngine;

public class Door : MonoBehaviour
{
    public DoorDirection direction;
    private Transform spawnPoint;
    private RoomController room;
    public bool isReturnPath = false; // NEW: Mark the door player came from
    private bool isActive = true;
    private bool isLocked = false;
    
    public Transform SpawnPoint 
    { 
        get 
        {
            if (spawnPoint == null)
                spawnPoint = transform.Find("SpawnPoint");
            return spawnPoint;
        }
    }

    void Start()
    {
        room = GetComponentInParent<RoomController>();
    }

    public void LockDoor()
    {
        isLocked = true;
        isActive = false;
        // Visual feedback
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = Color.gray;
    }

    public void UnlockDoor()
    {
        isLocked = false;
        isActive = true;
        // Visual feedback
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = Color.white;
    }

    public void SetDoorActive(bool active)
    {
        isActive = active;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isActive) return;
        if (isLocked) return;
        if (!other.CompareTag("Player")) return;

        // For tax doors, this will be handled by TaxDoor component
        // For normal doors, use original behavior
        if (GetComponent<TaxDoor>() == null)
        {
            RoomController currentRoom = GetComponentInParent<RoomController>();
            RoomManager.Instance.MoveThroughDoor(currentRoom, direction, other.transform);
        }
    }
}