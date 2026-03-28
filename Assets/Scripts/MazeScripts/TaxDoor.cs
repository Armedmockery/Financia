using UnityEngine;
using TMPro;

public class TaxDoor : MonoBehaviour
{
    public string choiceText;
    public int wisdomValue;
    public string explanation;
    
    private GameObject doorLabelPrefab;
    public Vector3 labelOffset = new Vector3(0, 0, 0);
    
    [Header("Door Colors")]
    public Color lockedColor = new Color(0.937f, 0.560f, 0.102f); // #EF8F1A (orange)
    public Color unlockedColor = new Color(0.106f, 0.714f, 0.024f); // #1BB606 (green)
    public Color chosenColor = new Color(0.106f, 0.714f, 0.024f); // Same as unlocked for now
    
    private Door doorComponent;
    private TextMeshPro label;
    private GameObject labelInstance;
    private bool isChosen = false;
    private RoomController parentRoom;
    private bool isUnlocked = false;
    
    public void Initialize(RoomController parent, GameObject doorLabelPrefab)
    {
        this.doorLabelPrefab = doorLabelPrefab;
        doorComponent = GetComponent<Door>();
        parentRoom = parent;
        
        // Check if this is a return path door
        if (doorComponent.isReturnPath)
        {
            Debug.Log($"Door {doorComponent.direction} is return path - removing TaxDoor");
            Destroy(this);
            return;
        }
        
        // Start with door LOCKED
        doorComponent.LockDoor();
        isUnlocked = false;
        
        CreateDoorLabel();
    }
    
    void CreateDoorLabel()
    {
        if (doorComponent == null)
        {
            Debug.LogError("doorComponent is null!");
            return;
        }
        
        Transform spawnPoint = doorComponent.SpawnPoint;
        if (spawnPoint == null) return;
        
        // If prefab exists, use it
        if (doorLabelPrefab != null)
        {
            // Instantiate the prefab
            labelInstance = Instantiate(doorLabelPrefab, spawnPoint.position + labelOffset, Quaternion.identity);
            labelInstance.transform.SetParent(transform);
            
            // Set sorting layer for EVERY renderer in the label
            Renderer[] allRenderers = labelInstance.GetComponentsInChildren<Renderer>();
            foreach (Renderer r in allRenderers)
            {
                r.sortingOrder = 100;
            }
            
            // Debug: Find all components
            Debug.Log($"=== Searching for TextMeshPro in {labelInstance.name} ===");
            Component[] allComponents = labelInstance.GetComponentsInChildren<Component>();
            Debug.Log($"Total components found: {allComponents.Length}");

            foreach (Component comp in allComponents)
            {
                Debug.Log($"Component on {comp.gameObject.name}: {comp.GetType().Name}");
                
                // Try to cast to TextMeshPro
                if (comp is TextMeshPro)
                {
                    label = comp as TextMeshPro;
                    Debug.Log($"✅ Found TextMeshPro on {comp.gameObject.name}");
                    break;
                }
            }

            if (label != null)
            {
                label.text = choiceText;
                label.color = lockedColor;
                label.fontSize = 36;
                label.alignment = TextAlignmentOptions.Center;
                label.ForceMeshUpdate();
                Debug.Log($"✅ Text set to: '{choiceText}'");
            }
            else
            {
                Debug.LogError("❌ No TextMeshPro component found in prefab! Check prefab structure.");
            }
            
            // Add billboard if not already in prefab
            if (labelInstance.GetComponent<Billboard>() == null)
            {
                labelInstance.AddComponent<Billboard>();
            }
            
            Debug.Log($"Created label from prefab for {doorComponent.direction} door: {choiceText}");
        }
        else
        {
            Debug.LogWarning("doorLabelPrefab not assigned!");
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        
        // Door is still locked? Can't use
        if (!isUnlocked)
        {
            Debug.Log($"Door {doorComponent.direction} is locked - clear room first!");
            return;
        }
        
        if (isChosen) return;
        if (parentRoom == null) return;
        
        isChosen = true;
        
        Debug.Log($"✅ Player chose: {choiceText} on {doorComponent.direction} door");
        
        // Record choice
        if (TaxChoiceTracker.Instance != null)
        {
            TaxChoiceTracker.Instance.RecordChoice(
                parentRoom,
                choiceText,
                wisdomValue,
                explanation
            );
        }
        
        // Teleport player to next room
        if (RoomManager.Instance != null)
        {
            RoomManager.Instance.MoveThroughDoor(
                parentRoom, 
                doorComponent.direction, 
                other.transform
            );
        }
        
        // Change label color to chosen color (green) when chosen
        if (label != null)
            label.color = chosenColor;
    }
    
    // Called by RoomManager when room is cleared
    public void UnlockDoor()
    {
        isUnlocked = true;
        doorComponent.UnlockDoor();
        Debug.Log($"✅ Door {doorComponent.direction} unlocked!");
        
        // Change label color to unlocked color (GREEN) when unlocked
        if (label != null)
            label.color = unlockedColor;
    }
    
    // Called by RoomTaxData after setting up the door
    public void UpdateChoiceText(string newText)
    {
        choiceText = newText;
        
        // Update the label text if it exists
        if (label != null)
        {
            label.text = choiceText;
            label.ForceMeshUpdate();
            Debug.Log($"🔄 Updated label text to: {choiceText}");
        }
    }
}

public class Billboard : MonoBehaviour
{
    void LateUpdate()
    {
        if (Camera.main != null)
            transform.forward = Camera.main.transform.forward;
    }
}