using UnityEngine;
using System.Collections;

public class EnemyStatsMaze : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 100;
    public int currentHealth;

    public RoomController assignedRoom;


    private Animator animator;
    private bool isDead = false;


    void Awake()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
    }

    public void EnemyDamaged(int amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        Debug.Log("Slime took damage: " + amount);


        if (currentHealth <= 0)
        {
            OnHealthDepleted();
        }
        else
        {
            animator.SetTrigger("Hurt");
        }

    }

   void OnHealthDepleted()
{
    if (isDead) return;
    isDead = true;

    Debug.Log("Enemy Died");

    // 🔴 STOP all other animation interference
    animator.ResetTrigger("Hurt");
    animator.SetFloat("Speed", 0f);

    // 🔴 CRITICAL: Disable movement script so it stops updating animator
    EnemyMovementTopDown movement = GetComponent<EnemyMovementTopDown>();
    if (movement != null)
        movement.enabled = false;

    // Now set death animation
    if (!animator.GetBool("Death"))
{
    animator.SetBool("Death", true);
}


    if (assignedRoom != null)
{
    assignedRoom.enemiesInRoom.Remove(this);

    int remaining = assignedRoom.enemiesInRoom.Count;
    int total = assignedRoom.totalEnemiesSpawned;

    // 🔥 LIVE UI UPDATE
    if (MazeUIController.Instance != null && assignedRoom.isCombatRoom)
    {
        MazeUIController.Instance.UpdateEnemyCount(remaining, total);
    }

    if (remaining == 0)
    {
        RoomManager.Instance.OnRoomCleared(assignedRoom);
    }
}


    StartCoroutine(DisableAfterDeath());
}


IEnumerator DisableAfterDeath()
{
    // Wait one frame so animator actually enters Death state
    yield return null;


    yield return new WaitForSeconds(1.0f);
    gameObject.SetActive(false);
}


}
