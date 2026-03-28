using UnityEngine;

public class PlayerSpawnPoint : MonoBehaviour
{
    [SerializeField] private Vector3 spawnPosition;

    public void Initailise()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            player.transform.position = spawnPosition;
        else
            Debug.LogWarning("PlayerSpawnPoint: No GameObject with tag 'Player' found!");
    }
}