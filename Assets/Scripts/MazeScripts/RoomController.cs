using UnityEngine;
using System.Collections.Generic;

public enum EnemySpawnType
{
    PoisonOnly,
    LavaOnly,
    Mixed
}


public class RoomController : MonoBehaviour
{
    [Header("Enemy Settings")]
public EnemySpawnType enemySpawnType = EnemySpawnType.PoisonOnly;

    public Transform northDoor;
    public Transform southDoor;
    public Transform eastDoor;
    public Transform westDoor;

    public Collider2D roomBounds;
    public Transform roomCenter;

    public Vector2Int gridPosition;

    public List<EnemyStatsMaze> enemiesInRoom = new List<EnemyStatsMaze>();
public bool isCombatRoom = false;
public bool isCleared = false;
public int totalEnemiesSpawned = 0;




    

}
