using UnityEngine;
using System.Collections.Generic;

public class MinimapManager : MonoBehaviour
{
    public static MinimapManager Instance;

    public RectTransform tileContainer;
    public MinimapTile tilePrefab;

    public float tileSpacing = 25f;
    private int radius = 1; // 3x3 window

    [Header("Room Icons (Pre-colored)")]
    public Sprite startIcon;           // Your start room icon (already colored)
    public Sprite combatUnclearedIcon; // Red combat icon
    public Sprite combatClearedIcon;   // Green combat icon
    public Sprite defaultIcon;         // Your default icon for non-combat

    private Dictionary<Vector2Int, MinimapTile> tiles =
        new Dictionary<Vector2Int, MinimapTile>();

    private Vector2Int currentPlayerPos;
    private RoomController currentRoom;

    void Awake()
    {
        Instance = this;
    }

    public void RegisterRoom(RoomController room)
    {
        if (tiles.ContainsKey(room.gridPosition))
            return;

        MinimapTile tile = Instantiate(tilePrefab, tileContainer);
        tiles.Add(room.gridPosition, tile);

        UpdateTileAppearance(room);
        RefreshWindow();
    }

    public void SetCurrentRoom(RoomController room)
    {
        // Remove highlight from previous current room
        if (currentRoom != null && tiles.ContainsKey(currentRoom.gridPosition))
        {
            MinimapTile prevTile = tiles[currentRoom.gridPosition];
            prevTile.SetCurrent(false);
        }

        currentRoom = room;
        currentPlayerPos = room.gridPosition;

        // Highlight new current room
        if (tiles.ContainsKey(currentPlayerPos))
        {
            MinimapTile currentTile = tiles[currentPlayerPos];
            currentTile.SetCurrent(true);
        }

        RefreshWindow();
    }

    void RefreshWindow()
    {
        foreach (var kvp in tiles)
        {
            Vector2Int pos = kvp.Key;
            MinimapTile tile = kvp.Value;

            Vector2Int offset = pos - currentPlayerPos;

            if (Mathf.Abs(offset.x) <= radius &&
                Mathf.Abs(offset.y) <= radius)
            {
                tile.gameObject.SetActive(true);

                tile.transform.localPosition =
                    new Vector3(
                        offset.x * tileSpacing,
                        offset.y * tileSpacing,
                        0f
                    );
            }
            else
            {
                tile.gameObject.SetActive(false);
            }
        }
    }

    void UpdateTileAppearance(RoomController room)
    {
        if (!tiles.ContainsKey(room.gridPosition)) return;

        MinimapTile tile = tiles[room.gridPosition];

        // Set icon based on room type and state
        if (room == RoomManager.Instance.startRoomInScene)
        {
            tile.SetIcon(startIcon);
        }
        else if (room.isCombatRoom)
        {
            // Choose between red or green based on cleared state
            Sprite combatIcon = room.isCleared ? combatClearedIcon : combatUnclearedIcon;
            tile.SetIcon(combatIcon);
        }
        else // Non-combat rooms
        {
            tile.SetIcon(defaultIcon);
        }
        
        // NO color tinting needed - icons already have their colors!
    }

    public void UpdateRoomAppearance(RoomController room)
    {
        if (tiles.ContainsKey(room.gridPosition))
        {
            UpdateTileAppearance(room);
        }
    }

    public void ResetMap()
    {
        foreach (var tile in tiles.Values)
            Destroy(tile.gameObject);

        tiles.Clear();
        currentRoom = null;
    }
}